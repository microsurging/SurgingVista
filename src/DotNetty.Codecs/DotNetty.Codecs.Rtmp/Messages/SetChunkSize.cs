using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class SetChunkSize : AbstractRtmpControlMessage
	{

		public int ChunkSize { get; set; }

		public SetChunkSize(int chunkSize)
		{
			ChunkSize = chunkSize;
		}

		public SetChunkSize()
		{ 
		}


		public override IByteBuffer EncodePayload()
		{
			return Unpooled.Buffer(4).WriteInt(ChunkSize);

		}

		public override int GetMsgType()
		{
			return Constants.MSG_SET_CHUNK_SIZE;
		}
	}
}
