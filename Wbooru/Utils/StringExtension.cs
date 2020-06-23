using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Wbooru
{
    public static class StringExtension
    {
        private static MD5 md5 = MD5.Create();
        public static string CalculateMD5(this string content) => string.Join("", md5.ComputeHash(Encoding.UTF8.GetBytes(content)).Select(x => x.ToString("x2")));
    }
}
