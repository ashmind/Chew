using System;
using System.Collections.Generic;
using System.Linq;

namespace Chew {
    public class FileResult {
        public FileResult(Action<string> providePath, string content) {
            this.ProvidePath = providePath;
            this.Content = content;
        }

        public Action<string> ProvidePath { get; private set; }
        public string Content { get; private set; }
    }
}
