using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	[MessagePackObject]
	public class VideoMessage : AbstractRtmpMediaMessage
	{
		[Key(4)]
		public byte[] VideoData { get; set; }

		public VideoMessage(byte[] videoData)
		{
			VideoData = videoData;
		}

		public VideoMessage()
		{

		}

		public override IByteBuffer EncodePayload()
		{

			return Unpooled.WrappedBuffer(VideoData);
		}


		public override int GetOutboundCsid()
		{

			return 12;
		}


		public override int GetMsgType()
		{
			return Constants.MSG_TYPE_VIDEO_MESSAGE;
		}

		public bool IsH264KeyFrame()
		{
			return VideoData.Length > 1 && VideoData[0] == 0x17;
		}

		public bool IsAVCDecoderConfigurationRecord()
		{
			return IsH264KeyFrame() && VideoData.Length > 2 && VideoData[1] == 0x00;
		}


		public override byte[] Raw()
		{
			return VideoData;
		}


		public override string ToString()
		{
			return "VideoMessage [timestampDelta=" + TimestampDelta + ", timestamp=" + Timestamp + ", inboundHeaderLength="
					+ InboundHeaderLength + ", inboundBodyLength=" + InboundBodyLength + "]";
		}

	}
}
