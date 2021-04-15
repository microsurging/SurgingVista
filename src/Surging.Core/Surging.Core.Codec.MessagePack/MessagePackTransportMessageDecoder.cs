using Surging.Core.Codec.MessagePack.Messages;
using Surging.Core.Codec.MessagePack.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Surging.Core.Codec.MessagePack
{
    public sealed class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            try
            {
                var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);
                return message.GetTransportMessage();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

       

        #endregion Implementation of ITransportMessageDecoder
    }
}
