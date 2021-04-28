using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream.Configurations
{
	public class LiveStreamOption
	{
		public int RtmpPort { get; set; }
		public int HttpFlvPort { get; set; }

		public string RouteTemplate { get; set; } = "Live";

		public int ClusterNode { get; set; } = 2;

		public bool DisablePooled { get; set; } = true;

		public bool IsSaveFlvFile { get; set; }
		public string SaveFlvFilePath { get; set; }
		public bool EnableHttpFlv { get; set; }

		public bool EnableLog { get; set; }
	}
}
