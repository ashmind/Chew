using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Optimization;
using Chew.VirtualPathSupport;
using HtmlAgilityPack;
using JetBrains.Annotations;

namespace Chew.Processors {
    public class ReferenceProcessor {
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

        private readonly IReferenceHandler handler;

        static ReferenceProcessor() {
            BundleTable.VirtualPathProvider = new PhysicalPathProvider();
        }

        public ReferenceProcessor([NotNull] IReferenceHandler handler) {
            this.handler = Argument.NotNull("handler", handler);
        }

        public void ProcessDocument(HtmlDocument document, string documentPath, FileUnitOfWork unitOfWork) {
            Argument.NotNull("document", document);
            Argument.NotNullOrEmpty("documentPath", documentPath);

            var references = this.handler.SelectReferences(document);
            var bundles = new List<BundledSequence>();
            var sequence = new List<HtmlNode>();

            foreach (var reference in references) {
                var shouldProcess = IsLocalPath(this.handler.GetPath(reference))
                                 && !reference.Attributes.Contains("data-bundled");
                if (shouldProcess) {
                    sequence.Add(reference);
                }
                else if (sequence.Count > 0) {
                    bundles.Add(BundleSequence(sequence, documentPath, unitOfWork));
                    sequence = new List<HtmlNode>();
                }
            }

            if (sequence.Count > 0)
                bundles.Add(BundleSequence(sequence, documentPath, unitOfWork));

            foreach (var bundle in bundles) {
                var bundleNode = ReplaceNodesInDocumentWithBundleNode(bundle);
                unitOfWork.RequestFile(
                    this.handler.FileExtension,
                    bundle.Content,
                    path => this.handler.SetPath(bundleNode, GetRelativePath(documentPath, path))
                );
            }
        }

        private bool IsLocalPath(string path) {
            if (path == null)
                return false;

            if (path.StartsWith("//") || new Uri(path, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                return false;

            return true;
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
            var bundleNode = bundle.Nodes[bundle.Nodes.Count - 1];
            bundleNode.SetAttributeValue("data-bundled", "data-bundled");
            return bundleNode;
        }

        private BundledSequence BundleSequence(IList<HtmlNode> sequence, string documentPath, FileUnitOfWork unitOfWork) {
            var parentPath = Path.GetDirectoryName(documentPath);
            var settings = new OptimizationSettings {
                BundleTable = new BundleCollection(),
                ApplicationPath = parentPath
            };

            var paths = sequence.Select(r => this.handler.GetPath(r)).ToArray();
            var bundle = new Bundle("~/stub")
                .Include(paths.Select(p => "~/" + p).ToArray());

            bundle.Transforms.Add(this.handler.Transform);
            settings.BundleTable.Add(bundle);

            var response = Optimizer.BuildBundle(bundle.Path, settings);

            foreach (var path in paths) {
                unitOfWork.RequestDelete(Path.Combine(parentPath, path));
            }

            return new BundledSequence(sequence, response.Content);
        }
    }
}
