using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PdfSharp.Pdf.Test
{
    public class PdfDictionaryTest
    {
        private readonly PdfDictionary dict = new PdfDictionary();

        [Fact]
        public void TestGetInteger()
        {
            dict.Elements.Add("/P", new PdfInteger(11));
            dict.Elements.GetInteger("/P").Should().Be(11);
        }

        [Fact]
        public void TestUIntConversion()
        {
            dict.Elements.Add("/P", new PdfUInteger(3u));

            dict.Elements.Invoking(d => d.GetInteger("/P")).Should().NotThrow<InvalidCastException>();
            dict.Elements.GetInteger("/P").Should().Be(3);
        }

        [Fact]
        public void TestUIntConversionOverflow()
        {
            dict.Elements.Add("/P", new PdfUInteger(uint.MaxValue));
            dict.Elements.Invoking(d => d.GetInteger("/P")).Should().Throw<InvalidCastException>();
        }
    }
}
