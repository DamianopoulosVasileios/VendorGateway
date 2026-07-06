using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using VendorGateway.API;
using VendorGateway.APIs;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Configuration;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;
using VendorGateway.Contracts.Product.Responses;
using VendorGateway.Enums;
using VendorGateway.Mappers;

namespace VendorGateway.Tests
{
    public class FakeStoreApiClientTests
    {
        #region Setup
        private readonly IFixture _fixture;
        private readonly FakeStoreAccountsApiClient _sutAccounts;
        private readonly FakeStoreProductsApiClient _sutProducts;

        private readonly Mock<IApiResponseReader> _readerMock;
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;

        private readonly VendorDetails _vendorDetails;
        private readonly HttpClient _httpClient;

        public FakeStoreApiClientTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _vendorDetails = new VendorDetails
            {
                Name = "FakeStore",
                ApiUrl = "https://fakestoreapi.com",
                Users = new UsersEndpoints
                {
                    Get = "/users/{id}",
                    Create = "/users",
                    Update = "/users/{id}",
                    Delete = "/users/{id}"
                },
                Products = new ProductsEndpoints
                {
                    GetAll = "/products"
                }
            };

            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri(_vendorDetails.ApiUrl)
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock
                .Setup(f => f.CreateClient(Vendors.FakeStore.ToString()))
                .Returns(_httpClient);

            _readerMock = new Mock<IApiResponseReader>();

            var configuration = BuildConfiguration(_vendorDetails);

            _sutAccounts = new FakeStoreAccountsApiClient(_readerMock.Object, _factoryMock.Object, configuration);
            _sutProducts = new FakeStoreProductsApiClient(_readerMock.Object, _factoryMock.Object, configuration);
        }
        #endregion

