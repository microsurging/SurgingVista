using DotNetty.Buffers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	[MessagePack.Union(0, typeof(RtmpCommandMessage))]
	[MessagePack.Union(1, typeof(AbstractRtmpMediaMessage))]
	[MessagePack.Union(2, typeof(AudioMessage))]
	[MessagePack.Union(3, typeof(VideoMessage))]
	public abstract class AbstractRtmpMessage
	{
		[Key(0)]
		public int InboundHeaderLength { get; set; }

		[Key(1)]
		public int InboundBodyLength { get; set; }

		public abstract int GetOutboundCsid();

		public abstract int GetMsgType();

		public abstract IByteBuffer EncodePayload();

	}
}
