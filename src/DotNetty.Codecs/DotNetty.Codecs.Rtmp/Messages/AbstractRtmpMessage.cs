using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
   public abstract class AbstractRtmpMessage
	{
		 
		public int InboundHeaderLength { get; set; }
		public int InboundBodyLength { get; set; }

		public abstract int GetOutboundCsid();

		public abstract int GetMsgType();

		public abstract IByteBuffer EncodePayload();

	}
}
