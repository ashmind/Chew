using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Chew {
    public class MD5FileNameGenerator : IFileNameGenerator {
        public string GenerateName(string content) {
            Argument.NotNullOrEmpty("content", content);

            using (var md5 = MD5.Create()) {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
