using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comments
{
    internal static class Extensions
    {
        public static string NormalizePath(this string path)
        {
            if (path == null) return null;

            while(path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }
            path = path.TrimEnd('/').ToLower();
            return path;
        }

        public static async Task WriteResponse(this HttpResponse response, string text, string mimeType, int statusCode)
        {
            response.ContentType = mimeType;
            response.StatusCode = statusCode;
            byte[] data = Encoding.UTF8.GetBytes(text);
            await response.Body.WriteAsync(data, 0, data.Length);
        }

        public static string ReadBodyAsString(this HttpRequest request)
        {
            byte[] data = new byte[1024];
            byte[] wholeBody = null;
            using (Stream s = new MemoryStream())
            {
                int read = 0;
                while ((read = request.Body.Read(data, 0, data.Length)) > 0)
                {
                    s.Write(data, 0, read);
                }
                s.Position = 0;
                wholeBody = new byte[s.Length];
                s.Read(wholeBody, 0, wholeBody.Length);
            }
            return Encoding.UTF8.GetString(wholeBody);
        }
    }
}
