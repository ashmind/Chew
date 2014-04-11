using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using BundleTransformer.Core.Transformers;
using HtmlAgilityPack;

namespace Chew.Processors {
    public class JavaScriptReferenceHandler : IReferenceHandler {
        public JavaScriptReferenceHandler() {
            Transform = new JsTransformer();
        }

        public IEnumerable<HtmlNode> SelectReferences(HtmlDocument document) {
            return document.DocumentNode.Descendants("script").Where(HasCorrectType);
        }

        private bool HasCorrectType(HtmlNode script) {
            var type = script.GetAttributeValue("type", null);
            return type == null
                || type.Equals("text/javascript", StringComparison.InvariantCultureIgnoreCase);
        }

        public string GetPath(HtmlNode script) {
            return script.GetAttributeValue("src", null);
        }

        public void SetPath(HtmlNode script, string path) {
            script.SetAttributeValue("src", path);
        }

        public IBundleTransform Transform { get; private set; }

        public string FileExtension {
            get { return "js"; }
        }
    }
}
