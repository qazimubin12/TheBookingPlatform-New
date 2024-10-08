using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Services
{
    public class HomeServices
    {
        #region Singleton
        public static HomeServices Instance
        {
            get
            {
                if (instance == null) instance = new HomeServices();
                return instance;
            }
        }
        private static HomeServices instance { get; set; }
        private HomeServices()
        {
        }
        #endregion

        public string Encode(string originalText)
        {
            // Compress the text using GZip
            byte[] compressedData;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    byte[] data = Encoding.UTF8.GetBytes(originalText);
                    gzipStream.Write(data, 0, data.Length);
                }
                compressedData = memoryStream.ToArray();
            }

            // Encode the compressed data in base64 to make it portable
            return Convert.ToBase64String(compressedData);
        }


        public string Decode(string encodedData)
        {
            // Decode the base64-encoded data
            byte[] compressedData = Convert.FromBase64String(encodedData);

            // Decompress the data using GZip
            string originalText;
            using (MemoryStream memoryStream = new MemoryStream(compressedData))
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (StreamReader streamReader = new StreamReader(gzipStream))
            {
                originalText = streamReader.ReadToEnd();
            }

            return originalText;
        }

    }
}
