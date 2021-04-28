using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Handlers
{
    public  enum DecodeState
    {
         NONE,
        STATE_HEADER, 
        STATE_PAYLOAD
    }
}
