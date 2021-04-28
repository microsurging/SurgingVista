using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream.Runtime
{
    [AttributeUsage(AttributeTargets.Interface)]
    public  class LiveStreamServiceBundleAttribute : ServiceBundleAttribute
    {
        public LiveStreamServiceBundleAttribute():base(AppConfig.Option.RouteTemplate,true)
        { 
        } 
    }
}