using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class RtmpDataMessage : AbstractRtmpMessage
	{
		public List<Object> Data;

		public RtmpDataMessage(List<Object> data)
		{
			Data = data;
		}


		public override IByteBuffer EncodePayload()
		{
			var buffer = Unpooled.Buffer();
			AMF0.Encode(buffer, Data);
			return buffer;
		}


		public override int GetOutboundCsid()
		{
			return 3;
		}

		public override int GetMsgType()
		{
			return Constants.MSG_TYPE_DATA_MESSAGE_AMF0;
		}
	}
}
