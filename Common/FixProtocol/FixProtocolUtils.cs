using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common.FixProtocol
{
    public static class FixProtocolUtils
    {
        private static readonly Dictionary<int, byte[]> ThreeDigitsNumbers = new Dictionary<int, byte[]>();
        private static readonly Dictionary<int, byte[]> OneDigitsNumbers = new Dictionary<int, byte[]>();


        static FixProtocolUtils()
        {
            for (int i = 0; i < 1000; i++)
            {
                ThreeDigitsNumbers.Add(i, Encoding.ASCII.GetBytes(i.ToString("000")));
                OneDigitsNumbers.Add(i, Encoding.ASCII.GetBytes(i.ToString()));
            }
        }

        public const byte Delimiter = 1;

        private static readonly byte[] FixHeader = Encoding.ASCII.GetBytes("8=FIX.4.2" + '\u0001' + "9=");
        private static readonly byte[] FixFooter = Encoding.ASCII.GetBytes("10=");

        public static IEnumerable<byte> WrapFixUp(this List<byte> data)
        {

            var len = data.Count;

            data.InsertRange(0, FixHeader);

            var lenBytes = OneDigitsNumbers[len];
            data.InsertRange(FixHeader.Length, lenBytes);

            data.Insert(FixHeader.Length + lenBytes.Length, Delimiter);

            var checkSum = ThreeDigitsNumbers[data.CalculateFixChecksum()];

            data.AddRange(FixFooter);
            data.AddRange(checkSum);
            data.Add(Delimiter);

            return data;
        }

        public static int CalculateFixChecksum(this IEnumerable<byte> body)
        {
            var result = 0;
            foreach (var b in body)
                result += b;
            return result % 256;
        }



        private static byte DelimiterEq = (byte) '=';
        private static byte ZeroIndex = (byte) '0';


        private static void EncodeFixPair(List<byte> array, out int key, out string value)
        {
            var eqIndex = array.IndexOf(DelimiterEq);
            key = 0;

            var mult = 1;
            for (int i = eqIndex-1; i >=0; i--)
            {
                key += (array[i]-ZeroIndex)*mult;
                mult *= 10;
            }

            var resultString = new StringBuilder();

            for (int i = eqIndex + 1; i < array.Count; i++)
            {
                resultString.Append((char) array[i]);
            }

            value = resultString.ToString();

        }


        private static int EncodeFixLen(List<byte> data)
        {
            var result = 0;
            var mult = 1;
            for (var i = data.Count - 1; i >= 0; i--)
            {
                if (data[i]==Delimiter)
                    continue;
                if (data[i]==DelimiterEq)
                    break;

                result += (data[i] - ZeroIndex)*mult;
                mult *= 10;
            }

            return result;
        }



    }
}
