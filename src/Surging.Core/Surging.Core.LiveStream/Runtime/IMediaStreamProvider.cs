using DotNetty.Codecs.Rtmp.Stream;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream.Runtime
{
    public  interface IMediaStreamProvider
    {
        ConcurrentDictionary<StreamName, MediaStream> GetMediaStream();
    }
}
