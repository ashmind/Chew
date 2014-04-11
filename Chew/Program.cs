using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chew.Processors;
using HtmlAgilityPack;

namespace Chew {
    public static class Program {
        public static void Main(string[] args) {
            try {
                MainWithoutErrorHandling(args);
            }
            catch (Exception ex) {
                FluentConsole.Red.Line(ex);
            }
        }

        private static void MainWithoutErrorHandling(string[] args) {
            var workingDirectory = args[0];
            var processors = new[] {
                new ReferenceProcessor(new JavaScriptReferenceHandler()),
                new ReferenceProcessor(new CssReferenceHandler())
            };
            var writer = new FileDependencyWriter(workingDirectory, new MD5FileNameGenerator());

            var filePaths = Directory.EnumerateFiles(workingDirectory, "*.html", SearchOption.AllDirectories);
            var allResults = new List<FileDependency>();
            var allDocuments = new List<Tuple<HtmlDocument, string>>();
            foreach (var filePath in filePaths) {
                FluentConsole.White.Line(filePath);

                var document = new HtmlDocument();
                document.Load(filePath);

                var results = processors.SelectMany(p => p.ProcessDocument(document, filePath));
                allResults.AddRange(results);
                allDocuments.Add(Tuple.Create(document, filePath));
            }

            writer.WriteResults(allResults);

            foreach (var documentAndPath in allDocuments) {
                documentAndPath.Item1.Save(documentAndPath.Item2 + ".processed");
            }
        }
    }
}
