
internal interface IInternalServices
{
    Task<InventoryDetailsResponseModel?> GetInventoryDetails(string[] productNames, CancellationToken cancellationToken);
    Task<object> GetProductDetails(IEnumerable<int> productIds, CancellationToken cancellationToken);
}

internal class InternalServices : IInternalServices
{
    private readonly IInternalHttpClient _httpClient;

    public InternalServices(IInternalHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryDetailsResponseModel?> GetInventoryDetails(string[] productNames, CancellationToken cancellationToken)
    {
        var request = RequestBuilder
            .CreateGetRequest("Inventory/GetInventoryDetails")
            .AddQueryParameter("productNames", string.Join(",", productNames));

        var result = await _httpClient.GetAsync<InventoryDetailsResponseModel>(InternalServiceType.Ordering, request, cancellationToken);

        return result.Match(
            inventoryDetails => inventoryDetails,
            _ => default!);
    }

    public async Task<object> GetProductDetails(IEnumerable<int> productIds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public record InventoryDetailsResponseModel(string ProductId, string ProductName, decimal AvailableStock);
