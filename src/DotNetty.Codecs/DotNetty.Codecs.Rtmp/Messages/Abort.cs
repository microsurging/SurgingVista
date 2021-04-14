using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
	public class Abort : AbstractRtmpControlMessage
	{
		public int Csid { get; set; }


		public Abort(int csid)
		{
			Csid = csid;
		}


		public override IByteBuffer EncodePayload()
		{
			return Unpooled.Buffer(4).WriteInt(Csid);
		}


		public override int GetMsgType()
		{
			return Constants.MSG_ABORT_MESSAGE;
		}
	}
}