        #region Accounts
        // ---------------------------------------------------------------
        // GetAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task GetAsync_SendsGetRequest_ToResolvedUrl()
        {
            var id = _fixture.Create<int>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeStoreResponse = _fixture.Create<FakeStoreGetAccountResponse>();

            HttpRequestMessage capturedRequest = null;
            SetupHandler(httpResponse, req => capturedRequest = req);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreGetAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            using var cts = new CancellationTokenSource();

            await _sutAccounts.GetByIdAsync(id, cts.Token);

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Get);
            capturedRequest.RequestUri!.AbsolutePath.Should().Be($"/users/{id}");
        }

        [Fact]
        public async Task GetAsync_ReturnsMappedResponse_FromReader()
        {
            var id = _fixture.Create<int>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeStoreResponse = _fixture.Create<FakeStoreGetAccountResponse>();

            var expected = AccountMappers.ToApi(fakeStoreResponse);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreGetAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            using var cts = new CancellationTokenSource();

            var result = await _sutAccounts.GetByIdAsync(id, cts.Token);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetAsync_PropagatesException_WhenReaderThrows()
        {
            var id = _fixture.Create<int>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreGetAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("boom"));

            var act = () => _sutAccounts.GetByIdAsync(id, CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        // ---------------------------------------------------------------
        // CreateAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_SendsPostRequest_ToCreateUrl_WithMappedBody()
        {
            var apiRequest = _fixture.Create<CreateAccountRequest>();
            var fakeStoreResponse = _fixture.Create<FakeStoreCreateAccountResponse>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var expectedBody = AccountMappers.ToFakeStore(apiRequest);

            HttpRequestMessage capturedRequest = null;
            SetupHandler(httpResponse, req => capturedRequest = req);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreCreateAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            await _sutAccounts.CreateAsync(apiRequest, CancellationToken.None);

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Post);
            capturedRequest.RequestUri!.AbsolutePath.Should().Be("/users");

            var sentBody = await capturedRequest.Content!.ReadFromJsonAsync<FakeStoreCreateAccountRequest>();
            sentBody.Should().BeEquivalentTo(expectedBody);
        }

        [Fact]
        public async Task CreateAsync_ReturnsMappedResponse_FromReader()
        {
            var apiRequest = _fixture.Create<CreateAccountRequest>();
            var fakeStoreResponse = _fixture.Create<FakeStoreCreateAccountResponse>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var expected = AccountMappers.ToApi(fakeStoreResponse);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreCreateAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            var result = await _sutAccounts.CreateAsync(apiRequest, CancellationToken.None);

            result.Should().BeEquivalentTo(expected);
        }

        // ---------------------------------------------------------------
        // UpdateAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_SendsPutRequest_ToResolvedUrl_WithMappedBody()
        {
            var id = _fixture.Create<int>();
            var apiRequest = _fixture.Create<UpdateAccountRequest>();
            var fakeStoreResponse = _fixture.Create<FakeStoreUpdateAccountResponse>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var expectedBody = AccountMappers.ToFakeStore(apiRequest);

            HttpRequestMessage capturedRequest = null;
            SetupHandler(httpResponse, req => capturedRequest = req);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreUpdateAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            await _sutAccounts.UpdateAsync(apiRequest, id, CancellationToken.None);

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Put);
            capturedRequest.RequestUri!.AbsolutePath.Should().Be($"/users/{id}");

            var sentBody = await capturedRequest.Content!.ReadFromJsonAsync<FakeStoreUpdateAccountRequest>();
            sentBody.Should().BeEquivalentTo(expectedBody);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsMappedResponse_FromReader()
        {
            var id = _fixture.Create<int>();
            var apiRequest = _fixture.Create<UpdateAccountRequest>();
            var fakeStoreResponse = _fixture.Create<FakeStoreUpdateAccountResponse>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var expected = AccountMappers.ToApi(fakeStoreResponse);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreUpdateAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            var result = await _sutAccounts.UpdateAsync(apiRequest, id, CancellationToken.None);

            result.Should().BeEquivalentTo(expected);
        }

        // ---------------------------------------------------------------
        // DeleteAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task DeleteAsync_SendsDeleteRequest_ToResolvedUrl_AndEnsuresSuccess()
        {
            var id = _fixture.Create<int>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            HttpRequestMessage capturedRequest = null;
            SetupHandler(httpResponse, req => capturedRequest = req);

            _readerMock
                .Setup(r => r.EnsureSuccessStatusCode(httpResponse))
                .Returns(httpResponse);

            var result = await _sutAccounts.DeleteAsync(id, CancellationToken.None);

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Delete);
            capturedRequest.RequestUri!.AbsolutePath.Should().Be($"/users/{id}");

            _readerMock.Verify(r => r.EnsureSuccessStatusCode(httpResponse), Times.Once);
            result.Should().BeSameAs(httpResponse);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenEnsureSuccessStatusCodeThrows()
        {
            var id = _fixture.Create<int>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.EnsureSuccessStatusCode(httpResponse))
                .Throws(new HttpRequestException("not found"));

            var act = () => _sutAccounts.DeleteAsync(id, CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }
        #endregion

        #region Products
        ///
        /// Products
        /// 

        [Fact]
        public async Task GetAsync_SendsGetRequest_ToResolvedUrl_Products()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeStoreResponse = _fixture.Create<FakeStoreGetAccountResponse>();

            HttpRequestMessage capturedRequest = null;
            SetupHandler(httpResponse, req => capturedRequest = req);

            _readerMock
                .Setup(r => r.ReadAsync<FakeStoreGetAccountResponse>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            using var cts = new CancellationTokenSource();

            await _sutProducts.GetAllAsync(cts.Token);

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Get);
            capturedRequest.RequestUri!.AbsolutePath.Should().Be($"/products");
        }

        [Fact]
        public async Task GetAsync_ReturnsMappedResponse_FromReader_Products()
        {
            var id = _fixture.Create<int>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeStoreResponse = _fixture.Create<IEnumerable<FakeStoreGetProductsResponse>>();

            var expected = ProductMapper.ToApi(fakeStoreResponse);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.ReadAsync<IEnumerable<FakeStoreGetProductsResponse>>(httpResponse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeStoreResponse);

            using var cts = new CancellationTokenSource();

            var result = await _sutProducts.GetAllAsync(cts.Token);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetAsync_PropagatesException_WhenReaderThrows_Products()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            SetupHandler(httpResponse);

            _readerMock
                .Setup(r => r.ReadAsync<IEnumerable<FakeStoreGetProductsResponse>>(httpResponse, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("boom"));

            var act = () => _sutProducts.GetAllAsync(CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }
        #endregion

        #region Helpers
        private static VendorsConfiguration BuildConfiguration(VendorDetails details)
        {
            var vendorSettings = new VendorSettings
            {
                VendorDetails = [details]
            };

            var optionsMonitorMock = new Mock<IOptionsMonitor<VendorSettings>>();
            optionsMonitorMock.Setup(o => o.CurrentValue).Returns(vendorSettings);

            return new VendorsConfiguration(optionsMonitorMock.Object);
        }

        private void SetupHandler(HttpResponseMessage response, Action<HttpRequestMessage> captureRequest = null)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => captureRequest?.Invoke(req))
                .ReturnsAsync(response);
        }
        #endregion
    }
}