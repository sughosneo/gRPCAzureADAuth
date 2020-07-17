using Contracts.Lib;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProductSvc.Consumable.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("ProductSvc.Consumable.Client:: Starting client executions ...");

                IConfiguration configuration = GetLoadedConfiguration();
                var productGrpcAPIUrl = configuration.GetValue<string>("ProductGrpcAPIUrl");
                var accessToken = await GetAccessTokenAsync(configuration);

                Console.WriteLine("ProductSvc.Consumable.Client:: Fecthing all product details ...");

                var allProductResult = await GetAllProductDetailsAsync(productGrpcAPIUrl, accessToken);                
                Console.WriteLine(allProductResult);

                Console.WriteLine("ProductSvc.Consumable.Client:: Fecthing specific product details ...");
                var specificProductResult = await GetSpecificProductDetailsAsync(productGrpcAPIUrl, accessToken);
                Console.WriteLine(specificProductResult);

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadKey();
        }        
        
        /// <summary>
        ///     Method to load necessary configurations from appsettings.json
        /// </summary>
        /// <returns>IConfiguration</returns>
        static IConfiguration GetLoadedConfiguration()
        {            
            try
            {
                Console.WriteLine("ProductSvc.Consumable.Client:: loading configurations");

                IConfiguration _configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();

                return _configuration;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProductSvc.Consumable.Client:: Exception while loading configuration");
                throw ex;
            }

        }

        /// <summary>
        ///     Method to fetch necessary access token
        /// </summary>
        /// <returns>string</returns>
        static async Task<string> GetAccessTokenAsync(IConfiguration configuration)
        {
            Console.WriteLine("ProductSvc.Consumable.Client:: Fetching access token");

            try
            {

                var clientId = configuration.GetValue<string>("AzureAD:ClientId");
                var clientSecret = configuration.GetValue<string>("AzureAD:ClientSecret");
                var tenantId = configuration.GetValue<string>("AzureAD:TenantId");
                var apiResourceId = configuration.GetValue<string>("AzureAD:APIResourceId");
                var authorityUrl = $"{configuration.GetValue<string>("AzureAD:InstanceId")}{tenantId}/oauth2/v2.0/token";

                IConfidentialClientApplication _clientApp = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authorityUrl))
                    .Build();

                string[] scopes = new string[] { apiResourceId };

                AuthenticationResult result = await _clientApp.AcquireTokenForClient(scopes).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalClientException ex)
            {
                Console.WriteLine("ProductSvc.Consumable.Client:: Exception while loading configuration");

                throw ex;
            }
        }

        /// <summary>
        ///     Method to fetch all product details
        /// </summary>
        /// <returns>string</returns>
        
        static async Task<string> GetAllProductDetailsAsync(string productGrpcAPIUrl, string accessToken)
        {
            try
            {
                Console.WriteLine("ProductSvc.Consumable.Client:: Calling Product grpc API..");

                var requestHeaderMetadata = new Metadata()
                {
                    { "Authorization", $"Bearer {accessToken}"}
                };

                var channel = GrpcChannel.ForAddress(productGrpcAPIUrl);
                var client = new Contracts.Lib.ProductSvc.ProductSvcClient(channel);
                var response = await client.GetAllProductsAsync(new Empty(), requestHeaderMetadata);

                return response.Productmsgs.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Method to fetch specific product details
        /// </summary>
        /// <returns>string</returns>
        /// 
        private static async Task<string> GetSpecificProductDetailsAsync(string productGrpcAPIUrl, string accessToken)
        {
            try
            {
                Console.WriteLine("ProductSvc.Consumable.Client:: Calling Product grpc API..");

                var requestHeaderMetadata = new Metadata()
                {
                    { "Authorization", $"Bearer {accessToken}"}
                };

                var channel = GrpcChannel.ForAddress(productGrpcAPIUrl);
                var client = new Contracts.Lib.ProductSvc.ProductSvcClient(channel);                
                var productIdMsg = new ProductIdMsg() { Id = 1 };
                var response = await client.GetAllProductsByIdAsync(productIdMsg, requestHeaderMetadata);

                return response.Productmsgs.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
