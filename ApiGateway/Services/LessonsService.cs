using ApiGateway.Dtos;
using ApiGateway.Services.External;
using Grpc.Core;
using Trading;
using Microsoft.Extensions.Caching.Memory;

namespace ApiGateway.Services
{
    public interface ILessonsService
    {
        Task<LessonDto> GetLessonByIdAsync(string id);
        Task<IEnumerable<LessonDto>> GetAllLessonsAsync();
        Task<VideoStreamDto> PrepareVideoStreamAsync(string id, string? rangeHeader);
    }

    public class LessonsService : ILessonsService
    {
        private readonly ILessonsGateway _gateway;
        private readonly IMemoryCache _cache; 

        public LessonsService(ILessonsGateway gateway, IMemoryCache cache)
        {
            _gateway = gateway;
            _cache = cache;
        }

        public async Task<LessonDto> GetLessonByIdAsync(string id)
        {
            string cacheKey = $"lesson_details_{id}";
            
            if (!_cache.TryGetValue(cacheKey, out LeccionDetalles detalles))
            {
                detalles = await _gateway.GetLessonDetailsAsync(id);
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                
                _cache.Set(cacheKey, detalles, cacheOptions);
            }
            
            return MapToDto(detalles);
        }

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync()
        {
            return await _cache.GetOrCreateAsync("all_lessons_list", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var response = await _gateway.GetAllLessonsAsync();
                return response.Lecciones.Select(MapToDto);
            });
        }

        public async Task<VideoStreamDto> PrepareVideoStreamAsync(string id, string? rangeHeader)
        {
            string cacheKey = $"lesson_details_{id}";
            
            if (!_cache.TryGetValue(cacheKey, out LeccionDetalles info))
            {
                info = await _gateway.GetLessonDetailsAsync(id);
                _cache.Set(cacheKey, info, TimeSpan.FromMinutes(30));
            }

            long totalLength = info.TotalBytes;

            long start = 0;
            long end = totalLength - 1;

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

            var streamCall = _gateway.DownloadVideoStream(id, start, end);

            return new VideoStreamDto
            {
                StreamCall = streamCall,
                Start = start,
                End = end,
                TotalLength = totalLength,
                ContentLength = contentLength
            };
        }

        private static LessonDto MapToDto(LeccionDetalles l)
        {
            return new LessonDto
            {
                Id = l.Id,
                Title = l.Titulo,
                Description = l.Descripcion,
                Thumbnail = l.Miniatura.IsEmpty ? null : Convert.ToBase64String(l.Miniatura.ToByteArray())
            };
        }
    }
}
