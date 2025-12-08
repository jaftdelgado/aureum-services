using Grpc.Core;
using Trading; 

namespace ApiGateway.Dtos
{
	public class VideoStreamDto
	{
		public AsyncServerStreamingCall<VideoChunk> StreamCall { get; set; } = null!;
		public long Start { get; set; }
		public long End { get; set; }
		public long TotalLength { get; set; }
		public long ContentLength { get; set; }
	}
}