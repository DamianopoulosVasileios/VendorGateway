using FluentAssertions;
using VendorGateway.Infrastructure.APIs.Configuration;
using VendorGateway.Infrastructure.Enums;
using VendorGateway.Tests.Configuration;

namespace VendorGateway.Tests
{
    public class VendorsConfigurationTests
    {
        private readonly VendorsConfiguration _sut;
        private const Vendors VendorFakeValue = (Vendors)999;

        public VendorsConfigurationTests()
        {
            _sut = ModelsConfiguration.VendorsConfiguration();
        }

        [Fact]
        public void GetAll_ShouldReturnAllConfiguredVendors()
        {
            // Act
            var result = _sut.GetAll();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Get_ShouldReturnRequestedVendor()
        {
            // Act
            var result = _sut.Get(Vendors.FakeStore);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(Vendors.FakeStore.ToString());
        }

        [Fact]
        public void Get_WhenVendorDoesNotExist_ShouldThrow()
        {
            // Arrange

            var sut = ModelsConfiguration.EmptyVendorsConfiguration();

            // Act
            Action act = () => sut.Get(VendorFakeValue);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetAll_WhenNoVendorsConfigured_ShouldReturnEmptyList()
        {
            // Arrange
            var sut = ModelsConfiguration.EmptyVendorsConfiguration();

            // Act
            var result = sut.GetAll();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
