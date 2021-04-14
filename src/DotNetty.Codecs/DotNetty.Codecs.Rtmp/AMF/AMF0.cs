using DotNetty.Buffers;
using DotNetty.Codecs.Rtmp.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.AMF
{
	public class AMF0
	{
		private static readonly byte BOOLEAN_TRUE = 0x01;
		private static readonly byte BOOLEAN_FALSE = 0x00;
		private static readonly byte[] OBJECT_END_MARKER = new byte[] { 0x00, 0x00, 0x09 };

		public enum AMF0_Type
		{
			NUMBER = 0x00,
			BOOLEAN = 0x01,
			STRING = 0x02,
			OBJECT = 0x03,
			NULL = 0x05,
			UNDEFINED = 0x06,
			Dic = 0x08,
			ARRAY = 0x0A,
			DATETIME = 0x0B,
			LONG_STRING = 0x0C,
			UNSUPPORTED = 0x0D
		}
		private static  AMF0_Type? GetType(Object value)
		{
			if (value == null)
			{
				return AMF0_Type.NULL;
			}
			else if (value is String)
			{
				return AMF0_Type.STRING;
			}
			else if (value is int)
			{
				return AMF0_Type.NUMBER;
			}
			else if(value is double)
			{
				return AMF0_Type.NUMBER;
			}
			else if (value is decimal)
			{
				return AMF0_Type.NUMBER;
			}
			else if (value is Boolean)
			{
				return AMF0_Type.BOOLEAN;
			}
			else if (value is AMF0Object)
			{
				return AMF0_Type.OBJECT;
			}
			else if (value is Dictionary<string, Object>)
			{
				return AMF0_Type.Dic;
			}
			else if (value is Object[])
			{
				return AMF0_Type.ARRAY;
			}
			else if (value is DateTime)
			{
				return AMF0_Type.DATETIME;
			}
			else
			{
				throw new ArgumentException("unexpected type: " + value.GetType());
			}
		}

		public static AMF0_Type? valueToEnum(int value)
		{
			switch (value)
			{
				case 0x00:
					return AMF0_Type.NUMBER;
				case 0x01:
					return AMF0_Type.BOOLEAN;
				case 0x02:
					return AMF0_Type.STRING;
				case 0x03:
					return AMF0_Type.OBJECT;
				case 0x05:
					return null;
				case 0x06:
					return AMF0_Type.UNDEFINED;
				case 0x08:
					return AMF0_Type.Dic;
				case 0x0A:
					return AMF0_Type.ARRAY;
				case 0x0B:
					return AMF0_Type.DATETIME;
				case 0x0C:
					return AMF0_Type.LONG_STRING;
				case 0x0D:
					return AMF0_Type.UNSUPPORTED;

				default:
					throw new ArgumentException("unexpected type: " + value);

			}
		}

		public static void Encode(IByteBuffer byteBuffer,  Object value)
		{
			var type = GetType(value); 
			byteBuffer.WriteByte((byte)type); 
			switch (type)
			{
				case AMF0_Type.NUMBER:
					{
						if (value is Double)
						{
							byteBuffer.WriteLong(BitConverter.DoubleToInt64Bits((Double)value));
						}
						else
						{ // this coverts int also
							byteBuffer.WriteLong(BitConverter.DoubleToInt64Bits(Double.Parse(value.ToString())));
						}
						return;
					}
				case AMF0_Type.BOOLEAN:
					{
						byteBuffer.WriteByte((Boolean)value ? BOOLEAN_TRUE : BOOLEAN_FALSE);
						return;
					}
				case AMF0_Type.STRING:
					EncodeString(byteBuffer, (String)value);
					return;
				case AMF0_Type.NULL:
					return;
				case AMF0_Type.Dic:
					byteBuffer.WriteInt(0);
					goto case AMF0_Type.OBJECT;
				// no break; remaining processing same as OBJECT
				case AMF0_Type.OBJECT:
					{
						var dic = (Dictionary<String, Object>)value;
						foreach (var entry in dic)
						{
							EncodeString(byteBuffer, entry.Key);
							Encode(byteBuffer, entry.Value);
						}
						byteBuffer.WriteBytes(OBJECT_END_MARKER);
						return;
					}
				case AMF0_Type.ARRAY:
					{
						var array = (Object[])value;
						byteBuffer.WriteInt(array.Length);
						foreach (Object o in array)
						{
							Encode(byteBuffer, o);
						}
						return;
					}
				case AMF0_Type.DATETIME:
					{
						long time = Utility.CurrentTimeMillis();
						byteBuffer.WriteLong(BitConverter.DoubleToInt64Bits(time));
						byteBuffer.WriteShort((short)0);
						return;
					}
				default:
					throw new ArgumentException("unexpected type: " + type);
			}
		}

		private static string DecodeString(IByteBuffer byteBuffer)
		{
			short size = byteBuffer.ReadShort();
			byte[] bytes = new byte[size];
			byteBuffer.ReadBytes(bytes);
			return Encoding.UTF8.GetString(bytes); // should we force encode to UTF-8 ?
		}

		private static void EncodeString( IByteBuffer byteBuffer,  string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			byteBuffer.WriteShort((short)bytes.Length);
			byteBuffer.WriteBytes(bytes);
		}

		public static void Encode(IByteBuffer byteBuffer,  List<Object> values)
		{
			foreach (  Object value in values)
			{
				Encode(byteBuffer, value);
			}
		}

		public static Object Decode(IByteBuffer byteBuffer)
		{
			 Enum.TryParse<AMF0_Type>( byteBuffer.ReadByte().ToString(),out AMF0_Type type );
			  Object value = Decode(byteBuffer, type);
			 

			return value;
		}

		public static List<Object> DecodeAll(IByteBuffer byteBuffer)
		{
			var result = new List<Object>();
			while (byteBuffer.IsReadable()) {
				Object decode = Decode(byteBuffer);
				result.Add(decode);
			}
			return result;

		}

		private static Object Decode(IByteBuffer byteBuffer, AMF0_Type type)
		{
			switch (type)
			{
				case AMF0_Type.NUMBER:
					return BitConverter.Int64BitsToDouble(byteBuffer.ReadLong());
				case AMF0_Type.BOOLEAN:
					return byteBuffer.ReadByte() == BOOLEAN_TRUE;
				case AMF0_Type.STRING:
					return DecodeString(byteBuffer);
				case AMF0_Type.ARRAY:
					{
						int arraySize = byteBuffer.ReadInt();
						Object[] array = new Object[arraySize];
						for (int j = 0; j < arraySize; j++)
						{
							array[j] = Decode(byteBuffer);
						}
						return array;
					}
				case AMF0_Type.Dic:
				case AMF0_Type.OBJECT:
					  int count;
					Dictionary<String, Object> dic;
					if (type == AMF0_Type.Dic)
					{
						count = byteBuffer.ReadInt(); // should always be 0
						dic = new Dictionary<String, Object>();
					 
					}
					else
					{
						count = 0;
						dic = new AMF0Object();
					}
					int i = 0;
					  byte[] endMarker = new byte[3];
					while (byteBuffer.IsReadable()) {
						byteBuffer.GetBytes(byteBuffer.ReaderIndex, endMarker);
						if (endMarker.Eq(OBJECT_END_MARKER))
						{
							byteBuffer.SkipBytes(3);
							break;
						}
						if (count > 0 && i++ == count)
						{
							break;
						}
						dic.Add(DecodeString(byteBuffer), Decode(byteBuffer));
					}
					return dic;
				case AMF0_Type.DATETIME:
					  long dateValue = byteBuffer.ReadLong();
					byteBuffer.ReadShort(); 
					return new DateTime((long)BitConverter.Int64BitsToDouble(dateValue));
				case AMF0_Type.LONG_STRING:
					  int stringSize = byteBuffer.ReadInt();
					  byte[] bytes = new byte[stringSize];
					byteBuffer.ReadBytes(bytes);
					return  Encoding.UTF8.GetString(bytes); 
				case AMF0_Type.NULL:
				case AMF0_Type.UNDEFINED:
				case AMF0_Type.UNSUPPORTED:
					return null;
				default:
					throw new ArgumentException("unexpected type: " + type);
			}
		}
	}
}
