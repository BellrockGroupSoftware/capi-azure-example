using Bellrock.Capi.CapiConnector.Api;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Bellrock.Capi.Functions.Startup))]
namespace Bellrock.Capi.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();

        builder.Services.AddMemoryCache();

        var capiApiUrl = builder.GetContext().Configuration.GetValue<string>("capi-base-path");

        builder.Services.AddHttpClient<ICapiAuthenticationService, CapiApiKeySecretAuthService>()
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(capiApiUrl);
            });

        builder.Services.AddTransient<CapiAuthHandler>();

        builder.Services.AddHttpClient<ICapiService, CapiService>()
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(capiApiUrl);
            })
            .AddHttpMessageHandler<CapiAuthHandler>();
    }
}
