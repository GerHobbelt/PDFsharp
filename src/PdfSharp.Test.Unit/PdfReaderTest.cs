using FluentAssertions;
using PdfSharp.Pdf.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfSharp.Pdf.IO.Test.Unit
{
    public class PdfReaderTest
    {
        [Fact]
        public void TestOpenUsingStream()
        {
            const string testFilePath = @"data\3 pg doc.pdf";
            using (var stream = new FileStream(testFilePath, FileMode.Open))
            {
                var pdf = PdfReader.Open(stream);
                pdf.PageCount.Should().Be(3);
            }
        }

        [Fact]
        public async Task TestOpenAsyncUsingStream()
        {
            const string testFilePath = @"data\3 pg doc.pdf";
            using (var stream = new FileStream(testFilePath, FileMode.Open))
            {
                var pdf = await PdfReader.OpenAsync(stream);
                pdf.PageCount.Should().Be(3);
            }
        }
    }
}
