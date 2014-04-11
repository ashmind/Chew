using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Chew {
    public class FileDependency {
        public FileDependency([NotNull] string content, [NotNull] Action<string> providePath) {
            this.Content = Argument.NotNullOrEmpty("content", content);
            this.ProvidePath = Argument.NotNull("providePath", providePath);
        }

        [NotNull] public string Content { get; private set; }
        [NotNull] public Action<string> ProvidePath { get; private set; }
    }
}
