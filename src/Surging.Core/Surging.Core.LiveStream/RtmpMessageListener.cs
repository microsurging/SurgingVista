﻿using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Messages;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Buffers;
using Surging.Core.LiveStream.Adapter;
using DotNetty.Codecs.Rtmp.Handlers;
using System.Collections.Concurrent;
using DotNetty.Codecs.Rtmp.Stream;
using DotNetty.Common.Internal.Logging;
using DotNetty.Codecs.Rtmp.Messages;
using DotNetty.Codecs.Rtmp;

namespace Surging.Core.LiveStream
{
  public  class RtmpMessageListener : IMessageListener, IDisposable
    { 
        private readonly ILogger<RtmpMessageListener> _logger;
        private IChannel _channel;
        private readonly ConcurrentDictionary<StreamName, MediaStream> _mediaStreamDic;

        public RtmpMessageListener(ILogger<RtmpMessageListener> logger , ILoggerFactory loggerFactory, ConcurrentDictionary<StreamName, MediaStream> mediaStreamDic)
        {
            _logger = logger;
            _mediaStreamDic = mediaStreamDic;
            InternalLoggerFactory.DefaultFactory = loggerFactory;
        }

        public async Task StartAsync(EndPoint endPoint)
        {
            Environment.SetEnvironmentVariable("io.netty.allocator.numDirectArenas", "0");

            var bootstrap = new ServerBootstrap();
            if (AppConfig.Option.DisablePooled)
                bootstrap = bootstrap.Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default);
            var bossGroup = new MultithreadEventLoopGroup();
            var workerGroup = new MultithreadEventLoopGroup();
            bootstrap.Channel<TcpServerSocketChannel>();
            bootstrap
            .Option(ChannelOption.SoBacklog, 128)
              .ChildOption(ChannelOption.SoKeepalive, true)
            .Group(bossGroup)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
                pipeline.AddLast(new ConnectionChannelHandlerAdapter(_logger));
                pipeline.AddLast(new HandShakeDecoder());
                pipeline.AddLast(new ChunkDecoder());
                pipeline.AddLast(new ChunkEncoder());
                pipeline.AddLast(workerGroup, new RtmpMessageHandler(_mediaStreamDic, async (contenxt, key, message) =>
                  {
                      var dic = new Dictionary<StreamName, AbstractRtmpMessage>();
                      dic.Add(key, message);
                      await OnReceived(null, new TransportMessage(dic));
                      message = null;
                  }, new RtmpConfig
                  {
                      App = AppConfig.Option.RouteTemplate,
                      IsSaveFlvFile = AppConfig.Option.IsSaveFlvFile,
                      SaveFlvFilePath = AppConfig.Option.SaveFlvFilePath
                  }));

            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Rtmp服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"Rtmp服务主机启动失败，监听地址：{endPoint}。 ");
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


        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        public void Dispose()
        {
        }
    }
}
