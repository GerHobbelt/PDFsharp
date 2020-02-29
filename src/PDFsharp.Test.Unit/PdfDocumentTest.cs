using FluentAssertions;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using System;
using System.IO;
using Xunit;

namespace PdfSharp.Pdf.Test
{
    public class PdfDocumentTest : IClassFixture<PdfDocumentFixture>
    {
        private readonly PdfDocumentFixture fixture;

        public PdfDocumentTest(PdfDocumentFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestImportMode()
        {
            var pageCount = 1;
            using (var pdfStream = fixture.CreateInMemoryPdf(pageCount))
            {
                var pdf = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
                pdf.PageCount.Should().Be(pageCount);
                pdf.Pages[0].Should().NotBeNull();

                pdf.CanSave(out string _).Should().BeFalse();
                pdf.Invoking(p => p.AddPage()).Should().Throw<InvalidOperationException>();
                pdf.Invoking(p => p.Save(".\\test.pdf")).Should().Throw<InvalidOperationException>();
            }
        }

        [Fact]
        public void TestModifyMode()
        {
            var pageCount = 1;
            using (var pdfStream = fixture.CreateInMemoryPdf(pageCount))
            {
                var pdf = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Modify);
                pdf.AddPage().Should().NotBeNull();

                pdf.PageCount.Should().BeGreaterThan(pageCount);
                pdf.Pages[1].Should().NotBeNull();
            }
        }

        [Fact]
        public void TestPermissionsBug_Issue4()
        {
            var password = "test1234";
            using (var pdfStream = fixture.CreateInMemoryPdf(1, pdf =>
            {
                pdf.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.None;
                pdf.SecuritySettings.OwnerPassword = password;
                pdf.SecuritySettings.PermitAccessibilityExtractContent = false;
                pdf.SecuritySettings.PermitAnnotations = false;
                pdf.SecuritySettings.PermitAssembleDocument = false;
                pdf.SecuritySettings.PermitExtractContent = true;
                pdf.SecuritySettings.PermitFormsFill = false;
                pdf.SecuritySettings.PermitFullQualityPrint = true;
                pdf.SecuritySettings.PermitModifyDocument = false;
                pdf.SecuritySettings.PermitPrint = true;
            }))
            {
                this.Invoking(_ => PdfReader.Open(pdfStream, password, PdfDocumentOpenMode.Modify)).Should().NotThrow<InvalidCastException>();
            }
        }
    }
}
