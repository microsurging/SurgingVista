using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class RtmpCommandMessage : AbstractRtmpMessage
	{
		public List<Object> Command { get; set; }

		public RtmpCommandMessage(List<Object> command)
		{
			Command = command;
		}

		public override int GetOutboundCsid()
		{
			return 3;
		}


		public override IByteBuffer EncodePayload()
		{
			IByteBuffer buffer = Unpooled.Buffer();
			AMF0.Encode(buffer, Command);
			return buffer;
		}


		public override int GetMsgType()
		{
			return Constants.MSG_TYPE_COMMAND_AMF0;

		}
	}
}
