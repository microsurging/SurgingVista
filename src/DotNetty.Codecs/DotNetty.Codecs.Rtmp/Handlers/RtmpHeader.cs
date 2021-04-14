using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Handlers
{
   public class RtmpHeader
    {
		public int Csid { get; set; }
		public int Fmt { get; set; }
		public int Timestamp { get; set; }

		public int MessageLength { get; set; }
		public short MessageTypeId { get; set; }
		public int MessageStreamId { get; set; }

		public int TimestampDelta { get; set; }

		public long ExtendedTimestamp { get; set; }
		 
		public int HeaderLength { get; set; }

	}
}
