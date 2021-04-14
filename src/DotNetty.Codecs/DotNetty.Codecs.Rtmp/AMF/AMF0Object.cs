using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.AMF
{
	public class AMF0Object : Dictionary<String, Object>
	{
		public AMF0Object AddProperty(String key, Object value)
		{
			Add(key, value);
			return this;
		}
	}
}
