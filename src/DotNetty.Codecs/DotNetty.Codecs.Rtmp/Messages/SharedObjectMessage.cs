using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class SharedObjectMessage : AbstractRtmpMessage
	{
		public List<Object> Body { get; set; }

		public SharedObjectMessage(List<Object> body)
		{
			Body = body;
		}

		public override IByteBuffer EncodePayload()
		{
			return null;
		}


		public override int GetOutboundCsid()
		{
			return 4;
		}


		public override int GetMsgType()
		{
			return Constants.MSG_TYPE_SHARED_OBJECT_MESSAGE_AMF0;
		}
	}
}
