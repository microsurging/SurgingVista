using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Rtmp.Stream;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Handlers
{
	public class HttpFlvHandler : SimpleChannelInboundHandler<IHttpObject>
	{
		private readonly ConcurrentDictionary<StreamName, MediaStream> _mediaStreamDic = new ConcurrentDictionary<StreamName, MediaStream>();

		public HttpFlvHandler(ConcurrentDictionary<StreamName, MediaStream> mediaStreamDic)
		{
			_mediaStreamDic = mediaStreamDic;
		}


		protected override async void ChannelRead0(IChannelHandlerContext ctx, IHttpObject msg)
		{
			if (msg is IHttpRequest) {
				var req = (IHttpRequest)msg;

				var uri = req.Uri;
				var streamName = uri.Split('/',StringSplitOptions.RemoveEmptyEntries);
				if (streamName.Length != 2) {
					HttpResponseStreamNotExist(ctx, uri);
					return;
				}
				 
				var app = streamName[0];
				var name = streamName[1];
				if (name.EndsWith(".flv")) {
					name = name.Substring(0, name.Length - 4);
				}
				StreamName sn = new StreamName(app, name, false); 
				var stream = _mediaStreamDic.GetValueOrDefault(sn);

				if (stream == null) {
					HttpResponseStreamNotExist(ctx, uri);
					return;
				}
				DefaultHttpResponse response = new DefaultHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK);
				response.Headers.Add(HttpHeaderNames.ContentType, "video/x-flv");
				response.Headers.Add(HttpHeaderNames.TransferEncoding, "chunked");
				response.Headers.Add(HttpHeaderNames.AccessControlAllowOrigin, "*");
				response.Headers.Add(HttpHeaderNames.AccessControlAllowMethods, "GET, POST, PUT,DELETE");
				response.Headers.Add(HttpHeaderNames.AccessControlAllowHeaders, "Origin, X-Requested-With, Content-Type, Accept"); 
				await ctx.WriteAndFlushAsync(response); 
				await stream.AddHttpFlvSubscriber(ctx.Channel); 
			}  
		}

		private void HttpResponseStreamNotExist(IChannelHandlerContext ctx, string uri)
		{
			var body = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("stream [" + uri + "] not exist"));
			DefaultFullHttpResponse response = new DefaultFullHttpResponse(HttpVersion.Http11,
					HttpResponseStatus.NotImplemented, body);
			response.Headers.Add(HttpHeaderNames.ContentType, "text/plain");
			ctx.WriteAndFlushAsync(response);
		}

		public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
		{
			context.CloseAsync();

		}

	}
}
