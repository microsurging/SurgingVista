using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{ 
	public class SetPeerBandWidth : AbstractRtmpControlMessage
	{
		public int AcknowledgementWindowSize { get; set; }
		public int LimitType { get; set; }

		public SetPeerBandWidth(int acknowledgementWindowSize,int limitType)
		{
			AcknowledgementWindowSize = acknowledgementWindowSize;
			LimitType = limitType;
		}

		public override IByteBuffer EncodePayload()
		{
			return Unpooled.Buffer(5).WriteInt(AcknowledgementWindowSize).WriteByte(LimitType);

		}

		public override int GetMsgType()
		{
			return Constants.MSG_SET_PEER_BANDWIDTH;
		}
	}
}

