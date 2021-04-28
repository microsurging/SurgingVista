using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.LiveStream.Runtime
{
   public interface IRtmpRemoteInvokeService
    {
        Task InvokeAsync(RemoteInvokeMessage message, int requestTimeout); 
    }
}
