using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Stream
{
	[MessagePackObject]
	public class StreamName
	{
		[Key(0)]
		public String App { get; set; }
		[Key(1)]
		public String Name { get; set; }

		[Key(2)]
		public bool IsObsClient { get; set; }


		public StreamName(string app,string name,bool isObsClient)
		{
			App = app;
			Name = name;
			IsObsClient = isObsClient;
		}

		public override bool Equals(Object obj)
		{
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (this.GetType() != obj.GetType())
				return false;
			StreamName other = (StreamName)obj;
			if (App == null)
			{
				if (other.App != null)
					return false;
			}
			else if (!App.Equals(other.App))
				return false;
			if (Name == null)
			{
				if (other.Name != null)
					return false;
			}
			else if (!Name.Equals(other.Name))
				return false;
			return true;
		}


		public override int GetHashCode()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + ((App == null) ? 0 : App.GetHashCode());
			result = prime * result + ((Name == null) ? 0 : Name.GetHashCode());
			return result;
		}
	}
}
