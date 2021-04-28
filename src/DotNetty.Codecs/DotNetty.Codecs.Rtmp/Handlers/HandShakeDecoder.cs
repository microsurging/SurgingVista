using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Handlers
{
	public class HandShakeDecoder : ByteToMessageDecoder
	{
		private bool _c0c1done;

		private bool _c2done;

		static int HANDSHAKE_LENGTH = 1536;
		static int VERSION_LENGTH = 1;

		// server rtmp version
		static byte S0 = 3;

		byte[] CLIENT_HANDSHAKE = new byte[HANDSHAKE_LENGTH];
		private bool _handshakeDone;


		protected override void Decode(IChannelHandlerContext ctx, IByteBuffer input, List<Object> output)
		{

			if (_handshakeDone)
			{
				ctx.FireChannelRead(input);
				return;
			}

			var buf = input;
			if (!_c0c1done)
			{
				// read c0 and c1
				if (buf.ReadableBytes < VERSION_LENGTH + HANDSHAKE_LENGTH)
				{
					return;
				}

				buf.ReadByte();

				buf.ReadBytes(CLIENT_HANDSHAKE);

				WriteS0S1S2(ctx);
				_c0c1done = true;

			}
			else
			{ 
				if (buf.ReadableBytes < HANDSHAKE_LENGTH)
				{
					return;
				}

				buf.ReadBytes(CLIENT_HANDSHAKE); 
				CLIENT_HANDSHAKE = null;
				_handshakeDone = true;
				ctx.Channel.Pipeline.Remove(this);
			}

		}


		public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
		{ 
		}

		private void WriteS0S1S2(IChannelHandlerContext ctx)
		{
			// S0+S1+S2
			var responseBuf = Unpooled.Buffer(VERSION_LENGTH + HANDSHAKE_LENGTH + HANDSHAKE_LENGTH);
			// version = 3
			responseBuf.WriteByte(S0);
			// s1 time
			responseBuf.WriteInt(0);
			// s1 zero
			responseBuf.WriteInt(0);
			// s1 random bytes
			responseBuf.WriteBytes(Utility.GenerateRandomData(HANDSHAKE_LENGTH - 8));
			// s2 time
			responseBuf.WriteInt(0);
			// s2 time2
			responseBuf.WriteInt(0);
			// s2 random bytes
			responseBuf.WriteBytes(Utility.GenerateRandomData(HANDSHAKE_LENGTH - 8)); 
			ctx.WriteAndFlushAsync(responseBuf);
		}
	}
}