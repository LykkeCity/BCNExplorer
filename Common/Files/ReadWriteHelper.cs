using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Files
{
    public static class ReadWriteHelper
    {

        public static async Task<byte[]> ReadAllFileAsync(string filename, int bufferSize = 4096)
        {
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                var buff = new byte[file.Length];
                await file.ReadAsync(buff, 0, (int)file.Length);

                return file.ToBytes();
            }
        }

        public static async Task WriteAsync(string filePath, byte[] data, int bufferSize = 4096)
        {

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: bufferSize, useAsync: true))
            {
                await sourceStream.WriteAsync(data, 0, data.Length);
            };
        }

    }
}
