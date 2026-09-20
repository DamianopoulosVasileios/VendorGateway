using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Mappers;
using VendorGateway.Application.Services.Product;

namespace VendorGateway.Tests.Product
{
    // =====================================================================
    // CreateProductService
    // =====================================================================
    public class CreateProductServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductsApiClient> _apiClientMock;
        private readonly Mock<IProductCommands> _productCommandsMock;
        private readonly CreateProductService _sut;

        public CreateProductServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IProductsApiClient>>();
            _productCommandsMock = _fixture.Freeze<Mock<IProductCommands>>();
            _sut = new CreateProductService(_apiClientMock.Object, _productCommandsMock.Object);
        }

        // NOTE: assumes GetProductsResponse is a positional record shaped like
        // (int id, string Title, float Price, string Description, string Category, string Image),
        // matching the pattern used by your other vendor response DTOs. Adjust the
        // constructor call below if the real shape differs.
        private static GetProductsResponse VendorProduct(int id) => new(
            id,
            $"Title {id}",
            9.99f * id,
            $"Description {id}",
            "electronics",
            $"https://example.com/{id}.png");

        [Fact]
        public async Task CreateAsync_AddRangeSucceeds_ReturnsSuccessResultWithTrueValue_AndPersistsCorrectlyMappedProducts()
        {
            var vendorProducts = new List<GetProductsResponse> { VendorProduct(1), VendorProduct(2) };

            _apiClientMock
                .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendorProducts);

            IEnumerable<Application.Entities.Product>? capturedProducts = null;
            _productCommandsMock
                .Setup(c => c.UpdateRangeAsync(It.IsAny<IEnumerable<Application.Entities.Product>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Application.Entities.Product>, CancellationToken>((products, _) => capturedProducts = products)
                .ReturnsAsync(true);

            using var cts = new CancellationTokenSource();

            var result = await _sut.UpdateAsync(cts.Token);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();

            var expectedMapped = ProductMappers.Map(vendorProducts);
            capturedProducts.Should().BeEquivalentTo(expectedMapped);

            _productCommandsMock.Verify(
                c => c.UpdateRangeAsync(It.IsAny<IEnumerable<Application.Entities.Product>>(), cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_AddRangeReturnsFalse_ReturnsSuccessResultWithFalseValue()
        {
            _apiClientMock
                .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([VendorProduct(1)]);

            _productCommandsMock
                .Setup(c => c.UpdateRangeAsync(It.IsAny<IEnumerable<Application.Entities.Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _sut.UpdateAsync(CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }


        [Fact]
        public async Task CreateAsync_GetAllAsyncThrows_PropagatesException_AndNeverCallsAddRange()
        {
            _apiClientMock
                .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("vendor unreachable"));

            var act = () => _sut.UpdateAsync(CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();

            _productCommandsMock.Verify(
                c => c.UpdateRangeAsync(It.IsAny<IEnumerable<Application.Entities.Product>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_NoProductsFromVendor_CallsAddRangeWithEmptyCollection()
        {
            _apiClientMock
                .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _productCommandsMock
                .Setup(c => c.UpdateRangeAsync(It.IsAny<IEnumerable<Application.Entities.Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.UpdateAsync(CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _productCommandsMock.Verify(
                c => c.UpdateRangeAsync(It.Is<IEnumerable<Application.Entities.Product>>(p => !p.Any()), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    // =====================================================================
    // DeleteProductService
    // =====================================================================
    public class DeleteProductServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductCommands> _productCommandsMock;
        private readonly DeleteProductService _sut;

        public DeleteProductServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _productCommandsMock = _fixture.Freeze<Mock<IProductCommands>>();
            _sut = new DeleteProductService(_productCommandsMock.Object);
        }

        [Fact]
        public async Task DeleteAsync_CallsProductCommandsDeleteAsync_WithSameCancellationToken()
        {
            using var cts = new CancellationTokenSource();

            var result = await _sut.DeleteAsync(cts.Token);

            result.IsSuccess.Should().BeTrue();
            _productCommandsMock.Verify(c => c.DeleteAsync(cts.Token), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_PropagatesException_WhenProductCommandsThrows()
        {
            _productCommandsMock
                .Setup(c => c.DeleteAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("failed to delete products"));

            var act = () => _sut.DeleteAsync(CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("failed to delete products");
        }
    }

    // =====================================================================
    // GetProductService
    // =====================================================================
    public class GetProductServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductQueries> _productQueriesMock;
        private readonly GetProductService _sut;

        public GetProductServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _productQueriesMock = _fixture.Freeze<Mock<IProductQueries>>();
            _sut = new GetProductService(_productQueriesMock.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsWhateverProductQueriesReturns()
        {
            var expected = _fixture.CreateMany<Application.Entities.Product>(3).ToList();

            using var cts = new CancellationTokenSource();

            _productQueriesMock
                .Setup(q => q.GetAsync(cts.Token))
                .ReturnsAsync(expected);

            var result = await _sut.GetAsync(cts.Token);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(expected);
            _productQueriesMock.Verify(q => q.GetAsync(cts.Token), Times.Once);
        }

        [Fact]
        public async Task GetAsync_PropagatesException_WhenProductQueriesThrows()
        {
            _productQueriesMock
                .Setup(q => q.GetAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db unavailable"));

            var act = () => _sut.GetAsync(CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("db unavailable");
        }
    }
}
