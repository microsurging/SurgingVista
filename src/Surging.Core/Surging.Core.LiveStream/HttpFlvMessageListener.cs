using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Rtmp.Handlers;
using DotNetty.Codecs.Rtmp.Stream;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.LiveStream
{
   public class HttpFlvMessageListener : IMessageListener, IDisposable
    {
        private readonly ILogger<HttpFlvMessageListener> _logger;
        private IChannel _channel;
        private readonly ConcurrentDictionary<StreamName, MediaStream> _mediaStreamDic;

        public HttpFlvMessageListener(ILogger<HttpFlvMessageListener> logger, ConcurrentDictionary<StreamName, MediaStream> mediaStreamDic)
        {
            _logger = logger;
            _mediaStreamDic = mediaStreamDic;
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            var bootstrap = new ServerBootstrap();
            var bossGroup = new MultithreadEventLoopGroup();
            var workerGroup = new MultithreadEventLoopGroup();
            bootstrap.Channel<TcpServerSocketChannel>();
            bootstrap
            .Option(ChannelOption.SoBacklog, 128)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
              .ChildOption(ChannelOption.SoKeepalive, true)
            .Group(bossGroup)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
                pipeline.AddLast(new HttpRequestDecoder());
                pipeline.AddLast(new HttpResponseEncoder()); 
                pipeline.AddLast(new HttpFlvHandler(_mediaStreamDic));

            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"HttpFlv服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"HttpFlv服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        public event ReceivedDelegate Received;

        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }


        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}