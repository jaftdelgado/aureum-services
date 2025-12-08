using Grpc.Core;
using Trading; 

namespace ApiGateway.Services.External
{
    public interface ILessonsGateway
    {
        Task<LeccionDetalles> GetLessonDetailsAsync(string id);
        Task<ListaLecciones> GetAllLessonsAsync();
        AsyncServerStreamingCall<VideoChunk> DownloadVideoStream(string id, long start, long end);
    }

    public class LessonsGrpcGateway : ILessonsGateway
    {
        private readonly LeccionesService.LeccionesServiceClient _client;

        public LessonsGrpcGateway(LeccionesService.LeccionesServiceClient client)
        {
            _client = client;
        }

        public async Task<LeccionDetalles> GetLessonDetailsAsync(string id)
        {
            var request = new LeccionRequest { IdLeccion = id };
            return await _client.ObtenerDetallesAsync(request);
        }

        public async Task<ListaLecciones> GetAllLessonsAsync()
        {
            return await _client.ObtenerTodasAsync(new Empty());
        }

        public AsyncServerStreamingCall<VideoChunk> DownloadVideoStream(string id, long start, long end)
        {
            var request = new DescargaRequest
            {
                IdLeccion = id,
                StartByte = start,
                EndByte = end
            };
            return _client.DescargarVideo(request);
        }
    }
}