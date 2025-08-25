using System;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

public static class ScreenshotPdfCompiler
{
    public static void CompileScreenshotsToPdf(string screenshotsDir, string outputPdfPath)
    {
        var pngFiles = Directory.GetFiles(screenshotsDir, "*.png").OrderBy(f => f).ToList();
        if (pngFiles.Count == 0)
        {
            Console.WriteLine("No screenshots found to compile into PDF.");
            return;
        }
        var document = new PdfDocument();
        foreach (var file in pngFiles)
        {
            var page = document.AddPage();
            using (var image = XImage.FromFile(file))
            {
                page.Width = image.PixelWidth * 72 / image.HorizontalResolution;
                page.Height = image.PixelHeight * 72 / image.VerticalResolution;
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                }
            }
        }
        document.Save(outputPdfPath);
        Console.WriteLine($"PDF compiled and saved to {outputPdfPath}");
    }
}
