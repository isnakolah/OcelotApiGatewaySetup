public class SingletonTokenManager
{
    private static readonly object _lock = new();
    private static ILogger<SingletonTokenManager>? _logger;
    private static Lazy<Task<(string Token, DateTime Expiry)>> _lazyToken;
    private static readonly HttpClient _httpClient;

    static SingletonTokenManager()
    {
        _httpClient = new HttpClient();
        _lazyToken = new Lazy<Task<(string, DateTime)>>(() => FetchNewTokenAsync(CancellationToken.None), isThreadSafe: true);

        StartTokenRefreshTask();
    }

    public static async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var (token, expiry) = await _lazyToken.Value;

            if (IsTokenExpired(expiry))
            {
                await RefreshTokenAsync(cancellationToken);
            }

            if (IsTokenAboutToExpire(expiry))
            {
                _ = Task.Run(() => RefreshTokenAsync(cancellationToken), cancellationToken);
            }

            return token;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get the token");

            throw; 
        }
    }

    public static void Initialize(IConfiguration configuration, ILogger<SingletonTokenManager> logger)
    {
        _httpClient.BaseAddress = new Uri(configuration["Auth:RefreshTokenUrl"]);
        _logger = logger;
    }

    private static bool IsTokenAboutToExpire(DateTime expiry)
    {
        return DateTime.UtcNow.AddSeconds(20) >= expiry;
    }
    
    private static bool IsTokenExpired(DateTime expiry)
    {
        return DateTime.UtcNow >= expiry;
    }

    private static async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            lock (_lock)
            {
                if (_lazyToken.IsValueCreated && IsTokenAboutToExpire(_lazyToken.Value.Result.Expiry))
                {
                    _lazyToken = new Lazy<Task<(string, DateTime)>>(() => FetchNewTokenAsync(cancellationToken), isThreadSafe: true);
                }
            }

            await _lazyToken.Value;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Token refresh operation was canceled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh the token");
        }
    }
    
    private static async Task<(string Token, DateTime Expiry)> FetchNewTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            // var response = await _httpClient.PostAsync("", null, cancellationToken);
            //
            // if (!response.IsSuccessStatusCode)
            // {
            //     throw new Exception("Failed to fetch new token");
            // }

            var token = "da;ldfja;lsdfkja;sdlfkjas;dlfjaweoi;jffdkl;ajsdf";

            var expiry = DateTime.UtcNow.AddMinutes(20);

            return (token, expiry);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to fetch a new token");

            throw;
        }
    }
    
    private static void StartTokenRefreshTask()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var (_, expiry) = await _lazyToken.Value;

                    if (IsTokenAboutToExpire(expiry))
                    {
                        _logger?.LogInformation("Token is about to expire. Refreshing...");

                        await RefreshTokenAsync(CancellationToken.None);
                        
                        continue;
                    }

                    var secondsToSleep = (expiry - DateTime.UtcNow).TotalSeconds - 20;
                    
                    if (secondsToSleep > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(secondsToSleep));
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to refresh the token");
                }
            }
        });
    }
}
