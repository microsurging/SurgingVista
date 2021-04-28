using DotNetty.Codecs.Rtmp.Messages;
using DotNetty.Codecs.Rtmp.Stream;
using Surging.Core.CPlatform.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.LiveStream.Runtime
{
    [LiveStreamServiceBundleAttribute()]
    public interface ILiveRomtePublishService:IServiceKey
    {
         Task<bool> Publish(StreamName key, AbstractRtmpMessage message);
    }
}
