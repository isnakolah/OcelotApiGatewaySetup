using Polly;
using Polly.Contrib.WaitAndRetry;
using static InternalServicesConfiguration;

var builder = WebApplication.CreateBuilder(args);

var loggerFactory = LoggerFactory.Create(loggingBuilder => 
{
    loggingBuilder.AddConsole();
});
var logger = loggerFactory.CreateLogger<SingletonTokenManager>();

SingletonTokenManager.Initialize(builder.Configuration, logger);

Func<PolicyBuilder<HttpResponseMessage>,IAsyncPolicy<HttpResponseMessage>> retryPolicyBuilder = policyBuilder =>
    policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5));

builder.Services
    .AddHttpClient(Onboarding, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.BaseAddress = new Uri(builder.Configuration[$"UrlConfigs:{Onboarding}"]);
    })
    .AddTransientHttpErrorPolicy(retryPolicyBuilder);

builder.Services
    .AddHttpClient(Ordering, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.BaseAddress = new Uri(builder.Configuration[$"UrlConfigs:{Ordering}"]);
    })
    .AddTransientHttpErrorPolicy(retryPolicyBuilder);

builder.Services.AddMemoryCache();

builder.Services
    .AddDecoratedService<IInternalHttpClient, ConcreteInternalHttpClient>()
    .AddDecorator<InternalHttpClientCacheDecorator>()
    .AddDecorator<InternalHttpClientAuthDecorator>();

builder.Services.BuildDecorators();

var app = builder.Build();

app.MapGet("test", async (IInternalHttpClient httpClient) =>
{
    var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/AggregatedOrders");
    
    var result = await httpClient.GetAsync<object>(InternalServiceType.Ordering, request, CancellationToken.None);

    return Results.Ok(result);
});

app.Run();


// services.AddTransient<ConcreteInternalHttpClient>();
//
// services.AddTransient(sp => new InternalHttpClientCacheDecorator(
//     internalHttpClient: sp.GetRequiredService<InternalHttpClientAuthDecorator>(),
//     logger: sp.GetRequiredService<ILogger<InternalHttpClientCacheDecorator>>(),
//     cache: sp.GetRequiredService<IMemoryCache>()));
//
// services.AddTransient(sp => new InternalHttpClientAuthDecorator(
//     internalHttpClient: sp.GetRequiredService<ConcreteInternalHttpClient>()));
//
// services.AddTransient<IInternalHttpClient>(sp => sp.GetRequiredService<InternalHttpClientCacheDecorator>());
//
// return services.BuildServiceProvider().GetRequiredService<IDecoratedService>();