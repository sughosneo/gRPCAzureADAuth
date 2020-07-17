using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebMotions.Fake.Authentication.JwtBearer;

namespace Product.gRPC.API.Functional.Tests.Helpers
{
    public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception exception);

    public class GrpcTestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;
        private readonly IHost _host;

        public event LogMessage? LoggedMessage;

        public GrpcTestFixture() : this(null) { }

        public GrpcTestFixture(Action<IServiceCollection>? initialConfigureServices)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
            {
                LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
            }));

            var builder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    initialConfigureServices?.Invoke(services);
                    services.AddSingleton<ILoggerFactory>(LoggerFactory);
                })
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost
                        .UseTestServer()
                        .ConfigureTestServices(collection =>
                        {
                            collection.AddAuthentication(options =>
                            {
                                options.DefaultAuthenticateScheme = FakeJwtBearerDefaults.AuthenticationScheme;                                
                            }).AddFakeJwtBearer();
                        })
                        .UseStartup<TStartup>();

                });
            _host = builder.Start();
            _server = _host.GetTestServer();

            var responseVersionHandler = new ResponseVersionHandler();
            responseVersionHandler.InnerHandler = _server.CreateHandler();

            Handler = responseVersionHandler;            
        }

        public LoggerFactory LoggerFactory { get; }

        public HttpMessageHandler Handler { get; }        

        public void Dispose()
        {
            Handler.Dispose();
            _host.Dispose();
            _server.Dispose();
        }

        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {

                // Setting up the authorization header. 
                // Had to understand the code little bit to tweak it. 
                /// https://github.com/webmotions/fake-authentication-jwtbearer/blob/eea2b6bc6a69064a3cb85101aaf8a71142b7756b/src/WebMotions.Fake.Authentication.JwtBearer/HttpClientExtensions.cs#L86
                /// 

                dynamic token = new ExpandoObject();
                token.sub = Guid.NewGuid();
                token.role = new[] { "sub_role", "admin" };

                request.Headers.Authorization = new AuthenticationHeaderValue(FakeJwtBearerDefaults.AuthenticationScheme, JsonConvert.SerializeObject(token));

                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;

                return response;
            }
        }

        public IDisposable GetTestContext()
        {
            return new GrpcTestContext<TStartup>(this);
        }
    }
}
