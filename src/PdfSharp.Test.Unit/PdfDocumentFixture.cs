using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfSharp.Pdf.Test
{
    public class PdfDocumentFixture
    {
        public PdfDocument CreatePdf(Stream stream, int pageCount = 1, Action<PdfDocument> setPropertiesDelegate = null)
        {
            var pdf = new PdfDocument();
            for (int i = 0; i < pageCount; i++)
                pdf.AddPage();
            setPropertiesDelegate?.Invoke(pdf);
            pdf.Save(stream);
            return pdf;
        }

        public Stream CreateInMemoryPdf(int pageCount = 1, Action<PdfDocument> setPropertiesDelegate = null)
        {
            var pdfStream = new MemoryStream();
            var pdf = CreatePdf(pdfStream, pageCount, setPropertiesDelegate);
            pdfStream.Position = 0;
            return pdfStream;
        }
    }
}
