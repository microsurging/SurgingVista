using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp
{
    public  class RtmpConfig
    {
        public static RtmpConfig Instance { get; internal set; }

  

        public bool IsSaveFlvFile { get; set; }

        public string SaveFlvFilePath { get; set; }

        public string App { get; set; }
    }
}
