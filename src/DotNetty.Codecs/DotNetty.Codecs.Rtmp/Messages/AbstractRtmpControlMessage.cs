using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Messages
{
   public abstract class AbstractRtmpControlMessage:AbstractRtmpMessage
    {
        public override int GetOutboundCsid()
        {
            return 2;
        }
    }
}
