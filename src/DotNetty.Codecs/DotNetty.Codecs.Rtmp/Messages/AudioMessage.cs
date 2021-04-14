using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class AudioMessage : AbstractRtmpMediaMessage
	{
		public byte[] AudioData { get; set; }

		public AudioMessage(byte[] audioData)
		{
			AudioData = audioData;
		}

		public AudioMessage()
		{
		}


		public override int GetOutboundCsid()
		{
			return 10;
		}


		public override IByteBuffer EncodePayload()
		{
			return Unpooled.WrappedBuffer(AudioData);
		}


		public override int GetMsgType()
		{
			return Constants.MSG_TYPE_AUDIO_MESSAGE;
		}


		public override byte[] Raw()
		{

			return AudioData;
		}

		public bool IsAACAudioSpecificConfig()
		{
			return AudioData.Length > 1 && AudioData[1] == 0;
		}


		public override String ToString()
		{
			return "AudioMessage [audioData=" + String.Join(" ", AudioData) + ", timestampDelta=" + this.TimestampDelta
					+ ", timestamp=" + this.Timestamp + ", inboundHeaderLength=" + this.InboundHeaderLength + ", inboundBodyLength="
					+ this.InboundBodyLength + "]";
		}
	}
}
