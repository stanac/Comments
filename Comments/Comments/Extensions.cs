using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
            byte[] data = new byte[(int)request.Body.Length];
            request.Body.Read(data, 0, data.Length);
            return Encoding.UTF8.GetString(data);
        }
    }
}
