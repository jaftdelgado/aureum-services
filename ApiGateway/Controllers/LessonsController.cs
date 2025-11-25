using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;
using Trading;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly LeccionesService.LeccionesServiceClient _client;

    public LessonsController(LeccionesService.LeccionesServiceClient client)
    {
        _client = client;
    }

    // GET: api/lessons/{id} (Detalles normales)
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = "SupabaseAuth")]
    public async Task<IActionResult> GetDetails(string id)
    {
        try
        {
            var request = new LeccionRequest { IdLeccion = id };
            var response = await _client.ObtenerDetallesAsync(request);
            return Ok(response);
        }
        catch (RpcException ex)
        {
            return StatusCode((int)ex.StatusCode, ex.Status.Detail);
        }
    }

    // GET: api/lessons/{id}/video (Streaming de Video)
    // El Frontend pondrá esto en <video src="...">
    [HttpGet("{id}/video")]
    // [Authorize] <-- Opcional: A veces los tags <video> no envían headers de auth fácil. 
    // Si falla el video, prueba quitando el Authorize temporalmente o pasando el token por query param.
    public async Task GetVideo(string id)
    {
        var request = new LeccionRequest { IdLeccion = id };
        using var call = _client.DescargarVideo(request);

        Response.ContentType = "video/mp4"; // Le decimos al navegador que es video

        try
        {
            await foreach (var chunk in call.ResponseStream.ReadAllAsync(HttpContext.RequestAborted))
            {
                if (chunk.Contenido.Length > 0)
                {
                    await Response.Body.WriteAsync(chunk.Contenido.Memory);
                    await Response.Body.FlushAsync();
                }
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            // Cliente cerró el video
        }
    }
}