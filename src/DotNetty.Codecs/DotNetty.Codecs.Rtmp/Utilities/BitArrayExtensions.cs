using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Rtmp.Utilities
{
   public  static  class BitArrayExtensions
    {
        public static bool Eq(this byte[] b1 ,byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }
    }
}
