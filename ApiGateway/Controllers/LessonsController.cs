using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiGateway.Services; 
using Grpc.Core;

namespace ApiGateway.Controllers
{
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonsService _lessonsService;

        public LessonsController(ILessonsService lessonsService)
        {
            _lessonsService = lessonsService;
        }

        private bool IsStudent()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == "user_role")?.Value;
            return !string.IsNullOrEmpty(role) && role == "student";
        }

        [HttpGet("api/lessons/{id:length(24)}")]
        [Authorize(AuthenticationSchemes = "SupabaseAuth")]
        public async Task<IActionResult> GetDetails(string id)
        {
            if (!IsStudent()) return StatusCode(403, "Acceso denegado: Solo los estudiantes pueden ver los detalles.");

            try
            {
                var result = await _lessonsService.GetLessonByIdAsync(id);
                return Ok(result);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return NotFound("Lección no encontrada");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("api/lessons")]
        [Authorize(AuthenticationSchemes = "SupabaseAuth")]
        public async Task<IActionResult> GetAll()
        {
            if (!IsStudent()) return StatusCode(403, "Acceso denegado: Solo los estudiantes pueden listar las lecciones.");

            try
            {
                var result = await _lessonsService.GetAllLessonsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("api/lessons/{id:length(24)}/video")]
        [Authorize(AuthenticationSchemes = "SupabaseAuth")]
        public async Task GetVideo(string id)
        {
            if (!IsStudent())
            {
                Response.StatusCode = 403;
                await Response.Body.FlushAsync();
                return;
            }

            try
            {
                var rangeHeader = Request.Headers.Range.ToString();
                var videoData = await _lessonsService.PrepareVideoStreamAsync(id, rangeHeader);

                Response.StatusCode = 206; 
                Response.ContentType = "video/mp4";
                Response.Headers.Append("Content-Range", $"bytes {videoData.Start}-{videoData.End}/{videoData.TotalLength}");
                Response.Headers.Append("Accept-Ranges", "bytes");
                Response.ContentLength = videoData.ContentLength;

                using var call = videoData.StreamCall;

                await foreach (var chunk in call.ResponseStream.ReadAllAsync(HttpContext.RequestAborted))
                {
                    if (chunk.Contenido.Length > 0)
                    {
                        var bytesToWrite = chunk.Contenido.ToByteArray();
                        await Response.Body.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
                        await Response.Body.FlushAsync();
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