using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.AMF;
using DotNetty.Codecs.Rtmp.Handlers;
using DotNetty.Codecs.Rtmp.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp
{
   public class RtmpMessageDecoder
    {

		public static AbstractRtmpMessage Decode(RtmpHeader header, IByteBuffer payload)
		{

			AbstractRtmpMessage result = null;
			short messageTypeId = header.MessageTypeId;

			switch (messageTypeId)
			{
				case Constants.MSG_SET_CHUNK_SIZE:
					{
						int readInt = payload.ReadInt();
						SetChunkSize setChunkSize = new SetChunkSize();
						setChunkSize.ChunkSize=readInt;
						result = setChunkSize;

					}
					break;
				case Constants.MSG_ABORT_MESSAGE:
					{
						int csid = payload.ReadInt();
						Abort abort = new Abort(csid);
						result = abort;
					}

					break;
				case Constants.MSG_ACKNOWLEDGEMENT:
					{
						int ack = payload.ReadInt();
						result = new Acknowledgement(ack);
					}
					break;

				case Constants.MSG_WINDOW_ACKNOWLEDGEMENT_SIZE:
					{
						int size = payload.ReadInt();
						result = new WindowAcknowledgementSize(size);

					}
					break;

				case Constants.MSG_SET_PEER_BANDWIDTH:
					{
						int ackSize = payload.ReadInt();
						int type = payload.ReadByte();
						result = new SetPeerBandWidth(ackSize, type);
					}
					break;

				case Constants.MSG_TYPE_COMMAND_AMF0:
					{
						List<Object> decode = AMF0.DecodeAll(payload);
						result = new RtmpCommandMessage(decode);

					}
					break;

				case Constants.MSG_USER_CONTROL_MESSAGE_EVENTS:
					{
						short readShort = payload.ReadShort();
						int data = payload.ReadInt();
						result = new UserControlMessageEvent(readShort, data);
					}
					break;

				case Constants.MSG_TYPE_AUDIO_MESSAGE:
					{
						AudioMessage am = new AudioMessage();

						byte[] data = ReadAll(payload);
						am.AudioData=data;

						if (header.Fmt == Constants.CHUNK_FMT_0)
						{
							am.Timestamp=header.Timestamp;
						}
						else if (header.Fmt == Constants.CHUNK_FMT_1 || header.Fmt == Constants.CHUNK_FMT_2)
						{
							am.TimestampDelta=header.TimestampDelta;
						}
						result = am;
					}
					break;
				case Constants.MSG_TYPE_VIDEO_MESSAGE:
					{
						VideoMessage vm = new VideoMessage();
						byte[] data = ReadAll(payload);

						vm.VideoData= data;

						if (header.Fmt == Constants.CHUNK_FMT_0)
						{
							vm.Timestamp=header.Timestamp;
						}
						else if (header.Fmt == Constants.CHUNK_FMT_1 || header.Fmt == Constants.CHUNK_FMT_2)
						{
							vm.TimestampDelta=header.TimestampDelta;
						}
						result = vm;
					}
					break;
				case Constants.MSG_TYPE_DATA_MESSAGE_AMF0:
					{
						result = new RtmpDataMessage(AMF0.DecodeAll(payload));
					}
					break;

				default:
					break;
			}
			if (result != null)
			{
				result.InboundBodyLength=header.MessageLength;
				result.InboundHeaderLength=header.HeaderLength;

				return result;
			}
			else
			{
				return null;
			}
		}

		private static byte[] ReadAll(IByteBuffer payload)
		{
			byte[] all = new byte[payload.ReadableBytes];
			payload.ReadBytes(all);
			return all;
		}
	}
}
