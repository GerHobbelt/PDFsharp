using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace PdfSharp.Test.Integration
{
    public class PdfManipulationTest
    {
        public PdfManipulationTest()
        {
            Helpers.ResetTempFolder();
        }

        [Theory]
        [InlineData("3 pages.pdf", "image.pdf")]
        public void TestCombineDocuments(string fileName1, string fileName2)
        {
            PdfDocument doc1 = PdfReader.Open(Path.Combine("data", fileName1), PdfDocumentOpenMode.Modify),
                doc2 = PdfReader.Open(Path.Combine("data", fileName2), PdfDocumentOpenMode.Import);
            var expectedPageCount = doc1.PageCount + doc2.PageCount;

            foreach (var page in doc2.Pages)
                doc1.AddPage(page);
            var outputPath = Path.Combine("temp", "test.pdf");
            doc1.Save(outputPath);

            var combined = PdfReader.Open(outputPath, PdfDocumentOpenMode.InformationOnly);
            combined.PageCount.Should().Be(expectedPageCount);
        }
    }
}
