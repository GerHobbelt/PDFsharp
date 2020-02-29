using System;
using System.IO;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Performance.Profile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Reading {args.Length} files...");
            foreach (var path in args)
            {
                if (File.Exists(path))
                {
                    Console.Write($"Reading {path}...");
                    var pdf = PdfReader.OpenAsync(new FileStream(path, FileMode.Open), PdfDocumentOpenMode.Modify).Result;
                    Console.WriteLine($" Complete! {pdf.PageCount} pages read.");
                }
                else
                    Console.Error.WriteLine("File not found:  " + path);
            }
#if DEBUG
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
#endif
        }
    }
}
