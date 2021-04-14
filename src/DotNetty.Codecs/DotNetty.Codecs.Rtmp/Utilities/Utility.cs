using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Utilities
{
    public class Utility
    { 
        private static Random random = new Random();
	    public static byte[] GenerateRandomData(int size)
        {
            byte[] bytes = new byte[size];
            random.NextBytes(bytes);
            return bytes;
        }

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
