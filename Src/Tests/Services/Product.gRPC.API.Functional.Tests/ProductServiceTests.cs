using Contracts.Lib;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace Product.gRPC.API.Functional.Tests
{
    public class ProductServiceTests : FunctionalTestBase
    {
        public ProductServiceTests() : base() { }

        [Fact]
        public async Task GetAllProducts_Success_Not_Null()
        {            
            // Arrange
            var client = new ProductSvc.ProductSvcClient(Channel);

            // Act                       
            var response = await client.GetAllProductsAsync(new Empty());

            // Assert            
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllProducts_Success_List()
        {

            // Arrange
            var client = new ProductSvc.ProductSvcClient(Channel);

            // Act            
            var response = await client.GetAllProductsAsync(new Empty());

            // Assert            
            response.Productmsgs.Count.Should().Be(3);
        }

        [Fact]
        public async Task GetAllProductsById_Failed_Wrong_Id_Invalid_Argument_Exceptions()
        {

            // Arrange
            var client = new ProductSvc.ProductSvcClient(Channel);

            // Act            
            var productIdMsg = new ProductIdMsg() { Id = 0 };
            Func<Task> action = async () => await client.GetAllProductsByIdAsync(productIdMsg);

            // Assert            
            action.Should().Throw<RpcException>()
                .WithMessage(new Status(StatusCode.InvalidArgument, "Invalid Product Id").ToString());
        }

        [Fact]
        public async Task GetAllProductsById_Failed_Null_Id_Invalid_Argument_Exceptions()
        {

            // Arrange
            var client = new ProductSvc.ProductSvcClient(Channel);

            // Act                                       
            var productIdMsg = new ProductIdMsg();
            Func<Task> action = async () => await client.GetAllProductsByIdAsync(productIdMsg);

            // Assert            
            action.Should().Throw<RpcException>()
                .WithMessage(new Status(StatusCode.InvalidArgument, "Invalid Product Id").ToString());
        }
    }
}
