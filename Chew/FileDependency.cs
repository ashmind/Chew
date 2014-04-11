using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Chew {
    public class FileDependency {
        public FileDependency([NotNull] string extension, [NotNull] string content, [NotNull] Action<string> providePath) {
            this.Extension = Argument.NotNullOrEmpty("extension", extension);
            this.Content = Argument.NotNullOrEmpty("content", content);
            this.ProvidePath = Argument.NotNull("providePath", providePath);
        }

        [NotNull] public string Extension { get; private set; }
        [NotNull] public string Content { get; private set; }
        [NotNull] public Action<string> ProvidePath { get; private set; }
    }
}
