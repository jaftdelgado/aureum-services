using ApiGateway.Dtos;
using ApiGateway.Services.External;
using Grpc.Core;
using Trading;
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks;

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
        private static readonly Dictionary<string, long> _videoSizeCache = new();

        public LessonsService(ILessonsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<LessonDto> GetLessonByIdAsync(string id)
        {
            var response = await _gateway.GetLessonDetailsAsync(id);
            return MapToDto(response);
        }

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync()
        {
            var response = await _gateway.GetAllLessonsAsync();
            return response.Lecciones.Select(MapToDto);
        }

        public async Task<VideoStreamDto> PrepareVideoStreamAsync(string id, string? rangeHeader)
        {
            if (!_videoSizeCache.TryGetValue(id, out long totalLength))
            {
                var info = await _gateway.GetLessonDetailsAsync(id);
                totalLength = info.TotalBytes;
                
                _videoSizeCache[id] = totalLength;
            }

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
