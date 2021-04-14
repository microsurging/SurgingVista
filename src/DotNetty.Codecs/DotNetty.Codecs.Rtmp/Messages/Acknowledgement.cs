using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class Acknowledgement : AbstractRtmpControlMessage
	{

		public int SequnceNumber { get; set; }

		public Acknowledgement(int sequnceNum)
		{
			SequnceNumber = sequnceNum;
		}


		public override IByteBuffer EncodePayload()
		{
			return Unpooled.Buffer(4).WriteInt(SequnceNumber);
		}


		public override int GetMsgType()
		{
			return Constants.MSG_ACKNOWLEDGEMENT;
		}
	}
}
