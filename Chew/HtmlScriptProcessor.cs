using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Optimization;
using HtmlAgilityPack;
using JetBrains.Annotations;

namespace Chew {
    public class HtmlScriptProcessor {
        #region BundledSequence class

        private class BundledSequence {
            public BundledSequence([NotNull] IList<HtmlNode> nodes, [NotNull] string content) {
                this.Nodes = Argument.NotNullOrEmpty("nodes", nodes);
                this.Content = Argument.NotNull("content", content);
            }

            [NotNull] public IList<HtmlNode> Nodes { get; private set; }
            [NotNull] public string Content { get; private set; }
        }

        #endregion

        public IEnumerable<FileResult> ProcessDocument(HtmlDocument document, string documentPath) {
            Argument.NotNull("document", document);
            Argument.NotNullOrEmpty("documentPath", documentPath);

            var scripts = document.DocumentNode.Descendants("script");
            
            var bundles = new List<BundledSequence>();
            var sequence = new List<HtmlNode>();

            foreach (var script in scripts) {
                if (ShouldProcess(script)) {
                    sequence.Add(script);
                }
                else if (sequence.Count > 0) {
                    bundles.Add(BundleSequence(sequence, documentPath));
                    sequence = new List<HtmlNode>();
                }
            }

            if (sequence.Count > 0)
                bundles.Add(BundleSequence(sequence, documentPath));

            var results = new List<FileResult>();
            foreach (var bundle in bundles) {
                var bundleNode = ReplaceNodesInDocumentWithBundleNode(bundle);
                var result = new FileResult(
                    path => bundleNode.SetAttributeValue("src", GetRelativePath(documentPath, path)),
                    bundle.Content
                );
                results.Add(result);
            }

            return results;
        }

        private string GetRelativePath(string documentPath, string path) {
            return new Uri(documentPath).MakeRelativeUri(new Uri(path)).ToString();
        }

        private static HtmlNode ReplaceNodesInDocumentWithBundleNode(BundledSequence bundle) {
            foreach (var node in bundle.Nodes.Take(bundle.Nodes.Count - 1)) {
                while (node.NextSibling != null && node.NextSibling.NodeType == HtmlNodeType.Text) {
                    node.NextSibling.Remove();
                }
                node.Remove();
            }
            return bundle.Nodes[bundle.Nodes.Count - 1];
        }

        private BundledSequence BundleSequence(IList<HtmlNode> sequence, string documentPath) {
            var settings = new OptimizationSettings {
                BundleTable = new BundleCollection(),
                ApplicationPath = Path.GetDirectoryName(documentPath)
            };

            var bundle = new Bundle("~/stub")
                .Include(sequence.Select(s => "~/" + s.GetAttributeValue("src", null)).ToArray());
            bundle.Transforms.Add(new JsMinify());
            settings.BundleTable.Add(bundle);
    
            var response = Optimizer.BuildBundle("~/stub", settings);

            return new BundledSequence(sequence, response.Content);
        }

        private bool ShouldProcess(HtmlNode script) {
            if (!HasCorrectType(script))
                return false;

            var src = script.GetAttributeValue("src", null);
            if (src == null)
                return false;

            if (src.StartsWith("//") || new Uri(src, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                return false;

            return true;
        }

        private static bool HasCorrectType(HtmlNode script) {
            var type = script.GetAttributeValue("type", null);
            return type == null 
                || type.Equals("text/javascript", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
