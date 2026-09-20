using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Enums;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Services.Order;

namespace VendorGateway.Tests.Order
{
    // =====================================================================
    // Shared test data builders
    // =====================================================================
    public static class TestData
    {
        public static Application.Entities.Product Product(int id, string category, float price) => new()
        {
            Id = id,
            Title = $"Product {id}",
            Category = category,
            Price = price,
            Description = "desc",
            Image = "img"
        };

        public static OrderDetails.Order Order(
            int id,
            int accountId,
            OrderStatus status,
            List<OrderDetails.OrderItem> items) => new()
            {
                Id = id,
                AccountId = accountId,
                Status = status,
                Items = items
            };

        public static OrderDetails.OrderItem OrderItem(int productId, int quantity, float unitPrice) => new()
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    // =====================================================================
    // CreateOrderService
    // =====================================================================
    public class CreateOrderServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IProductQueries> _productQueriesMock;
        private readonly Mock<IOrderQueries> _orderQueriesMock;
        private readonly Mock<IOrderCommands> _orderCommandsMock;
        private readonly CreateOrderService _sut;

        public CreateOrderServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _productQueriesMock = _fixture.Freeze<Mock<IProductQueries>>();
            _orderQueriesMock = _fixture.Freeze<Mock<IOrderQueries>>();
            _orderCommandsMock = _fixture.Freeze<Mock<IOrderCommands>>();
            _sut = new CreateOrderService(
                _accountExistenceGuardMock.Object,
                _productQueriesMock.Object,
                _orderQueriesMock.Object,
                _orderCommandsMock.Object);
        }

        private void SetupAccountExists(int accountId) =>
            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

        private void SetupAccountDoesNotExist(int accountId) =>
            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {accountId} not found.")));

        private void SetupProducts(params Application.Entities.Product[] products) =>
            _productQueriesMock
                .Setup(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products.ToList());

