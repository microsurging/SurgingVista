using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using DotNetty.Codecs.Rtmp.Messages;
using DotNetty.Codecs.Rtmp.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Handlers
{
	public class ChunkEncoder : MessageToByteEncoder<AbstractRtmpMessage>
	{
		public int ChunkSize { get; set; } = 128;
		public long TimestampBegin { get; set; } = Utility.CurrentTimeMillis();

		public bool FirstVideo { get; set; } = true;
		public bool FirstAudio { get; set; } = true;

		public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
		{ 
		}

		protected override void Encode(IChannelHandlerContext ctx, AbstractRtmpMessage msg, IByteBuffer output)
		{
			if (msg is SetChunkSize)
			{
				ChunkSize = ((SetChunkSize)msg).ChunkSize;
			}

			if (msg is AudioMessage)
			{
				EncodeAudio((AudioMessage)msg, output);
			}
			else if (msg is VideoMessage)
			{
				EncodeVideo((VideoMessage)msg, output);
			}
			else
			{
				EncodeWithFmt0And3(msg, output);
			}

		}

		private void EncodeVideo(VideoMessage msg, IByteBuffer output)
		{
			if (FirstVideo)
			{
				EncodeWithFmt0And3(msg, output);
				FirstVideo = false;

			}
			else
			{
				EncodeWithFmt1(msg, output, msg.TimestampDelta??0);

			}

		}

		private void EncodeAudio(AudioMessage msg, IByteBuffer output)
		{
			if (FirstAudio)
			{
				EncodeWithFmt0And3(msg, output);
				FirstAudio = false;

			}
			else
			{
				EncodeWithFmt1(msg, output, msg.TimestampDelta??0);

			}

		}

		private void EncodeWithFmt1(AbstractRtmpMessage msg, IByteBuffer output, int timestampDelta)
		{

			int outboundCsid = msg.GetOutboundCsid();
			var buffer = Unpooled.Buffer();

			buffer.WriteBytes(EncodeFmtAndCsid(1, outboundCsid));

			var payload = msg.EncodePayload();
			buffer.WriteMedium(timestampDelta);
			buffer.WriteMedium(payload.ReadableBytes);
			buffer.WriteByte(msg.GetMsgType());

			var fmt1Part = true;
			while (payload.IsReadable())
			{
				int min = Math.Min(ChunkSize, payload.ReadableBytes);

				if (fmt1Part)
				{
					buffer.WriteBytes(payload, min);
					fmt1Part = false;
				}
				else
				{
					byte[] fmt3BasicHeader = EncodeFmtAndCsid(Constants.CHUNK_FMT_3, outboundCsid);
					buffer.WriteBytes(fmt3BasicHeader);
					buffer.WriteBytes(payload, min);

				}
				output.WriteBytes(buffer);
				buffer = Unpooled.Buffer();
			}

		}

		private void EncodeWithFmt0And3(AbstractRtmpMessage msg, IByteBuffer output)
		{
			int csid = msg.GetOutboundCsid();

			byte[] basicHeader = EncodeFmtAndCsid(0, csid);

			// as for control msg ,we always use 0 timestamp

			var payload = msg.EncodePayload();
			int messageLength = payload.ReadableBytes;
			var buffer = Unpooled.Buffer();

			buffer.WriteBytes(basicHeader);

			long timestamp = GetRelativeTime();
			var needExtraTime = false;
			if (timestamp >= Constants.MAX_TIMESTAMP)
			{
				needExtraTime = true;
				buffer.WriteMedium(Constants.MAX_TIMESTAMP);
			}
			else
			{
				buffer.WriteMedium((int)timestamp);
			}
			// message length
			buffer.WriteMedium(messageLength);

			buffer.WriteByte(msg.GetMsgType());
			if (msg is UserControlMessageEvent)
			{
				// message stream id in UserControlMessageEvent is always 0
				buffer.WriteIntLE(0);
			}
			else
			{
				buffer.WriteIntLE(Constants.DEFAULT_STREAM_ID);
			}

			if (needExtraTime)
			{
				buffer.WriteInt((int)(timestamp));
			}
			// split by chunk size

			var fmt0Part = true;
			while (payload.IsReadable())
			{
				int min = Math.Min(ChunkSize, payload.ReadableBytes);
				if (fmt0Part)
				{
					buffer.WriteBytes(payload, min);
					fmt0Part = false;
				}
				else
				{
					byte[] fmt3BasicHeader = EncodeFmtAndCsid(Constants.CHUNK_FMT_3, csid);
					buffer.WriteBytes(fmt3BasicHeader);
					buffer.WriteBytes(payload, min);

				}
				output.WriteBytes(buffer);
				buffer = Unpooled.Buffer();
			}
		}

		public long BetRelativeTime()
		{
			return Utility.CurrentTimeMillis() - TimestampBegin;
		}

		private static byte[] EncodeFmtAndCsid(int fmt, int csid)
		{
			if (csid <= 63)
			{
				return new byte[] { (byte)((fmt << 6) + csid) };
			}
			else if (csid <= 320)
			{
				return new byte[] { (byte)(fmt << 6), (byte)(csid - 64) };
			}
			else
			{
				return new byte[] { (byte)((fmt << 6) | 1), (byte)((csid - 64) & 0xff), (byte)((csid - 64) >> 8) };
			}
		} 

		public long GetRelativeTime()
		{
			return Utility.CurrentTimeMillis() - TimestampBegin;
		} 
	}
}
