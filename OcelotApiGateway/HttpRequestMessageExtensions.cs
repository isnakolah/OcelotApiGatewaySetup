using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace System.Net.Http;

public class RequestBuilder
{
    public static HttpRequestMessage CreateGetRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Get, uri);
    }
    
    public static HttpRequestMessage CreateGetRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Get, uri);
    }
    
    public static HttpRequestMessage CreatePostRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Post, uri);
    }
    
    public static HttpRequestMessage CreatePostRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Post, uri);
    }
    
    public static HttpRequestMessage CreatePutRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Put, uri);
    }
    
    public static HttpRequestMessage CreatePutRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Put, uri);
    }
    
    public static HttpRequestMessage CreateDeleteRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Delete, uri);
    }
    
    public static HttpRequestMessage CreateDeleteRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Delete, uri);
    }
    
    public static HttpRequestMessage CreateHeadRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Head, uri);
    }
    
    public static HttpRequestMessage CreateHeadRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Head, uri);
    }
    
    public static HttpRequestMessage CreateOptionsRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Options, uri);
    }
    
    public static HttpRequestMessage CreateOptionsRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Options, uri);
    }
    
    public static HttpRequestMessage CreatePatchRequest(string uri)
    {
        return new HttpRequestMessage(HttpMethod.Patch, uri);
    }
    
    public static HttpRequestMessage CreatePatchRequest(Uri uri)
    {
        return new HttpRequestMessage(HttpMethod.Patch, uri);
    }
}

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage Clone(this HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var prop in request.Properties)
        {
            clone.Properties.Add(prop);
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }


    #region AddJsonBody

    public static HttpRequestMessage AddJsonBody<T>(this HttpRequestMessage request, T body)
    {
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        
        return request;
    }
    
    public static HttpRequestMessage AddJsonBody(this HttpRequestMessage request, object body)
    {
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        
        return request;
    }
    
    public static HttpRequestMessage AddJsonBody(this HttpRequestMessage request, Func<object> bodyFactory)
    {
        request.Content = new StringContent(JsonSerializer.Serialize(bodyFactory()), Encoding.UTF8, "application/json");
        
        return request;
    }

    #endregion

    #region Bearer Token
    public static HttpRequestMessage AddBearerToken(this HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return request;
    }
    
    public static HttpRequestMessage AddBearerToken(this HttpRequestMessage request, Func<string> tokenFactory)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenFactory());
        
        return request;
    }
    #endregion
    

    #region Query
    public static HttpRequestMessage AddQueryParameter(this HttpRequestMessage request, string key, string value)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("RequestUri is null");
        }
        
        var uriBuilder = new UriBuilder(request.RequestUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query[key] = value;

        uriBuilder.Query = query.ToString();

        request.RequestUri = uriBuilder.Uri;
        
        return request;
    }
    
    public static HttpRequestMessage AddQueryParameter(this HttpRequestMessage request, string key, IEnumerable<string> values)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("RequestUri is null");
        }
        
        var uriBuilder = new UriBuilder(request.RequestUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var value in values)
        {
            query.Add(key, value);
        }

        uriBuilder.Query = query.ToString();

        request.RequestUri = uriBuilder.Uri;
        
        return request;
    }
    
    public static HttpRequestMessage AddQueryParameter(this HttpRequestMessage request, string key, Func<string> valueFactory)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("RequestUri is null");
        }
        
        var uriBuilder = new UriBuilder(request.RequestUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query[key] = valueFactory();

        uriBuilder.Query = query.ToString();

        request.RequestUri = uriBuilder.Uri;
        
        return request;
    }
    
    public static HttpRequestMessage AddQueryParameter(this HttpRequestMessage request, string key, Func<IEnumerable<string>> valuesFactory)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("RequestUri is null");
        }
        
        var uriBuilder = new UriBuilder(request.RequestUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var value in valuesFactory())
        {
            query.Add(key, value);
        }

        uriBuilder.Query = query.ToString();

        request.RequestUri = uriBuilder.Uri;
        
        return request;
    }
            
    public static HttpRequestMessage AddQueryParameter(this HttpRequestMessage request, string key, params string[] values)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("RequestUri is null");
        }
        
        var uriBuilder = new UriBuilder(request.RequestUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var value in values)
        {
            query.Add(key, value);
        }

        uriBuilder.Query = query.ToString();

        request.RequestUri = uriBuilder.Uri;
        
        return request;
    }
    #endregion


    #region AddHeader
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, string value)
    {
        request.Headers.Add(key, value);
        
        return request;
    }
    
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, IEnumerable<string> values)
    {
        request.Headers.Add(key, values);
        
        return request;
    }
    
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, Func<string> valueFactory)
    {
        request.Headers.Add(key, valueFactory());
        
        return request;
    }
    
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, Func<IEnumerable<string>> valuesFactory)
    {
        request.Headers.Add(key, valuesFactory());
        
        return request;
    }
    
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, params string[] values)
    {
        request.Headers.Add(key, values);
        
        return request;
    }
    
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, params Func<string>[] valuesFactory)
    {
        request.Headers.Add(key, valuesFactory.Select(x => x()));
        
        return request;
    }
    
    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string key, params Func<IEnumerable<string>>[] valuesFactory)
    {
        request.Headers.Add(key, valuesFactory.SelectMany(x => x()));
        
        return request;
    }
    

    #endregion
}