using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Misc
{
    public static class MoreHash
    {
        private const int charLength = sizeof(char);
        public static T HashTo<T>(this string str) where T : unmanaged
        {
            unsafe
            {
                int tLength = sizeof(T);
                T t = default;
                Span<byte> outP = new Span<byte>(&t, tLength);
                for (int i = 0; i < outP.Length; i++)
                {
                    outP[i] = 0;
                }

                fixed (char* c = str)
                {
                    Span<byte> source = new Span<byte>(c, str.Length * charLength);
                    for (int i = 0; i < source.Length; i++)
                    {
                        outP[i % outP.Length] ^= source[i];
                    }
                }

                return t;
            }
        }
        public static T HashTo<T>(this StringBuilder str) where T : unmanaged => str.ToString().HashTo<T>();
    }
}
