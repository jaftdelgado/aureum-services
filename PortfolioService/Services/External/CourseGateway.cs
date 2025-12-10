using PortfolioService.Dtos;

namespace PortfolioService.Services.External
{
    /// <summary>
    /// Contrato para interactuar con el servicio externo de Cursos/Equipos.
    /// </summary>
    public interface ICourseGateway
    {
        /// <summary>
        /// Obtiene todas las membresías (alumnos) asociadas a un equipo o curso específico.
        /// </summary>
        /// <param name="teamId">El ID del equipo a consultar.</param>
        /// <returns>Lista de membresías con los IDs de usuario.</returns>
        Task<IEnumerable<MembershipExternalDto>> GetMembershipsByTeamAsync(Guid teamId);
    }

    public class CourseGateway : ICourseGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CourseGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IEnumerable<MembershipExternalDto>> GetMembershipsByTeamAsync(Guid teamId)
        {
            var client = _httpClientFactory.CreateClient();

            var baseUrl = _configuration["CourseServiceUrl"] ?? "http://courseservice.railway.internal:3000";
            baseUrl = baseUrl.TrimEnd('/');

            try
            {
                var url = $"{baseUrl}/api/v1/memberships/course/{teamId}";

                var response = await client.GetFromJsonAsync<IEnumerable<MembershipExternalDto>>(url);

                return response ?? new List<MembershipExternalDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CourseGateway] Error obteniendo miembros para el equipo {teamId}: {ex.Message}");
                return new List<MembershipExternalDto>();
            }
        }
    }
}