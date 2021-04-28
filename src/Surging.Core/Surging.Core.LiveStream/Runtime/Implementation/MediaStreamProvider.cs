using DotNetty.Codecs.Rtmp.Stream;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream.Runtime.Implementation
{
    public class MediaStreamProvider : IMediaStreamProvider
    {

        private readonly ConcurrentDictionary<StreamName, MediaStream> _mediaStreamDic;

        public MediaStreamProvider()
        {
            _mediaStreamDic = new ConcurrentDictionary<StreamName, MediaStream>();
        }
        public ConcurrentDictionary<StreamName, MediaStream> GetMediaStream()
        {
            return _mediaStreamDic;
        }
    }
}
