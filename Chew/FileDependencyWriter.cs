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
            var groupedByType = Argument.NotNull("results", results).GroupBy(r => new { r.Extension, r.Content }).GroupBy(x => x.Key.Extension);
            foreach (var groupByType in groupedByType) {
                var targetPath = Path.Combine(rootPath, groupByType.Key);
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                foreach (var group in groupByType) {
                    var path = Path.Combine(targetPath, nameGenerator.GenerateName(group.Key.Content) + "." + group.Key.Extension);
                    File.WriteAllText(path, group.Key.Content);

                    foreach (var entry in group) {
                        entry.ProvidePath(path);
                    }
                }
            }
        }
    }
}
