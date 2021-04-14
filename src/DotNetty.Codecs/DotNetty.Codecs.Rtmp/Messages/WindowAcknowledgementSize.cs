using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class WindowAcknowledgementSize : AbstractRtmpControlMessage
	{
		public int WindowSize { get; set; }

		public WindowAcknowledgementSize(int windowSize)
		{
			WindowSize = windowSize;
		}

		public override IByteBuffer EncodePayload()
		{
			return Unpooled.Buffer(4).WriteInt(WindowSize);
		}


		public override int GetMsgType()
		{
			return Constants.MSG_WINDOW_ACKNOWLEDGEMENT_SIZE;
		}
	}
}
