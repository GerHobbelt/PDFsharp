using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace PdfSharp.Test.Integration
{
    public static class Helpers
    {
        public static void ResetTempFolder()
        {
            try
            {
                if (Directory.Exists("temp"))
                    Directory.Delete("temp", true);
                Directory.CreateDirectory("temp");
            }
            catch (Exception e)
            {
                Assert.True(false, $"Error resetting temp folder:  {e.Message}");
            }
            Assert.True(Directory.Exists("temp") && Directory.EnumerateFiles("temp").Count() == 0, "Reset temp folder failed");
        }
    }
}
