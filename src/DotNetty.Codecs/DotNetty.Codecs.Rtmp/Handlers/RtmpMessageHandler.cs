using DotNetty.Codecs.Rtmp.AMF;
using DotNetty.Codecs.Rtmp.Messages;
using DotNetty.Codecs.Rtmp.Stream;
using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Codecs.Rtmp.Handlers
{
   public class RtmpMessageHandler: SimpleChannelInboundHandler<AbstractRtmpMessage>
    {
		static readonly IInternalLogger logger = InternalLoggerFactory.GetInstance<RtmpMessageHandler>();
		private int _ackWindowSize;
		private int _lastSentbackSize;
		private int _bytesReceived; 
		private RtmpTag _tag;
		private bool _normalShutdown; 
		private StreamName _streamName;
		public readonly Action<IChannelHandlerContext, StreamName, AbstractRtmpMessage> _readAction;

		private readonly ConcurrentDictionary<StreamName, MediaStream> _mediaStreamDic = new ConcurrentDictionary<StreamName, MediaStream>();

		public RtmpMessageHandler(ConcurrentDictionary<StreamName, MediaStream> mediaStreamDic, Action<IChannelHandlerContext, StreamName, AbstractRtmpMessage> readAction, RtmpConfig rtmpConfig) :this(mediaStreamDic)
		{
			_readAction = readAction;
			RtmpConfig.Instance = rtmpConfig;
		}

		public RtmpMessageHandler(ConcurrentDictionary<StreamName, MediaStream> mediaStreamDic)
		{
			_mediaStreamDic = mediaStreamDic;
		}

		public override async void ChannelInactive(IChannelHandlerContext ctx)
		{
			if (!_normalShutdown && _streamName!=null && _tag == RtmpTag.Publisher)
			{
				var stream = _mediaStreamDic.GetValueOrDefault(_streamName);
				if (stream != null)
				{
					await stream.SendEofToAllSubscriberAndClose();
				}
				else
				{
					logger.Error($"stream:{nameof(_streamName)} is null");
				}
			}
		}

		protected override void ChannelRead0(IChannelHandlerContext ctx, AbstractRtmpMessage msg)
		{
			MaySendAck(ctx, msg);
			if (msg is WindowAcknowledgementSize)
			{
				int ackSize = ((WindowAcknowledgementSize)msg).WindowSize;
				_ackWindowSize = ackSize;
				return;
			}

			if (msg is RtmpCommandMessage)
			{
				HandleCommand(ctx, (RtmpCommandMessage)msg);
				
			}
			else if (msg is RtmpDataMessage)
			{
				HandleDataMessage(ctx, (RtmpDataMessage)msg);
			}
			else if (msg is AbstractRtmpMediaMessage)
			{ 
				HandleMedia(ctx, (AbstractRtmpMediaMessage)msg);
				 _readAction?.Invoke(ctx, _streamName, msg);
			}
			else if (msg is UserControlMessageEvent)
			{
				HandleUserControl(ctx, (UserControlMessageEvent)msg);
			}
			ReferenceCountUtil.Release(msg);
		}

 

		private void HandleMedia(IChannelHandlerContext ctx, AbstractRtmpMediaMessage msg)
		{
			var stream = _mediaStreamDic.GetValueOrDefault(_streamName);
			if (stream == null)
			{
				logger.Error($"stream:{nameof(_streamName)} not exist!");
				return;
			}
			stream.AddContent(msg);
		}

		private void HandleUserControl(IChannelHandlerContext ctx, UserControlMessageEvent msg)
		{
		 
		}

		private void HandleDataMessage(IChannelHandlerContext ctx, RtmpDataMessage msg)
		{

			var  name = msg.Data[0].ToString();
			if (name=="@setDataFrame")
			{
				var properties = (Dictionary<String, Object>)msg.Data[2];
				properties.Remove("filesize");

				String encoder = (String)properties.GetValueOrDefault("encoder");
				if (encoder != null && encoder.Contains("obs"))
				{
					_streamName.IsObsClient=true;
				}
				var stream = _mediaStreamDic.GetValueOrDefault(_streamName);
				stream.Metadata=properties;
			}
		}

		private void HandleCommand(IChannelHandlerContext ctx, RtmpCommandMessage msg)
		{
			var command = msg.Command;
			var commandName = (String)command[0].ToString();
			switch (commandName)
			{
				case "connect":
					HandleConnect(ctx, msg);
					break;
				case "createStream":
					HandleCreateStream(ctx, msg);
					break;
				case "publish":
					HandlePublish(ctx, msg);
					_readAction?.Invoke(ctx, _streamName, msg);
					break;
				case "play":
					HandlePlay(ctx, msg);
					break;
				case "deleteStream":
				case "closeStream":
					HandleCloseStream(ctx, msg);
					_readAction?.Invoke(ctx, _streamName, msg);
					break;
				default:
					break;
			}

		}

		private async void HandleCloseStream(IChannelHandlerContext ctx, RtmpCommandMessage msg)
		{
			MediaStream stream = _mediaStreamDic.GetValueOrDefault(_streamName);
			try
			{
				if (_tag == RtmpTag.Subscriber)
				{
					logger.Info($"subscriber:{ctx.Channel.RemoteAddress} close");
					return;
				}
				var onStatus = OnStatus("status", "NetStream.Unpublish.Success", "Stop publishing");
				await ctx.WriteAsync(onStatus); 
				if (stream == null)
				{
					logger.Error($"can't find stream:{nameof(_streamName)} in buffer queue");
				}
				else
				{
					if (stream != null)
					{
						await stream.SendEofToAllSubscriberAndClose();
						_mediaStreamDic.Remove(_streamName, out MediaStream mediaStream);
					}
					_normalShutdown = true;
					await ctx.CloseAsync();

				}
			}
			catch
			{
				if (stream != null)
				{
					await stream.SendEofToAllSubscriberAndClose();
					_mediaStreamDic.Remove(_streamName, out MediaStream mediaStream);
				}
				await ctx.CloseAsync();
			}
		}

		private async void HandlePlay(IChannelHandlerContext ctx, RtmpCommandMessage msg)
		{
			_tag = RtmpTag.Subscriber; 
			var name = (String)msg.Command[3];
			_streamName.Name=name; 
			var  stream = _mediaStreamDic.GetValueOrDefault(_streamName);
			if (stream == null)
			{
				logger.Info($"client play request for stream:{nameof(_streamName)} not exist.");
				RtmpCommandMessage onStatus = OnStatus("error", "NetStream.Play.StreamNotFound", "No Such Stream"); 
				await ctx.WriteAndFlushAsync(onStatus); 
				_normalShutdown = true;
				await ctx.Channel.CloseAsync();
			}
			else
			{
				StartPlay(ctx, stream);
			}
		}

		private async void StartPlay(IChannelHandlerContext ctx, MediaStream stream)
		{
			try
			{ 
				await ctx.WriteAndFlushAsync(UserControlMessageEvent.StreamBegin(Constants.DEFAULT_STREAM_ID)); 
				RtmpCommandMessage onStatus = OnStatus("status", "NetStream.Play.Start", "Start live"); 
				await ctx.WriteAndFlushAsync(onStatus); 
				var args = new List<Object>();
				args.Add("|RtmpSampleAccess");
				args.Add(true);
				args.Add(true);
				RtmpCommandMessage rtmpSampleAccess = new RtmpCommandMessage(args); 
				await ctx.WriteAndFlushAsync(rtmpSampleAccess); 
				var metadata = new List<Object>();
				metadata.Add("onMetaData");
				metadata.Add(stream.Metadata);
				RtmpDataMessage msgMetadata = new RtmpDataMessage(metadata);
				await ctx.WriteAndFlushAsync(msgMetadata);
				await stream.AddSubscriber(ctx.Channel);

			}
			catch(Exception ex)
			{
				var i = 0;
			}
		}

		private async void HandlePublish(IChannelHandlerContext ctx, RtmpCommandMessage msg)
		{
			logger.Info($"publish :{msg}");
			_tag = RtmpTag.Publisher; 
			var streamType = msg.Command[4].ToString();
			if (streamType != "live")
			{
				await ctx.Channel.DisconnectAsync();
			}
			var name =  msg.Command[3].ToString();
			_streamName.Name=name;
			CreateStream(ctx); 
			RtmpCommandMessage onStatus = OnStatus("status", "NetStream.Publish.Start", "Start publishing"); 
			await ctx.WriteAndFlushAsync(onStatus);

		}

		private void CreateStream(IChannelHandlerContext ctx)
		{
			var s = new MediaStream(_streamName);
			s.Publisher=ctx.Channel;
			_mediaStreamDic.TryAdd(_streamName, s);
		}

		private async void HandleCreateStream(IChannelHandlerContext ctx, RtmpCommandMessage msg)
		{
			logger.Info($"create stream received : {msg}");
			var  result = new List<Object>();
			result.Add("_result");
			result.Add(msg.Command[1]);
			result.Add(null);
			result.Add(Constants.DEFAULT_STREAM_ID);
			RtmpCommandMessage response = new RtmpCommandMessage(result);
			await ctx.WriteAndFlushAsync(response);

		}

		private async void HandleConnect(IChannelHandlerContext ctx, RtmpCommandMessage msg)
		{
			logger.Info($"client connect {msg} ");
			var command = ((Dictionary<string, object>)msg.Command[2]);
			var app = command.GetValueOrDefault("app").ToString();
			var clientRequestEncode =  command.GetValueOrDefault("objectEncoding") ;
			if (!string.Equals(app, RtmpConfig.Instance.App, StringComparison.OrdinalIgnoreCase))
			{
				await ctx.CloseAsync();
				return;
			}
			if (clientRequestEncode != null && clientRequestEncode.ToString() == "3")
			{
				logger.Error($"client :{ctx} request AMF3 encoding  server  doesn't support");
				await ctx.CloseAsync();
				return;
			}

			_streamName = new StreamName(app, null, false);

			int ackSize = 5000000;
			WindowAcknowledgementSize was = new WindowAcknowledgementSize(ackSize);

			var spb = new  SetPeerBandWidth(ackSize, Constants.SET_PEER_BANDWIDTH_TYPE_SOFT);

			SetChunkSize setChunkSize = new SetChunkSize(5000);

			await ctx.WriteAndFlushAsync(was);
			await ctx.WriteAndFlushAsync(spb);
			await ctx.WriteAndFlushAsync(setChunkSize);

			var  result = new List<Object>();
			result.Add("_result");
			result.Add(msg.Command[1]);// transaction id
			result.Add(new AMF0Object().AddProperty("fmsVer", "FMS/3,0,1,123").AddProperty("capabilities", 31));
			result.Add(new AMF0Object().AddProperty("level", "status").AddProperty("code", "NetConnection.Connect.Success")
					.AddProperty("description", "Connection succeeded").AddProperty("objectEncoding", 0));

			RtmpCommandMessage response = new RtmpCommandMessage(result);

			await ctx.WriteAndFlushAsync(response);

		}

		private async void MaySendAck(IChannelHandlerContext ctx, AbstractRtmpMessage msg)
		{ 
			int receiveBytes = msg.InboundBodyLength + msg.InboundHeaderLength;
			_bytesReceived += receiveBytes;

			if (_ackWindowSize <= 0)
			{
				return;
			} 
			if (_bytesReceived > 0X70000000)
			{
				logger.Warn("reset bytesReceived");
				await ctx.WriteAndFlushAsync(new Acknowledgement(_bytesReceived));
				_bytesReceived = 0;
				_lastSentbackSize = 0;
				return;
			}

			if (_bytesReceived - _lastSentbackSize >= _ackWindowSize)
			{ 
				_lastSentbackSize = _bytesReceived;
				await ctx.WriteAndFlushAsync(new Acknowledgement(_lastSentbackSize));
			}
		}

		public RtmpCommandMessage OnStatus(String level, String code, String description)
		{
			var  result = new List<Object>();
			result.Add("onStatus");
			result.Add(0);
			result.Add(null);
			result.Add(new AMF0Object().AddProperty("level", level).AddProperty("code", code).AddProperty("description",
					description));

			RtmpCommandMessage response = new RtmpCommandMessage(result);
			return response;
		}
	}
}