        private void SetupNoPendingOrders(int accountId) =>
            _orderQueriesMock
                .Setup(q => q.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        [Fact]
        public async Task CreateAsync_HappyPath_NoDiscount_PersistsExpectedItems()
        {
            var accountId = _fixture.Create<int>();
            var idempotencyKey = Guid.NewGuid();
            var womens = TestData.Product(10, "women's clothing", 20f);
            var jewelery = TestData.Product(20, "jewelery", 100f);

            var request = new OrderRequest.CreateOrder(new List<OrderRequest.OrderItems>
            {
                new(womens.Id, 2), // below the 5-unit discount threshold
                new(jewelery.Id, 1)
            });

            SetupAccountExists(accountId);
            SetupProducts(womens, jewelery);
            SetupNoPendingOrders(accountId);

            List<OrderDetails.OrderItem>? capturedItems = null;
            _orderCommandsMock
                .Setup(c => c.CreateAsync(accountId, idempotencyKey, It.IsAny<List<OrderDetails.OrderItem>>(), It.IsAny<CancellationToken>()))
                .Callback<int, Guid, List<OrderDetails.OrderItem>, CancellationToken>((_, _, items, _) => capturedItems = items)
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.CreateAsync(accountId, idempotencyKey, request, cts.Token);

            result.IsSuccess.Should().BeTrue();
            capturedItems.Should().BeEquivalentTo(new[]
            {
                new { ProductId = womens.Id, Quantity = 2, UnitPrice = 20f, ItemId = 1 },
                new { ProductId = jewelery.Id, Quantity = 1, UnitPrice = 100f, ItemId = 2 }
            });

            _orderCommandsMock.Verify(
                c => c.CreateAsync(accountId, idempotencyKey, It.IsAny<List<OrderDetails.OrderItem>>(), cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WomensClothingQuantityMeetsThreshold_AppliesDiscountToJeweleryItemsOnly()
        {
            var accountId = _fixture.Create<int>();
            var idempotencyKey = Guid.NewGuid();
            var womens = TestData.Product(10, "women's clothing", 20f);
            var jewelery = TestData.Product(20, "jewelery", 100f);

            var request = new OrderRequest.CreateOrder(new List<OrderRequest.OrderItems>
            {
                new(womens.Id, 5), // meets the >= 5 threshold
                new(jewelery.Id, 1)
            });

            SetupAccountExists(accountId);
            SetupProducts(womens, jewelery);
            SetupNoPendingOrders(accountId);

            List<OrderDetails.OrderItem>? capturedItems = null;
            _orderCommandsMock
                .Setup(c => c.CreateAsync(accountId, idempotencyKey, It.IsAny<List<OrderDetails.OrderItem>>(), It.IsAny<CancellationToken>()))
                .Callback<int, Guid, List<OrderDetails.OrderItem>, CancellationToken>((_, _, items, _) => capturedItems = items)
                .ReturnsAsync(Result.Success());

            await _sut.CreateAsync(accountId, idempotencyKey, request, CancellationToken.None);

            capturedItems.Should().NotBeNull();
            capturedItems!.Single(i => i.ProductId == womens.Id).UnitPrice.Should().Be(20f); // not jewelery -> untouched
            capturedItems!.Single(i => i.ProductId == jewelery.Id).UnitPrice.Should().BeApproximately(90f, 0.001f); // 100 * 0.9
        }

        [Fact]
        public async Task CreateAsync_AccountDoesNotExist_ReturnsNotFoundResult_AndNeverChecksProductsOrOrders()
        {
            var accountId = _fixture.Create<int>();
            var request = new OrderRequest.CreateOrder(new List<OrderRequest.OrderItems> { new(1, 1) });

            SetupAccountDoesNotExist(accountId);

            var result = await _sut.CreateAsync(accountId, Guid.NewGuid(), request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(accountId.ToString());

            _productQueriesMock.Verify(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderQueriesMock.Verify(q => q.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderCommandsMock.Verify(c => c.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<List<OrderDetails.OrderItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_RequestedProductDoesNotExist_ReturnsNotFoundResult_AndNeverChecksOrdersOrCreates()
        {
            var accountId = _fixture.Create<int>();
            var request = new OrderRequest.CreateOrder(new List<OrderRequest.OrderItems> { new(999, 1) });

            SetupAccountExists(accountId);
            SetupProducts(); // no products returned -> 999 is missing

            var result = await _sut.CreateAsync(accountId, Guid.NewGuid(), request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain("999");

            _orderQueriesMock.Verify(q => q.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderCommandsMock.Verify(c => c.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<List<OrderDetails.OrderItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_PendingOrderAlreadyHasOneOfTheProducts_ReturnsConflictResult_AndNeverCreates()
        {
            var accountId = _fixture.Create<int>();
            var product = TestData.Product(10, "electronics", 50f);
            var request = new OrderRequest.CreateOrder(new List<OrderRequest.OrderItems> { new(product.Id, 1) });

            SetupAccountExists(accountId);
            SetupProducts(product);

            var pendingOrder = TestData.Order(
                id: 1,
                accountId: accountId,
                status: OrderStatus.Pending,
                items: [TestData.OrderItem(product.Id, 1, 50f)]);

            _orderQueriesMock
                .Setup(q => q.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync([pendingOrder]);

            var result = await _sut.CreateAsync(accountId, Guid.NewGuid(), request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Conflict);

            _orderCommandsMock.Verify(
                c => c.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<List<OrderDetails.OrderItem>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_PendingOrderExistsButWithDifferentProduct_ReturnsSuccessResult()
        {
            var accountId = _fixture.Create<int>();
            var product = TestData.Product(10, "electronics", 50f);
            var request = new OrderRequest.CreateOrder(new List<OrderRequest.OrderItems> { new(product.Id, 1) });

            SetupAccountExists(accountId);
            SetupProducts(product);

            var unrelatedPendingOrder = TestData.Order(
                id: 1,
                accountId: accountId,
                status: OrderStatus.Pending,
                items: [TestData.OrderItem(999, 1, 10f)]); // different product

            _orderQueriesMock
                .Setup(q => q.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync([unrelatedPendingOrder]);

            _orderCommandsMock
                .Setup(c => c.CreateAsync(accountId, It.IsAny<Guid>(), It.IsAny<List<OrderDetails.OrderItem>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            var result = await _sut.CreateAsync(accountId, Guid.NewGuid(), request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }

    // =====================================================================
    // DeleteOrderService
    // =====================================================================
    public class DeleteOrderServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IOrderCommands> _orderCommandsMock;
        private readonly DeleteOrderService _sut;

        public DeleteOrderServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _orderCommandsMock = _fixture.Freeze<Mock<IOrderCommands>>();
            _sut = new DeleteOrderService(_accountExistenceGuardMock.Object, _orderCommandsMock.Object);
        }

        [Fact]
        public async Task DeleteAsync_AccountExists_DeletesOrder()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _orderCommandsMock
                .Setup(c => c.DeleteByIdAsync(accountId, orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.DeleteAsync(accountId, orderId, cts.Token);

            result.IsSuccess.Should().BeTrue();
            _orderCommandsMock.Verify(c => c.DeleteByIdAsync(accountId, orderId, cts.Token), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_AccountDoesNotExist_ReturnsNotFoundResult_AndNeverDeletes()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {accountId} not found.")));

            var result = await _sut.DeleteAsync(accountId, orderId, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(accountId.ToString());

            _orderCommandsMock.Verify(c => c.DeleteByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_OrderAlreadySubmitted_ReturnsConflictResultFromOrderCommands()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _orderCommandsMock
                .Setup(c => c.DeleteByIdAsync(accountId, orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.Conflict("already submitted")));

            var result = await _sut.DeleteAsync(accountId, orderId, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Conflict);
            result.Error.Message.Should().Be("already submitted");
        }
    }

    // =====================================================================
    // ExecuteOrderService
    // =====================================================================
    public class ExecuteOrderServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IOrderQueries> _orderQueriesMock;
        private readonly Mock<IOrderCommands> _orderCommandsMock;
        private readonly ExecuteOrderService _sut;

        public ExecuteOrderServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _orderQueriesMock = _fixture.Freeze<Mock<IOrderQueries>>();
            _orderCommandsMock = _fixture.Freeze<Mock<IOrderCommands>>();
            _sut = new ExecuteOrderService(_accountExistenceGuardMock.Object, _orderQueriesMock.Object, _orderCommandsMock.Object);
        }

        private void SetupAccountExists(int accountId) =>
            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

        [Fact]
        public async Task ExecuteAsync_OrderExists_ExecutesUsingOrderAccountAndOrderId()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var order = TestData.Order(orderId, accountId, OrderStatus.Pending, []);

            SetupAccountExists(accountId);
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync([order]);

            _orderCommandsMock
                .Setup(c => c.ExecuteAsync(order.AccountId, order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.ExecuteAsync(accountId, orderId, cts.Token);

            result.IsSuccess.Should().BeTrue();
            _orderCommandsMock.Verify(c => c.ExecuteAsync(order.AccountId, order.Id, cts.Token), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_AccountDoesNotExist_ReturnsNotFoundResult_AndNeverQueriesOrders()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {accountId} not found.")));

            var result = await _sut.ExecuteAsync(accountId, orderId, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(accountId.ToString());

            _orderQueriesMock.Verify(q => q.GetByIdsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderCommandsMock.Verify(c => c.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_OrderDoesNotExist_ReturnsNotFoundResult_AndNeverExecutes()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            SetupAccountExists(accountId);
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _sut.ExecuteAsync(accountId, orderId, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(orderId.ToString());

            _orderCommandsMock.Verify(c => c.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // NOTE: this documents the ACTUAL current behavior rather than the intended
        // "is not unique" NotFound-result path. Because the code only reaches
        // IsUniqueOrder() after results.SingleOrDefault(), a query returning more
        // than one matching order throws .NET's native InvalidOperationException
        // ("Sequence contains more than one element") before the custom check ever
        // runs — meaning that custom check is effectively dead code as written.
        // This is a genuinely unexpected failure (not an anticipated domain outcome),
        // so it still propagates as an exception rather than a Result.
        [Fact]
        public async Task ExecuteAsync_QueryReturnsMoreThanOneOrder_ThrowsInvalidOperationException_FromSingleOrDefault()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var duplicateOrders = new List<OrderDetails.Order>
            {
                TestData.Order(orderId, accountId, OrderStatus.Pending, []),
                TestData.Order(orderId, accountId, OrderStatus.Pending, [])
            };

            SetupAccountExists(accountId);
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(duplicateOrders);

            var act = () => _sut.ExecuteAsync(accountId, orderId, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            _orderCommandsMock.Verify(c => c.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    // =====================================================================
    // GetOrderService
    // =====================================================================
    public class GetOrderServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IOrderQueries> _orderQueriesMock;
        private readonly GetOrderService _sut;

        public GetOrderServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _orderQueriesMock = _fixture.Freeze<Mock<IOrderQueries>>();
            _sut = new GetOrderService(_accountExistenceGuardMock.Object, _orderQueriesMock.Object);
        }

        private void SetupAccountExists(int accountId) =>
            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

        [Fact]
        public async Task GetAsync_OrderExists_ReturnsIt()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var order = TestData.Order(orderId, accountId, OrderStatus.Pending, []);

            SetupAccountExists(accountId);
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync([order]);

            using var cts = new CancellationTokenSource();

            var result = await _sut.GetAsync(accountId, orderId, cts.Token);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(order);
            _orderQueriesMock.Verify(q => q.GetByIdsAsync(accountId, new[] { orderId }, cts.Token), Times.Once);
        }

        [Fact]
        public async Task GetAsync_AccountDoesNotExist_ReturnsNotFoundResult_AndNeverQueriesOrders()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {accountId} not found.")));

            var result = await _sut.GetAsync(accountId, orderId, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(accountId.ToString());

            _orderQueriesMock.Verify(q => q.GetByIdsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_OrderDoesNotExist_ReturnsNotFoundResult()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();

            SetupAccountExists(accountId);
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _sut.GetAsync(accountId, orderId, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(orderId.ToString());
        }
    }

    // =====================================================================
    // UpdateOrderService
    // =====================================================================
    public class UpdateOrderServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IProductQueries> _productQueriesMock;
        private readonly Mock<IOrderQueries> _orderQueriesMock;
        private readonly Mock<IOrderCommands> _orderCommandsMock;
        private readonly UpdateOrderService _sut;

        public UpdateOrderServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _productQueriesMock = _fixture.Freeze<Mock<IProductQueries>>();
            _orderQueriesMock = _fixture.Freeze<Mock<IOrderQueries>>();
            _orderCommandsMock = _fixture.Freeze<Mock<IOrderCommands>>();
            _sut = new UpdateOrderService(
                _accountExistenceGuardMock.Object,
                _productQueriesMock.Object,
                _orderQueriesMock.Object,
                _orderCommandsMock.Object);
        }

        private void SetupAccountExists(int accountId) =>
            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

        private void SetupProducts(params Application.Entities.Product[] products) =>
            _productQueriesMock
                .Setup(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products.ToList());

        private void SetupOrder(int accountId, int orderId, OrderDetails.Order order) =>
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync([order]);

        [Fact]
        public async Task UpdateAsync_HappyPath_UpdatesQuantitiesAndAppliesJeweleryDiscountWhenThresholdMet()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var womens = TestData.Product(10, "women's clothing", 20f);
            var jewelery = TestData.Product(20, "jewelery", 100f);

            var request = new OrderRequest.UpdateOrder(new List<OrderRequest.OrderItems>
            {
                new(womens.Id, 5), // meets threshold
                new(jewelery.Id, 3)
            });

            var existingItem1 = TestData.OrderItem(womens.Id, 1, 20f);
            var existingItem2 = TestData.OrderItem(jewelery.Id, 1, 100f);
            var order = TestData.Order(orderId, accountId, OrderStatus.Pending, [existingItem1, existingItem2]);

            SetupAccountExists(accountId);
            SetupProducts(womens, jewelery);
            SetupOrder(accountId, orderId, order);

            _orderCommandsMock
                .Setup(c => c.UpdateAsync(accountId, order, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.UpdateAsync(accountId, orderId, request, cts.Token);

            result.IsSuccess.Should().BeTrue();
            existingItem1.Quantity.Should().Be(5);
            existingItem1.UnitPrice.Should().Be(20f); // not jewelery -> price untouched

            existingItem2.Quantity.Should().Be(3);
            existingItem2.UnitPrice.Should().BeApproximately(90f, 0.001f); // jewelery + discount applied

            _orderCommandsMock.Verify(c => c.UpdateAsync(accountId, order, cts.Token), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WomensClothingQuantityBelowThreshold_DoesNotDiscountJeweleryItems()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var womens = TestData.Product(10, "women's clothing", 20f);
            var jewelery = TestData.Product(20, "jewelery", 100f);

            var request = new OrderRequest.UpdateOrder(new List<OrderRequest.OrderItems>
            {
                new(womens.Id, 2), // below threshold
                new(jewelery.Id, 1)
            });

            var existingItem = TestData.OrderItem(jewelery.Id, 1, 100f);
            var order = TestData.Order(orderId, accountId, OrderStatus.Pending,
                [TestData.OrderItem(womens.Id, 1, 20f), existingItem]);

            SetupAccountExists(accountId);
            SetupProducts(womens, jewelery);
            SetupOrder(accountId, orderId, order);

            _orderCommandsMock
                .Setup(c => c.UpdateAsync(accountId, order, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            await _sut.UpdateAsync(accountId, orderId, request, CancellationToken.None);

            existingItem.UnitPrice.Should().Be(100f); // unchanged
        }

        [Fact]
        public async Task UpdateAsync_AccountDoesNotExist_ReturnsNotFoundResult_AndNeverChecksProductsOrOrder()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var request = new OrderRequest.UpdateOrder(new List<OrderRequest.OrderItems> { new(1, 1) });

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {accountId} not found.")));

            var result = await _sut.UpdateAsync(accountId, orderId, request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(accountId.ToString());

            _productQueriesMock.Verify(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderQueriesMock.Verify(q => q.GetByIdsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_RequestedProductDoesNotExist_ReturnsNotFoundResult_AndNeverQueriesOrder()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var request = new OrderRequest.UpdateOrder(new List<OrderRequest.OrderItems> { new(999, 1) });

            SetupAccountExists(accountId);
            SetupProducts(); // 999 missing

            var result = await _sut.UpdateAsync(accountId, orderId, request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain("999");

            _orderQueriesMock.Verify(q => q.GetByIdsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderCommandsMock.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<OrderDetails.Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_OrderDoesNotExist_ReturnsNotFoundResult()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var product = TestData.Product(1, "electronics", 10f);
            var request = new OrderRequest.UpdateOrder(new List<OrderRequest.OrderItems> { new(product.Id, 1) });

            SetupAccountExists(accountId);
            SetupProducts(product);
            _orderQueriesMock
                .Setup(q => q.GetByIdsAsync(accountId, new[] { orderId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _sut.UpdateAsync(accountId, orderId, request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(orderId.ToString());

            _orderCommandsMock.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<OrderDetails.Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_OrderAlreadySubmitted_ReturnsConflictResult_AndNeverUpdates()
        {
            var accountId = _fixture.Create<int>();
            var orderId = _fixture.Create<int>();
            var product = TestData.Product(1, "electronics", 10f);
            var request = new OrderRequest.UpdateOrder(new List<OrderRequest.OrderItems> { new(product.Id, 1) });

            var submittedOrder = TestData.Order(orderId, accountId, OrderStatus.Submitted,
                [TestData.OrderItem(product.Id, 1, 10f)]);

            SetupAccountExists(accountId);
            SetupProducts(product);
            SetupOrder(accountId, orderId, submittedOrder);

            var result = await _sut.UpdateAsync(accountId, orderId, request, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Conflict);
            result.Error.Message.Should().Contain("executed");

            _orderCommandsMock.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<OrderDetails.Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
