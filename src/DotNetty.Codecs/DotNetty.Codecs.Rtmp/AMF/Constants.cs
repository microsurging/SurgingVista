using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.AMF
{
	public struct Constants
	{
		public const int MAX_TIMESTAMP = 0XFFFFFF;
		public const int CHUNK_FMT_0 = 0;
		public const int CHUNK_FMT_1 = 1;
		public const int CHUNK_FMT_2 = 2;
		public const int CHUNK_FMT_3 = 3;

		public const int MSG_SET_CHUNK_SIZE = 1;
		public const int MSG_ABORT_MESSAGE = 2;
		public const int MSG_ACKNOWLEDGEMENT = 3;
		public const int MSG_WINDOW_ACKNOWLEDGEMENT_SIZE = 5;
		public const int MSG_SET_PEER_BANDWIDTH = 6;

		public const byte SET_PEER_BANDWIDTH_TYPE_HARD = 1;
		public const byte SET_PEER_BANDWIDTH_TYPE_SOFT = 2;
		public const byte SET_PEER_BANDWIDTH_TYPE_DYNAMIC = 3;

		public const int MSG_TYPE_COMMAND_AMF0 = 20;
		public const int MSG_TYPE_COMMAND_AMF3 = 17;

		public const int MSG_TYPE_DATA_MESSAGE_AMF0 = 18;
		public const int MSG_TYPE_DATA_MESSAGE_AMF3 = 15;

		public const int MSG_TYPE_SHARED_OBJECT_MESSAGE_AMF0 = 19;
		public const int MSG_TYPE_SHARED_OBJECT_MESSAGE_AMF3 = 16;

		public const int MSG_TYPE_AUDIO_MESSAGE = 8;
		public const int MSG_TYPE_VIDEO_MESSAGE = 9;

		public const int MSG_TYPE_AGGREGATE_MESSAGE = 22;
		public const int MSG_USER_CONTROL_MESSAGE_EVENTS = 4;


		public const byte RTMP_VERSION = 3;

		public const int DEFAULT_STREAM_ID = 5;
	}
}
