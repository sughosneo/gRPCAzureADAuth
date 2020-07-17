using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Product.gRPC.API.Functional.Tests.Helpers;
using System;

namespace Product.gRPC.API.Functional.Tests
{
    public class FunctionalTestBase : IDisposable
    {
        private GrpcChannel? _channel;
        private IDisposable? _testContext;

        protected GrpcTestFixture<Startup> Fixture { get; private set; } = default!;

        protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

        protected GrpcChannel Channel => _channel ??= CreateChannel();

        protected GrpcChannel CreateChannel()
        {
            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                LoggerFactory = LoggerFactory,
                HttpHandler = Fixture.Handler           
            });
        }

        public FunctionalTestBase()
        {
            Fixture = new GrpcTestFixture<Startup>(ConfigureServices);            
            _testContext = Fixture.GetTestContext();
            
        }

        public void Dispose()
        {
            Fixture.Dispose();
            
            _testContext?.Dispose();
            _channel = null;

        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }        
    }
}
