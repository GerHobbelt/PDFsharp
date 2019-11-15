using FluentAssertions;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using Xunit;

namespace PdfSharp.Test.Integration
{
    public class PdfReadingTest
    {
        [Theory]
        [InlineData("RC4-128 open password.pdf", "pass", 3)] //Created by Adobe Acrobat - Adobe 6.0 compatibility
        [InlineData("AES-256 open password.pdf", "pass", 3)] //Created by Adobe Acrobat - Adobe 7.0 compatibility
        [InlineData("AES-128 open password.pdf", "pass", 3)] //Created by Adobe Acrobat - Adobe X compatibility
        [InlineData("test.pdf", "pass", 3)] //Created by PdfSharp
        public void TestPasswordProtectedDocument(string fileName, string openPassword, int expectedPageCount)
        {
            var path = Path.Combine("data", fileName);
            Assert.Throws<PdfReaderException>(() =>
            {
                PdfReader.Open(path);
            });
            var doc = PdfReader.Open(path, openPassword);
            doc.PageCount.Should().Be(expectedPageCount);
        }

        [Theory]
        [InlineData("3 pages.pdf", 3)]
        [InlineData("image.pdf", 1)]
        public void TestRegularDocument(string fileName, int expectedPageCount)
        {
            var path = Path.Combine("data", fileName);
            var doc = PdfReader.Open(path);
            doc.PageCount.Should().Be(expectedPageCount);
        }
    }
}
