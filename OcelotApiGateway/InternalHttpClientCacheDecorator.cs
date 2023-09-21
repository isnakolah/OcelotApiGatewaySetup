using Microsoft.Extensions.Caching.Memory;
using OneOf;

internal class InternalHttpClientCacheDecorator : IInternalHttpClient
{
    private readonly ILogger<InternalHttpClientCacheDecorator> _logger;
    private readonly IInternalHttpClient _internalHttpClient;
    private readonly IMemoryCache _cache;
    
    public InternalHttpClientCacheDecorator(IInternalHttpClient internalHttpClient, ILogger<InternalHttpClientCacheDecorator> logger, IMemoryCache cache)
    {
        _internalHttpClient = internalHttpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<OneOf<T, Exception>> GetAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var key = request.RequestUri!.ToString();
        
        if (!_cache.TryGetValue(key, out T cachedValue))
        {
            return _cache.Set(
                key: key,
                value: await _internalHttpClient.GetAsync<T>(serviceType, request, cancellationToken), 
                absoluteExpirationRelativeToNow: TimeSpan.FromSeconds(10));
        }

        _logger.LogInformation("Retrieved {Url} from cache", key);

        return cachedValue;
    }

    public async Task<OneOf<T, Exception>> PostAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _internalHttpClient.PostAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<Exception?> PostAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _internalHttpClient.PostAsync(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> PutAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _internalHttpClient.PutAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<Exception?> PutAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _internalHttpClient.PutAsync(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> DeleteAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _internalHttpClient.DeleteAsync<T>(serviceType, request, cancellationToken);
    }
}

internal class InternalHttpClientAuthDecorator : IInternalHttpClient
{
    private readonly IInternalHttpClient _internalHttpClient;

    public InternalHttpClientAuthDecorator(IInternalHttpClient internalHttpClient)
    {
        _internalHttpClient = internalHttpClient;
    }

    public async Task<OneOf<T, Exception>> GetAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddBearerTokenAsync(request, cancellationToken);

        return await _internalHttpClient.GetAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> PostAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddBearerTokenAsync(request, cancellationToken);

        return await _internalHttpClient.PostAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<Exception?> PostAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddBearerTokenAsync(request, cancellationToken);

        return await _internalHttpClient.PostAsync(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> PutAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddBearerTokenAsync(request, cancellationToken);

        return await _internalHttpClient.PutAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<Exception?> PutAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddBearerTokenAsync(request, cancellationToken);

        return await _internalHttpClient.PutAsync(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> DeleteAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddBearerTokenAsync(request, cancellationToken);

        return await _internalHttpClient.DeleteAsync<T>(serviceType, request, cancellationToken);
    }

    private static async Task AddBearerTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await SingletonTokenManager.GetTokenAsync(cancellationToken);

        request.AddBearerToken(token);
    }
}
