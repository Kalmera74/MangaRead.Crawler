using Serilog;
using System.Net.Http.Json;

public abstract class Client<T, E>
{

    protected readonly HttpClient _client;
    protected string CREATE { get; init; } = string.Empty;
    protected string GET { get; init; } = string.Empty;
    protected string UPDATE { get; init; } = string.Empty;

    public Client(HttpClient client)
    {
        _client = client;

    }

    public virtual async Task<T?> Create(E entity, CancellationToken cancellationToken = default)
    {
        return await PostRequestAsync<T, E>(CREATE, entity);
    }

    public virtual async Task<T?> GetBy(string getIdentifier, CancellationToken cancellationToken = default)
    {
        return await GetRequestAsync<T>($"{GET}{getIdentifier}");
    }


    #region   Post Request Methods
    protected async Task<T?> PostRequestAsync(string endpoint, E entity, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(() => _client.PostAsJsonAsync(endpoint, entity), endpoint);
    }

    protected async Task<T?> PostRequestAsync<K>(string endpoint, K entity, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(() => _client.PostAsJsonAsync(endpoint, entity), endpoint);
    }

    protected async Task<K?> PostRequestAsync<K, F>(string endpoint, F entity, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<K>(() => _client.PostAsJsonAsync(endpoint, entity), endpoint);
    }
    #endregion


    #region   Put Request Methods
    protected async Task<T?> PutRequestAsync(string endpoint, E entity, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(() => _client.PutAsJsonAsync(endpoint, entity), endpoint);
    }

    protected async Task<T?> PutRequestAsync<K>(string endpoint, K entity, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(() => _client.PutAsJsonAsync(endpoint, entity), endpoint);
    }

    protected async Task<K?> PutRequestAsync<K, F>(string endpoint, F entity, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<K>(() => _client.PutAsJsonAsync(endpoint, entity), endpoint);
    }

    #endregion


    #region   Get Request Methods
    protected async Task<K?> GetRequestAsync<K>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<K>(() => _client.GetAsync(endpoint), endpoint);
    }
    protected async Task<T?> GetRequestAsync(string endpoint)
    {
        return await ExecuteRequestAsync<T>(() => _client.GetAsync(endpoint), endpoint);
    }
    #endregion

    private async Task<K?> ExecuteRequestAsync<K>(Func<Task<HttpResponseMessage>> requestFunc, string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await requestFunc();
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<K>();
            if (result == null)
            {
                Log.Warning($"Received null response from {endpoint}.");
            }

            return result;
        }

        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Log.Warning($"Resource at {endpoint} was not found.");
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Log.Error($"BadRequestException: Failed request to {endpoint}, reason: {ex.Message}");
            }
            else
            {
                Log.Error($"HttpRequestException: Failed request to {endpoint}, reason: {ex.Message}");
            }
        }
        catch (TaskCanceledException ex)
        {
            Log.Error($"TaskCanceledException: Request to {endpoint} timed out or was canceled, reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log.Error($"Unexpected error during request to {endpoint}, reason: {ex.Message}");
        }

        return default;
    }

}