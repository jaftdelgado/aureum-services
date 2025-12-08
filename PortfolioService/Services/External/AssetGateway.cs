using PortfolioService.Dtos;

namespace PortfolioService.Services.External
{
    public interface IAssetGateway
    {
        Task<AssetExternalDto> GetAssetInfoAsync(Guid assetId);
    }

    public class AssetGateway : IAssetGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AssetGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AssetExternalDto> GetAssetInfoAsync(Guid assetId)
        {
            var client = _httpClientFactory.CreateClient();

            var baseUrl = _configuration["AssetServiceUrl"] ?? "http://assetservice.railway.internal:3000";
            baseUrl = baseUrl.TrimEnd('/');

            try
            {
                var response = await client.GetFromJsonAsync<AssetExternalDto>($"{baseUrl}/assets/{assetId}");

                return response ?? new AssetExternalDto { Name = "Desconocido", Symbol = "---" };
            }
            catch (Exception)
            {
                return new AssetExternalDto { Name = "Cargando...", Symbol = "---" };
            }
        }
    }
}