using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Lib;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Product.gRPC.API.Constants;
using Product.gRPC.API.Repositories;

namespace Product.gRPC.API.Services
{
    [Authorize]
    public class ProductService : ProductSvc.ProductSvcBase
    {
        private readonly ILogger<ProductService> _logger;
        private readonly IProductRepository _productRepository;
        public ProductService(ILogger<ProductService> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;

        }

        public override async Task<ProductListMsg> GetAllProducts(Empty request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC Server:: Within method : GetAllProducts() ");

            try
            {
                var httpContext = context.GetHttpContext();
                var requestedUserContext = httpContext.User;

                _logger.LogInformation($"Request has been made by : {requestedUserContext}");

                // TODO: Need to convert this methods to a linq query.
                var listOfProducts = await _productRepository.GetAll();
                var filteredProduct = new RepeatedField<ProductMsg>();

                foreach (var item in listOfProducts)
                {
                    filteredProduct.Add(new ProductMsg() { Id = item.Id, Name = item.Name, Price = item.Price });
                }

                // https://stackoverflow.com/questions/59299158/initialize-google-protobuf-repeatedfield-collections

                var productListMsgs = new ProductListMsg();
                productListMsgs.Productmsgs.Add(filteredProduct);

                return productListMsgs;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC Server:: Exception in GetAllProducts()");
                _logger.LogError(ex.Message);

                throw new RpcException(new Status(StatusCode.Internal, MsgConstants.INTERNAL_SERVER_ERROR));
            }

        }

        public override async Task<ProductListMsg> GetAllProductsById(ProductIdMsg productMsg, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC Server:: Within method : GetAllProductsById() ");

            if (productMsg != null)
            {
                if (productMsg.Id > 0)
                {
                    try
                    {

                        var httpContext = context.GetHttpContext();
                        var requestedUserContext = httpContext.User;

                        _logger.LogInformation($"Request has been made by : {requestedUserContext}");

                        var listOfProducts = await _productRepository.GetSpecific(productMsg.Id);
                        var filteredProduct = new RepeatedField<ProductMsg>();

                        foreach (var item in listOfProducts)
                        {
                            filteredProduct.Add(new ProductMsg() { Id = item.Id, Name = item.Name, Price = item.Price });
                        }

                        var productListMsgs = new ProductListMsg();
                        productListMsgs.Productmsgs.Add(filteredProduct);

                        return productListMsgs;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception in GetAllProductsById()");
                        _logger.LogError(ex.Message);
                        throw new RpcException(new Status(StatusCode.Internal, MsgConstants.INTERNAL_SERVER_ERROR));
                    }
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, MsgConstants.INVALID_PRODUCT_ID));
                }
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, MsgConstants.NULL_PRODUCT_REQUEST));
            }
        }

    }
}
