using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using System.Net;
using VendorGateway.Application.Common;
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

            _accountCommandsMock
                .Setup(c => c.CreateAsync(id, request.email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.CreateAsync(request, id, cts.Token);

            result.IsSuccess.Should().BeTrue();
            _accountCommandsMock.Verify(
                c => c.CreateAsync(id, request.email, cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_VendorReturnsIdZero_ReturnsConflictResult_AndNeverPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = _fixture.Create<CreateAccountRequest>();
            var vendorResponse = new CreateAccountVendorResponse(0, request.email);

            _apiClientMock
                .Setup(c => c.CreateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendorResponse);

            var result = await _sut.CreateAsync(request, id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Conflict);
            result.Error.Message.Should().Contain(id.ToString());

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
                .ReturnsAsync(Result.Success());

            _apiClientMock
                .Setup(c => c.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(okResponse);

            _accountCommandsMock
                .Setup(c => c.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.DeleteAsync(id, cts.Token);

            result.IsSuccess.Should().BeTrue();
            _apiClientMock.Verify(c => c.DeleteAsync(id, cts.Token), Times.Once);
            _accountCommandsMock.Verify(c => c.DeleteAsync(id, cts.Token), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_AccountDoesNotExist_ReturnsNotFoundResult_AndNeverCallsVendorOrDeletesLocally()
        {
            var id = _fixture.Create<int>();

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {id} not found.")));

            var result = await _sut.DeleteAsync(id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(id.ToString());

            _apiClientMock.Verify(
                c => c.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _accountCommandsMock.Verify(
                c => c.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_VendorDeleteReturnsNonOkStatus_ReturnsValidationResult_AndNeverDeletesLocally()
        {
            var id = _fixture.Create<int>();
            var failedResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _apiClientMock
                .Setup(c => c.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponse);

            var result = await _sut.DeleteAsync(id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Validation);
            result.Error.Message.Should().Contain(id.ToString());

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

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(expected);
            _accountQueriesMock.Verify(q => q.GetByIdsAsync(new[] { id }, cts.Token), Times.Once);
        }

        [Fact]
        public async Task GetAsync_AccountDoesNotExist_ReturnsNotFoundResult()
        {
            var id = _fixture.Create<int>();

            _accountQueriesMock
                .Setup(q => q.GetByIdsAsync(new[] { id }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Application.Entities.Account>());

            var result = await _sut.GetAsync(id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(id.ToString());
        }

        [Fact]
        public async Task GetAsync_QueryReturnsNull_ReturnsNotFoundResult()
        {
            var id = _fixture.Create<int>();

            _accountQueriesMock
                .Setup(q => q.GetByIdsAsync(new[] { id }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Application.Entities.Account>)null!);

            var result = await _sut.GetAsync(id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(id.ToString());
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
                .ReturnsAsync(Result.Success());

            _apiClientMock
                .Setup(c => c.UpdateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendorResponse);

            _accountCommandsMock
                .Setup(c => c.UpdateAsync(id, request.email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            using var cts = new CancellationTokenSource();

            var result = await _sut.UpdateAsync(request, id, cts.Token);

            result.IsSuccess.Should().BeTrue();
            _accountExistenceGuardMock.Verify(
                g => g.EnsureExistsAsync(id, cts.Token),
                Times.Once);
            _apiClientMock.Verify(c => c.UpdateAsync(request, id, cts.Token), Times.Once);
            _accountCommandsMock.Verify(c => c.UpdateAsync(id, request.email, cts.Token), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_AccountDoesNotExistInitially_ReturnsNotFoundResult_AndNeverCallsVendor()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(Error.NotFound($"Account with id {id} not found.")));

            var result = await _sut.UpdateAsync(request, id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.NotFound);
            result.Error.Message.Should().Contain(id.ToString());

            _apiClientMock.Verify(
                c => c.UpdateAsync(It.IsAny<UpdateAccountRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _accountCommandsMock.Verify(
                c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_VendorUpdateReturnsIdZero_ReturnsConflictResult_AndNeverPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _apiClientMock
                .Setup(c => c.UpdateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateAccountVendorResponse(0));

            var result = await _sut.UpdateAsync(request, id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Conflict);

            _accountCommandsMock.Verify(
                c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_VendorUpdateReturnsNull_ReturnsConflictResult_AndNeverPersistsLocally()
        {
            var id = _fixture.Create<int>();
            var request = new UpdateAccountRequest(_fixture.Create<string>());

            _accountExistenceGuardMock
                .Setup(g => g.EnsureExistsAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _apiClientMock
                .Setup(c => c.UpdateAsync(request, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateAccountVendorResponse)null!);

            var result = await _sut.UpdateAsync(request, id, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error!.Category.Should().Be(ErrorCategory.Conflict);

            _accountCommandsMock.Verify(
                c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
