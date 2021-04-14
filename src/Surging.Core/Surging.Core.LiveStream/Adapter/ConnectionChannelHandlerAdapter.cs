using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream.Adapter
{
	public class ConnectionChannelHandlerAdapter : ChannelHandlerAdapter
	{
		private readonly ILogger _logger;
		public ConnectionChannelHandlerAdapter(ILogger logger)
		{
			_logger = logger;
		}
		public override void ChannelActive(IChannelHandlerContext ctx)
		{
			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("channel active:" + ctx.Channel.RemoteAddress);

		}

		public override void ChannelInactive(IChannelHandlerContext ctx)
		{

			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("channel active:" + ctx.Channel.RemoteAddress);
			ctx.FireChannelInactive();
		}


		public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
		{
			if (_logger.IsEnabled(LogLevel.Error))
				_logger.LogError("channel exceptionCaught:" + ctx.Channel.RemoteAddress, exception);

		}
	}
}
