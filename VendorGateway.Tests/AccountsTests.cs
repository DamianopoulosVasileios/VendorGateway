using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using System.Net;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Services.Account;

namespace VendorGateway.Tests.Account
{
    // =====================================================================
    // CreateAccountService
    // =====================================================================
    public class CreateAccountServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountsApiClient> _apiClientMock;
        private readonly Mock<IAccountCommands> _accountCommandsMock;
        private readonly CreateAccountService _sut;

        public CreateAccountServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IAccountsApiClient>>();
            _accountCommandsMock = _fixture.Freeze<Mock<IAccountCommands>>();
            _sut = new CreateAccountService(_apiClientMock.Object, _accountCommandsMock.Object);
        }

        [Fact]
        public async Task CreateAsync_VendorReturnsValidId_PersistsAccountLocally()
        {
            var id = _fixture.Create<int>();
            var request = _fixture.Create<CreateAccountRequest>();
            var vendorResponse = new CreateAccountVendorResponse(id, request.email);

            _apiClientMock
                .Setup(c => c.CreateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendorResponse);

            using var cts = new CancellationTokenSource();

            await _sut.CreateAsync(request, id, cts.Token);

            _accountCommandsMock.Verify(
                c => c.CreateAsync(id, request.email, cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_VendorReturnsIdZero_ThrowsInvalidOperationException_AndNeverPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = _fixture.Create<CreateAccountRequest>();
            var vendorResponse = new CreateAccountVendorResponse(0, request.email);

            _apiClientMock
                .Setup(c => c.CreateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendorResponse);

            var act = () => _sut.CreateAsync(request, id, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{id}*");

            _accountCommandsMock.Verify(
                c => c.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_PropagatesException_WhenVendorApiThrows()
        {
            var id = _fixture.Create<int>();
            var request = _fixture.Create<CreateAccountRequest>();

            _apiClientMock
                .Setup(c => c.CreateAsync(request, id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("vendor unreachable"));

            var act = () => _sut.CreateAsync(request, id, CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();

            _accountCommandsMock.Verify(
                c => c.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    // =====================================================================
    // DeleteAccountService
    // =====================================================================
    public class DeleteAccountServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountsApiClient> _apiClientMock;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IAccountCommands> _accountCommandsMock;
        private readonly DeleteAccountService _sut;

        public DeleteAccountServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IAccountsApiClient>>();
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _accountCommandsMock = _fixture.Freeze<Mock<IAccountCommands>>();
            _sut = new DeleteAccountService(_apiClientMock.Object, _accountExistenceGuardMock.Object, _accountCommandsMock.Object);
        }

        [Fact]
        public async Task DeleteAsync_AccountExistsAndVendorDeleteSucceeds_DeletesLocally()
        {
            var id = _fixture.Create<int>();
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _apiClientMock
                .Setup(c => c.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(okResponse);

            using var cts = new CancellationTokenSource();

            await _sut.DeleteAsync(id, cts.Token);

            _apiClientMock.Verify(c => c.DeleteAsync(id, cts.Token), Times.Once);
            _accountCommandsMock.Verify(c => c.DeleteAsync(id, cts.Token), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_AccountDoesNotExist_ThrowsKeyNotFoundException_AndNeverCallsVendorOrDeletesLocally()
        {
            var id = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Account with id {id} not found."));

            var act = () => _sut.DeleteAsync(id, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{id}*");

            _apiClientMock.Verify(
                c => c.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _accountCommandsMock.Verify(
                c => c.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_VendorDeleteReturnsNonOkStatus_ThrowsInvalidDataException_AndNeverDeletesLocally()
        {
            var id = _fixture.Create<int>();
            var failedResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _apiClientMock
                .Setup(c => c.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponse);

            var act = () => _sut.DeleteAsync(id, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage($"*{id}*");

            _accountCommandsMock.Verify(
                c => c.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    // =====================================================================
    // GetAccountService
    // =====================================================================
    public class GetAccountServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountQueries> _accountQueriesMock;
        private readonly GetAccountService _sut;

        public GetAccountServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountQueriesMock = _fixture.Freeze<Mock<IAccountQueries>>();
            _sut = new GetAccountService(_accountQueriesMock.Object);
        }

        [Fact]
        public async Task GetAsync_AccountExists_ReturnsIt()
        {
            var id = _fixture.Create<int>();
            var expected = new Application.Entities.Account { Id = id };

            _accountQueriesMock
                .Setup(q => q.GetByIdsAsync(new[] { id }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Application.Entities.Account> { expected });

            using var cts = new CancellationTokenSource();

            var result = await _sut.GetAsync(id, cts.Token);

            result.Should().BeSameAs(expected);
            _accountQueriesMock.Verify(q => q.GetByIdsAsync(new[] { id }, cts.Token), Times.Once);
        }

        [Fact]
        public async Task GetAsync_AccountDoesNotExist_ThrowsKeyNotFoundException()
        {
            var id = _fixture.Create<int>();

            _accountQueriesMock
                .Setup(q => q.GetByIdsAsync(new[] { id }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Application.Entities.Account>());

            var act = () => _sut.GetAsync(id, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{id}*");
        }

        [Fact]
        public async Task GetAsync_QueryReturnsNull_ThrowsKeyNotFoundException()
        {
            var id = _fixture.Create<int>();

            _accountQueriesMock
                .Setup(q => q.GetByIdsAsync(new[] { id }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Application.Entities.Account>)null!);

            var act = () => _sut.GetAsync(id, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{id}*");
        }
    }

    // =====================================================================
    // UpdateAccountService
    // =====================================================================
    public class UpdateAccountServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountExistenceGuard> _accountExistenceGuardMock;
        private readonly Mock<IAccountsApiClient> _apiClientMock;
        private readonly Mock<IAccountCommands> _accountCommandsMock;
        private readonly UpdateAccountService _sut;

        public UpdateAccountServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _accountExistenceGuardMock = _fixture.Freeze<Mock<IAccountExistenceGuard>>();
            _apiClientMock = _fixture.Freeze<Mock<IAccountsApiClient>>();
            _accountCommandsMock = _fixture.Freeze<Mock<IAccountCommands>>();
            _sut = new UpdateAccountService(_accountExistenceGuardMock.Object, _apiClientMock.Object, _accountCommandsMock.Object);
        }

        [Fact]
        public async Task UpdateAsync_HappyPath_UpdatesVendorThenPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());
            var vendorResponse = new UpdateAccountVendorResponse(id);

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _apiClientMock
                .Setup(c => c.UpdateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendorResponse);

            using var cts = new CancellationTokenSource();

            await _sut.UpdateAsync(request, id, cts.Token);

            _accountExistenceGuardMock.Verify(
                g => g.EnsureExistsAsync(id, cts.Token),
                Times.Once);
            _apiClientMock.Verify(c => c.UpdateAsync(request, id, cts.Token), Times.Once);
            _accountCommandsMock.Verify(c => c.UpdateAsync(id, request.email, cts.Token), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_AccountDoesNotExistInitially_ThrowsKeyNotFoundException_AndNeverCallsVendor()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Account with id {id} not found."));

            var act = () => _sut.UpdateAsync(request, id, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{id}*");

            _apiClientMock.Verify(
                c => c.UpdateAsync(It.IsAny<UpdateAccountRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _accountCommandsMock.Verify(
                c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_VendorUpdateReturnsIdZero_ThrowsInvalidOperationException_AndNeverPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _apiClientMock
                .Setup(c => c.UpdateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateAccountVendorResponse(0));

            var act = () => _sut.UpdateAsync(request, id, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            _accountCommandsMock.Verify(
                c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_VendorUpdateReturnsNull_ThrowsInvalidOperationException_AndNeverPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _apiClientMock
                .Setup(c => c.UpdateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateAccountVendorResponse)null!);

            var act = () => _sut.UpdateAsync(request, id, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            _accountCommandsMock.Verify(
                c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
