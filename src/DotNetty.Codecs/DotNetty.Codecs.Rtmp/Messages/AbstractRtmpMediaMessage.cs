using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
    public abstract class AbstractRtmpMediaMessage : AbstractRtmpMessage
    {
        public int? TimestampDelta { get; set; }
        public int? Timestamp { get; set; }

        public abstract byte[] Raw();
    }
}
