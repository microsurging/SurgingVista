using DotNetty.Codecs.Rtmp.Messages;
using DotNetty.Codecs.Rtmp.Stream;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.LiveStream.Runtime.Implementation
{
    public class LiveStreamServiceExecutor : IServiceExecutor
    {
        private readonly IRtmpRemoteInvokeService _rtmpRemoteInvokeService;
        private readonly string _publishServiceId;
        public LiveStreamServiceExecutor(
          IRtmpRemoteInvokeService rtmpRemoteInvokeService,
          IServiceIdGenerator serviceIdGenerator
          )
        {
            _rtmpRemoteInvokeService = rtmpRemoteInvokeService;
            _publishServiceId = serviceIdGenerator.GenerateServiceId(typeof(ILiveRomtePublishService).GetMethod("Publish"));
        }

        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            var rtmpMessages = message.GetContent<Dictionary<StreamName, AbstractRtmpMessage>>();
            foreach (var rtmpMediaMessage in rtmpMessages)
            {
                await _rtmpRemoteInvokeService.InvokeAsync(
                    new RemoteInvokeMessage
                    {
                        Attachments = RpcContext.GetContext().GetContextParameters(),
                        ServiceId = _publishServiceId,
                        Parameters = new Dictionary<string, object>() {
                           {"key",rtmpMediaMessage.Key},
                           { "message",rtmpMediaMessage.Value}
                          }
                    }, CPlatform.AppConfig.ServerOptions.ExecutionTimeoutInMilliseconds);
            }
        }
    }
}
