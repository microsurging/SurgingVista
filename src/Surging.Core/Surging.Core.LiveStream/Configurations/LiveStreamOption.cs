using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.LiveStream.Configurations
{
	public class LiveStreamOption
	{
		public int RtmpPort { get; set; }
		public int HttpFlvPort { get; set; }
		public bool SaveFlvFile { get; set; }
		public string SaveFlVFilePath { get; set; }
		public bool EnableHttpFlv { get; set; }

		public bool EnableLog { get; set; }
	}
}
