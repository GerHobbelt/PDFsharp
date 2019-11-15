using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace PdfSharp.Test.Integration
{
    public class PdfCreationTest
    {
        public PdfCreationTest()
        {
            Helpers.ResetTempFolder();
        }

        [Fact]
        public void TestAES128Encryption()
        {
            var pageCount = 3;
            var password = "pass";

            var doc = new PdfDocument();
            var font = new XFont(GlobalFontSettings.DefaultFontName, 12, XFontStyle.Regular);
            for (int i = 1; i <= pageCount; i++)
            {
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawString($"Page {i}", font, XBrushes.Black, 25, 25);
            }
            doc.SecuritySettings.DocumentSecurityLevel = Pdf.Security.PdfDocumentSecurityLevel.Encrypted128BitAes;
            doc.SecuritySettings.UserPassword = password;
            var path = Path.Combine("temp", "test.pdf");
            doc.Save(path);
            File.Exists(path).Should().BeTrue();

            var newDoc = PdfReader.Open(path, password, PdfDocumentOpenMode.InformationOnly);
            newDoc.PageCount.Should().Be(pageCount);
        }
    }
}
