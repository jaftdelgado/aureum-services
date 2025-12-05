using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;
using Trading; 
using System.Text;
using Google.Protobuf;

namespace ApiGateway.Controllers
{
    [ApiController]
    
    public class LessonsController : ControllerBase
    {
        private readonly LeccionesService.LeccionesServiceClient _client;

        public LessonsController(LeccionesService.LeccionesServiceClient client)
        {
            _client = client;
        }

        
        [HttpGet("api/lessons/{id:length(24)}")]
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
        [HttpGet("api/lessons")]
        [Authorize(AuthenticationSchemes = "SupabaseAuth")] 
        public async Task<IActionResult> GetAll()
        {
            try
            {
                
                var response = await _client.ObtenerTodasAsync(new Empty());

               
                var lista = response.Lecciones.Select(l => new
                {
                    id = l.Id,
                    title = l.Titulo,
                    description = l.Descripcion,
                  
                    thumbnail = l.Miniatura.IsEmpty ? null : Convert.ToBase64String(l.Miniatura.ToByteArray())
                });

                return Ok(lista);
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

       [HttpGet("api/lessons/{id:length(24)}/video")]
        public async Task GetVideo(string id)
        {
            try 
            {
                var infoRequest = new LeccionRequest { IdLeccion = id };
                var info = await _client.ObtenerDetallesAsync(infoRequest);
                long totalLength = info.TotalBytes; 

                long start = 0;
                long end = totalLength - 1;
                var rangeHeader = Request.Headers.Range.ToString();

                if (!string.IsNullOrEmpty(rangeHeader))
                {
                    var range = rangeHeader.Replace("bytes=", "").Split('-');
                    start = long.Parse(range[0]);
                    if (range.Length > 1 && !string.IsNullOrEmpty(range[1]))
                    {
                        end = long.Parse(range[1]);
                    }
                }

                if (end >= totalLength) end = totalLength - 1;
                long contentLength = end - start + 1;

                Response.StatusCode = 206; 
                Response.ContentType = "video/mp4";
                Response.Headers.Append("Content-Range", $"bytes {start}-{end}/{totalLength}");
                Response.Headers.Append("Accept-Ranges", "bytes");
                Response.ContentLength = contentLength;

                var request = new DescargaRequest 
                { 
                    IdLeccion = id, 
                    StartByte = start,
                    EndByte = end
                };

                using var call = _client.DescargarVideo(request);

                long bytesSent = 0;
                await foreach (var chunk in call.ResponseStream.ReadAllAsync(HttpContext.RequestAborted))
                {
                    if (chunk.Contenido.Length > 0)
                    {
                        var bytesToWrite = chunk.Contenido.ToByteArray();
                        
                        await Response.Body.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
                        await Response.Body.FlushAsync();
                        
                        bytesSent += bytesToWrite.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Streaming abortado o error: {ex.Message}");
            }
        }
    }
}
