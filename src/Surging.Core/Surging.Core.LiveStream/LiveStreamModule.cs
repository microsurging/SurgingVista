using Autofac;
using DotNetty.Codecs.Rtmp.Stream;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.LiveStream.Configurations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream
{
    class LiveStreamModule : EnginePartModule
    {
        private readonly ConcurrentDictionary<StreamName, MediaStream> mediaStreamDic =
            new ConcurrentDictionary<StreamName, MediaStream>();
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
            base.RegisterBuilder(builder);
            RegisterRtmpProtocol(builder, options);
            RegisterHttpFlvProtocol(builder, options);
        }


        private void RegisterRtmpProtocol(ContainerBuilderWrapper builder, LiveStreamOption options)
        {
            builder.Register(provider =>
            {
                return new RtmpMessageListener(provider.Resolve<ILogger<RtmpMessageListener>>(),
                   options.EnableLog? provider.Resolve<ILoggerFactory>() : new LoggerFactory(),
                      mediaStreamDic
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<RtmpMessageListener>();
                return new RtmpServiceHost(async endPoint =>
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
                      mediaStreamDic
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
