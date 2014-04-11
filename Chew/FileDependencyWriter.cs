using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Chew {
    public class FileDependencyWriter {
        [NotNull] private readonly string rootPath;
        [NotNull] private readonly IFileNameGenerator nameGenerator;

        public FileDependencyWriter([NotNull] string rootPath, [NotNull] IFileNameGenerator nameGenerator) {
            this.rootPath = Argument.NotNullOrEmpty("rootPath", rootPath);
            this.nameGenerator = Argument.NotNull("nameGenerator", nameGenerator);
        }

        public void WriteResults([NotNull] ICollection<FileDependency> results) {
            var grouped = Argument.NotNull("results", results).GroupBy(r => r.Content);
            var jsPath = Path.Combine(rootPath, "js");

            if (!Directory.Exists(jsPath))
                Directory.CreateDirectory(jsPath);

            foreach (var group in grouped) {
                var path = Path.Combine(jsPath, nameGenerator.GenerateName(group.Key) + ".js");
                File.WriteAllText(path, group.Key);

                foreach (var entry in group) {
                    entry.ProvidePath(path);
                }
            }
        }
    }
}
