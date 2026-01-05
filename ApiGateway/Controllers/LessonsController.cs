using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiGateway.Services; 
using Grpc.Core;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
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
          if (!IsStudent()) { Response.StatusCode = 403; return; }

   try
            {
                var rangeHeader = Request.Headers.Range.ToString();
                var videoData = await _lessonsService.PrepareVideoStreamAsync(id, rangeHeader);

                if (string.IsNullOrEmpty(rangeHeader))
                {
                    Response.StatusCode = 200; 
                    Response.ContentLength = videoData.TotalLength;
                }
                else
                {
                    Response.StatusCode = 206; 
                    Response.Headers.Append("Content-Range", $"bytes {videoData.Start}-{videoData.End}/{videoData.TotalLength}");
                    Response.ContentLength = videoData.ContentLength;
                }

                Response.ContentType = "video/mp4";
                Response.Headers.Append("Accept-Ranges", "bytes");

                using var call = videoData.StreamCall;

                await foreach (var chunk in call.ResponseStream.ReadAllAsync(HttpContext.RequestAborted))
                {
                    if (chunk.Contenido.Length > 0)
                    {
                        await Response.Body.WriteAsync(chunk.Contenido.ToByteArray());
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de Streaming: {ex.Message}");
            }

        }
    }
}
