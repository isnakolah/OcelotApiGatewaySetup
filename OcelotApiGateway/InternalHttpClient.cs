using System.Text.Json;
using OneOf;

internal interface IInternalHttpClient
{
    Task<OneOf<T, Exception>> GetAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken);
    
    Task<OneOf<T, Exception>> PostAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken);
    
    Task<Exception?> PostAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken);
    
    Task<OneOf<T, Exception>> PutAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken);
    
    Task<Exception?> PutAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken);
    
    Task<OneOf<T, Exception>> DeleteAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken);
}

internal class ConcreteInternalHttpClient : IInternalHttpClient
{
    private readonly ILogger<ConcreteInternalHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ConcreteInternalHttpClient(IHttpClientFactory httpClientFactory, ILogger<ConcreteInternalHttpClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<OneOf<T, Exception>> GetAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Method = HttpMethod.Get;
        
        return await SendAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> PostAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Method = HttpMethod.Post;
        
        return await SendAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<Exception?> PostAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Method = HttpMethod.Post;
        
        return await SendAsync(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> PutAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Method = HttpMethod.Put;
        
        return await SendAsync<T>(serviceType, request, cancellationToken);
    }

    public async Task<Exception?> PutAsync(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Method = HttpMethod.Put;
        
        return await SendAsync(serviceType, request, cancellationToken);
    }

    public async Task<OneOf<T, Exception>> DeleteAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Method = HttpMethod.Delete;
        
        return await SendAsync<T>(serviceType, request, cancellationToken);
    }

    private async Task<OneOf<T, Exception>> SendAsync<T>(InternalServiceType serviceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(serviceType.GetName());

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            
            _logger.LogDebug("Request to {Url} is {Request}", request.RequestUri, JsonSerializer.Serialize(request));
            
            _logger.LogDebug("Response from {Url} is {Response}", response.RequestMessage?.RequestUri, JsonSerializer.Serialize(response));

            response.EnsureSuccessStatusCode();

            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

            var content = JsonSerializer.Deserialize<T>(contentString);

            if (content is null)
            {
                return new Exception($"Error deserializing response from {request.RequestUri}");
            }

            return content;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling {Url}", $"{request.RequestUri}");
            
            return e;
        }
    }

    private async Task<Exception?> SendAsync(InternalServiceType internalServiceType, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(internalServiceType.GetName());

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            
            _logger.LogDebug("Request to {Url} is {Request}", request.RequestUri, JsonSerializer.Serialize(request));
            
            _logger.LogDebug("Response from {Url} is {Response}", response.RequestMessage?.RequestUri, JsonSerializer.Serialize(response));

            response.EnsureSuccessStatusCode();

            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling {Url}", $"{request.RequestUri}");
            
            return e;
        }
    }
}