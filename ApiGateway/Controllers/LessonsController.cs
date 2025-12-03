using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;
using Trading; 
using System.Text;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/lessons")]
    public class LessonsController : ControllerBase
    {
        private readonly LeccionesService.LeccionesServiceClient _client;

        public LessonsController(LeccionesService.LeccionesServiceClient client)
        {
            _client = client;
        }

        
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "SupabaseAuth")]
        public async Task<IActionResult> GetDetails(string id)
        {
            try
            {
                var request = new LeccionRequest { IdLeccion = id };
                var response = await _client.ObtenerDetallesAsync(request);

                return Ok(new
                {
                    response.Id,
                    response.Titulo,
                    response.Descripcion,
                    Miniatura = response.Miniatura.IsEmpty ? null : Convert.ToBase64String(response.Miniatura.ToByteArray())
                });
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.NotFound) return NotFound("Lección no encontrada");
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpGet("{id}/video")]
        public async Task GetVideo(string id)
        {

            var request = new LeccionRequest { IdLeccion = id };
            using var call = _client.DescargarVideo(request);

            Response.ContentType = "video/mp4";

            try
            {
                await foreach (var chunk in call.ResponseStream.ReadAllAsync(HttpContext.RequestAborted))
                {
                    if (chunk.Contenido.Length > 0)
                    {
                        var bytes = chunk.Contenido.ToByteArray();
                        await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                        await Response.Body.FlushAsync();
                    }
                }
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error streaming video: {ex.Message}");
            }
        }
    }
}
