using System;
using System.IO;
using System.Web.Hosting;
using JetBrains.Annotations;

namespace Chew.VirtualPathSupport {
    public class PhysicalFile : VirtualFile {
        private readonly string path;

        public PhysicalFile([NotNull] string path) : base(path) {
            this.path = Argument.NotNullOrEmpty("path", path);
        }

        public override Stream Open() {
            return File.OpenRead(this.path);
        }
    }
}