using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using BundleTransformer.Core.Transformers;
using HtmlAgilityPack;

namespace Chew.Processors {
    public class CssReferenceHandler : IReferenceHandler {
        public CssReferenceHandler() {
            Transform = new CssTransformer();
        }

        public IEnumerable<HtmlNode> SelectReferences(HtmlDocument document) {
            return document.DocumentNode
                           .Descendants("link")
                           .Where(l => l.GetAttributeValue("rel", null) == "stylesheet");
        }

        public string GetPath(HtmlNode script) {
            return script.GetAttributeValue("href", null);
        }
        
        public void SetPath(HtmlNode script, string path) {
            script.SetAttributeValue("href", path);
        }

        public IBundleTransform Transform { get; private set; }

        public string FileExtension {
            get { return "css"; }
        }
    }
}
