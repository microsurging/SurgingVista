using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	[MessagePackObject]
	public class RtmpCommandMessage : AbstractRtmpMessage
	{
		[Key(2)]
		public List<Object> Command { get; set; }

		[SerializationConstructor]
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
