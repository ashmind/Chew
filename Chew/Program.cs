using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Chew.Processors;
using HtmlAgilityPack;

namespace Chew {
    public static class Program {
        public static void Main(string[] args) {
            try {
                MainWithoutErrorHandling(args);
                if (Debugger.IsAttached)
                    Console.ReadKey();
            }
            catch (Exception ex) {
                FluentConsole.Red.Line(ex);
            }
        }

        private static void MainWithoutErrorHandling(string[] args) {
            var console = FluentConsole.Instance;

            var workingDirectory = Path.GetFullPath(args[0]);
            var processors = new[] {
                new ReferenceProcessor(new JavaScriptReferenceHandler()),
                new ReferenceProcessor(new CssReferenceHandler())
            };
            var unitOfWork = new FileUnitOfWork(workingDirectory, new MD5FileNameGenerator());

            console.White.Line("Prechewing:");
            var filePaths = Directory.EnumerateFiles(workingDirectory, "*.html", SearchOption.AllDirectories);
            var allDocuments = new List<Tuple<HtmlDocument, string>>();
            foreach (var filePath in filePaths) {
                console.Line(filePath);

                var document = new HtmlDocument();
                document.Load(filePath);

                foreach (var processor in processors) {
                    processor.ProcessDocument(document, filePath, unitOfWork);
                }
                allDocuments.Add(Tuple.Create(document, filePath));
            }

            console.NewLine().White.Line("Chewing:");
            unitOfWork.Commit(
                beforeWrite:  path => console.Green.Line(" + " + path),
                beforeDelete: path => console.Red.Line(" x " + path)
            );

            foreach (var documentAndPath in allDocuments) {
                console.Yellow.Line(" * " + documentAndPath.Item2);
                documentAndPath.Item1.Save(documentAndPath.Item2);
            }
        }
    }
}
