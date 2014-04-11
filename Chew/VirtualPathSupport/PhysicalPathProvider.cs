using System;
using System.IO;
using System.Web.Hosting;

namespace Chew.VirtualPathSupport {
    public class PhysicalPathProvider : VirtualPathProvider {
        public override bool FileExists(string virtualPath) {
            if (virtualPath.StartsWith("~"))
                return true; // I hope callers know what they are doing :)

            return File.Exists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath) {
            if (virtualPath.StartsWith("~"))
                base.GetFile(virtualPath);

            return new PhysicalFile(virtualPath);
        }
    }
}