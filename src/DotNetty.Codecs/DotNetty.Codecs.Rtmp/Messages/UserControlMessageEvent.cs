using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class UserControlMessageEvent : AbstractRtmpMessage
	{
		public short EventType;
		public int Data;

		public UserControlMessageEvent(short eventType, int data)
		{
			EventType = eventType;
			Data = data;
		}


		public override IByteBuffer EncodePayload()
		{
			var buffer = Unpooled.Buffer(6);
			buffer.WriteShort(EventType);
			buffer.WriteInt(Data);
			return buffer;
		}


		public override int GetOutboundCsid()
		{
			return 2;
		}


		public override int GetMsgType()
		{
			return Constants.MSG_USER_CONTROL_MESSAGE_EVENTS;
		}

		public static UserControlMessageEvent StreamBegin(int streamId)
		{
			UserControlMessageEvent e = new UserControlMessageEvent((short)0, streamId);
			return e;
		}

		public static UserControlMessageEvent StreamEOF(int streamId)
		{
			UserControlMessageEvent e = new UserControlMessageEvent((short)1, streamId);
			return e;
		}

		public static UserControlMessageEvent StreamDry(int streamId)
		{
			UserControlMessageEvent e = new UserControlMessageEvent((short)2, streamId);
			return e;
		}

		public static UserControlMessageEvent SetBufferLength(int bufferLengthInms)
		{
			UserControlMessageEvent e = new UserControlMessageEvent((short)3, bufferLengthInms);
			return e;
		}

		public bool IsBufferLength()
		{
			return EventType == 3;
		}

	}
}
