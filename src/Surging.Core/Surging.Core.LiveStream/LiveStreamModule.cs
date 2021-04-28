using Autofac;
using DotNetty.Codecs.Rtmp.Stream;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.LiveStream.Configurations;
using Surging.Core.LiveStream.Runtime;
using Surging.Core.LiveStream.Runtime.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream
{
    class LiveStreamModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var options = new LiveStreamOption();
            var section = CPlatform.AppConfig.GetSection("LiveStream");
            if (section.Exists())
                options = section.Get<LiveStreamOption>();
            AppConfig.Option = options;
            builder.RegisterType(typeof(RtmpRemoteInvokeService)).As(typeof(IRtmpRemoteInvokeService)).SingleInstance();
            builder.RegisterType(typeof(MediaStreamProvider)).As(typeof(IMediaStreamProvider)).SingleInstance();
            builder.RegisterType(typeof(LiveStreamServiceExecutor)).Named("Rtmp",typeof(IServiceExecutor)).SingleInstance();
            base.RegisterBuilder(builder);
            RegisterRtmpProtocol(builder, options);
            if(AppConfig.Option.EnableHttpFlv)
            RegisterHttpFlvProtocol(builder, options);
        }


        private void RegisterRtmpProtocol(ContainerBuilderWrapper builder, LiveStreamOption options)
        {
            builder.Register(provider =>
            {
                return new RtmpMessageListener(provider.Resolve<ILogger<RtmpMessageListener>>(),
                   options.EnableLog? provider.Resolve<ILoggerFactory>() : new LoggerFactory(),
                      provider.Resolve<IMediaStreamProvider>().GetMediaStream()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<RtmpMessageListener>();
                var serviceExecutor = provider.ResolveNamed<IServiceExecutor>("Rtmp");
                return new RtmpServiceHost(serviceExecutor,async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }

        private void RegisterHttpFlvProtocol(ContainerBuilderWrapper builder, LiveStreamOption options)
        {
            builder.Register(provider =>
            {
                return new HttpFlvMessageListener(provider.Resolve<ILogger<HttpFlvMessageListener>>(),
                       provider.Resolve<IMediaStreamProvider>().GetMediaStream()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<HttpFlvMessageListener>();
                return new HttpFlvServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }
    }
}
