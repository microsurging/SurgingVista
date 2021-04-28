using DotNetty.Codecs.Rtmp.Messages;
using DotNetty.Codecs.Rtmp.Stream;
using Surging.Core.CPlatform.Ioc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.LiveStream.Runtime.Implementation
{
    public class LiveRomtePublishService : ServiceBase, ILiveRomtePublishService
    {
        private readonly IMediaStreamProvider _mediaStreamProvider;
        public LiveRomtePublishService(IMediaStreamProvider mediaStreamProvider)
        {
            _mediaStreamProvider = mediaStreamProvider;
        }

        public async Task<bool> Publish(StreamName key, AbstractRtmpMessage message)
        {
            var mediaStreams = _mediaStreamProvider.GetMediaStream();
            mediaStreams.TryGetValue(key, out MediaStream mediaStream);
            if (mediaStream == null)
            {
                mediaStream = new MediaStream(key);
                mediaStreams.TryAdd(key, mediaStream); 
            }
            if (message is RtmpCommandMessage)
            {
                var msg = message as RtmpCommandMessage;
                var command = msg.Command;
                var commandName = (String)command[0].ToString();
                switch (commandName)
                {
                    case "publish":
                        Publish(mediaStreams, msg);
                        break;
                    case "closeStream":
                      await  mediaStream.SendEofToAllSubscriberAndClose();
                        break;
                    default:
                        break;
                }
            }
            if (message is AbstractRtmpMediaMessage)
            { 
                mediaStream.AddContent((AbstractRtmpMediaMessage)message,true);
            }
            return await Task.FromResult(true);
        }

        public void  Publish(ConcurrentDictionary<StreamName, MediaStream> streams, RtmpCommandMessage msg)
        {
            var streamName = new StreamName(msg.Command[4].ToString(), msg.Command[3].ToString(), false);
            streams.GetOrAdd(streamName, new MediaStream(streamName));
        }
    }
}