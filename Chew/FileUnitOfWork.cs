using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Chew {
    public class FileUnitOfWork {
        #region FileRequest

        public class FileRequest {
            public FileRequest([NotNull] string extension, [NotNull] string content, [NotNull] Action<string> pathCallback) {
                this.Extension = extension;
                this.Content = content;
                this.PathCallback = pathCallback;
            }

            [NotNull] public string Extension { get; private set; }
            [NotNull] public string Content { get; private set; }
            [NotNull] public Action<string> PathCallback { get; private set; }
        }

        #endregion

        [NotNull] private readonly string rootPath;
        [NotNull] private readonly IFileNameGenerator nameGenerator;

        [NotNull] private readonly IList<FileRequest> fileRequests = new List<FileRequest>();
        [NotNull] private readonly ISet<string> deleteRequests = new HashSet<string>();

        public FileUnitOfWork([NotNull] string rootPath, [NotNull] IFileNameGenerator nameGenerator) {
            this.rootPath = Argument.NotNullOrEmpty("rootPath", rootPath);
            this.nameGenerator = Argument.NotNull("nameGenerator", nameGenerator);
        }

        public void RequestFile([NotNull] string extension, [NotNull] string content, [NotNull] Action<string> pathCallback) {
            this.fileRequests.Add(new FileRequest(
                Argument.NotNullOrEmpty("extension", extension),
                Argument.NotNullOrEmpty("content", content),
                Argument.NotNull("pathCallback", pathCallback)
            ));
        }

        public void RequestDelete([NotNull] string path) {
            this.deleteRequests.Add(Argument.NotNullOrEmpty("path", path));
        }

        public void Commit(Action<string> beforeWrite = null, Action<string> beforeDelete = null) {
            CommitWrites(beforeWrite);
            CommitDeletes(beforeDelete);
        }

        private void CommitWrites(Action<string> beforeWrite) {
            beforeWrite = beforeWrite ?? (s => { });

            var groupsByExtension = this.fileRequests.GroupBy(r => new {r.Extension, r.Content}).GroupBy(x => x.Key.Extension);
            foreach (var groupByExtension in groupsByExtension) {
                var targetPath = Path.Combine(this.rootPath, groupByExtension.Key);
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                foreach (var group in groupByExtension) {
                    var path = Path.Combine(targetPath, this.nameGenerator.GenerateName(@group.Key.Content) + "." + @group.Key.Extension);
                    beforeWrite(path);
                    File.WriteAllText(path, @group.Key.Content);

                    foreach (var entry in @group) {
                        entry.PathCallback(path);
                    }
                }
            }
            fileRequests.Clear();
        }

        private void CommitDeletes(Action<string> beforeDelete) {
            beforeDelete = beforeDelete ?? (s => { });
            foreach (var path in this.deleteRequests) {
                beforeDelete(path);
                File.Delete(path);

                var directoryPath = Path.GetDirectoryName(path);
                while (!Directory.GetFileSystemEntries(directoryPath).Any()) {
                    beforeDelete(directoryPath);
                    Directory.Delete(directoryPath);
                    directoryPath = Path.GetDirectoryName(directoryPath);
                }
            }
            this.deleteRequests.Clear();
        }
    }
}
