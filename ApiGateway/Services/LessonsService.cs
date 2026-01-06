using ApiGateway.Dtos;
using ApiGateway.Services.External;
using Microsoft.Extensions.Caching.Memory;
using Grpc.Core;
using Trading;

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

    long totalLength = info.TotalBytes > 0 ? info.TotalBytes - 1 : 0;

    long start = 0;
    long end = totalLength - 1;

    if (!string.IsNullOrEmpty(rangeHeader))
    {
        var rangeValue = rangeHeader.Replace("bytes=", "").Trim();
        var parts = rangeValue.Split('-');

        if (!string.IsNullOrEmpty(parts[0]))
        {
            start = long.Parse(parts[0]);
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                end = long.Parse(parts[1]);
            }
        }
        else if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
        {
            long suffixLength = long.Parse(parts[1]);
            start = totalLength - suffixLength;
            if (start < 0) start = 0;
        }
    }

    if (end >= totalLength) end = totalLength - 1;
    if (start > end) start = end;

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
    }
}
