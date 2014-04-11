using System.Collections.Generic;
using System.Web.Optimization;
using HtmlAgilityPack;
using JetBrains.Annotations;

namespace Chew.Processors {
    public interface IReferenceHandler {
        [NotNull] IEnumerable<HtmlNode> SelectReferences([NotNull] HtmlDocument document);
        [NotNull] string GetPath([NotNull] HtmlNode reference);
        void SetPath([NotNull] HtmlNode reference, [NotNull] string path);

        IBundleTransform Transform { get; }
        string FileExtension { get; }
    }
}