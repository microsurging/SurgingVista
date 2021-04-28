using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.LiveStream.Runtime.Implementation
{
    public class RtmpRemoteInvokeService : IRtmpRemoteInvokeService
    {
        private readonly IServiceRouteProvider _serviceRotueProvider;
        private readonly ILogger<RtmpRemoteInvokeService> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly string _routePath;
        public RtmpRemoteInvokeService(IServiceRouteProvider serviceRotueProvider, ILogger<RtmpRemoteInvokeService> logger,
            IHealthCheckService healthCheckService,
           ITransportClientFactory transportClientFactory)
        {
            _serviceRotueProvider = serviceRotueProvider;
            _logger = logger;
            _healthCheckService = healthCheckService;
            _transportClientFactory = transportClientFactory;
            _routePath = RoutePatternParser.Parse(AppConfig.Option.RouteTemplate, "ILiveRomtePublishService", "Publish");
        }
        public async Task InvokeAsync(RemoteInvokeMessage message, int requestTimeout)
        {

            var host = NetUtils.GetHostAddress();
            AddressModel address = host;
            var route = await _serviceRotueProvider.GetRouteByPath(_routePath);
            if (route != null && route.Address.Count() > 1)
            {
                try
                {
                    var addresses = route.Address.ToList();
                    var index = addresses.IndexOf(host);
                    for (var i = 1; i < AppConfig.Option.ClusterNode; i++)
                    {

                        ++index;
                        if (addresses.Count == index)
                            index = 0;
                        address = addresses[index];
                        if (host == address)
                            break;
                        var endPoint = address.CreateEndPoint();
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");
                        var client = await _transportClientFactory.CreateClientAsync(endPoint);
                        using (var cts = new CancellationTokenSource())
                        {
                            await client.SendAsync(message, cts.Token).WithCancellation(cts, requestTimeout);
                        }
                    }
                }
                catch (CommunicationException ex)
                {
                    await _healthCheckService.MarkFailure(address);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"发起live请求中发生了错误，服务Id：{message.ServiceId}。");

                }
            }

        }

    }
}
