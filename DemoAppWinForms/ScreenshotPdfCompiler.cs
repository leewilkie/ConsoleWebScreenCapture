using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Linq;
using System.Collections.Generic;
using PrimeGlobalPeople;

public static class ScreenshotPdfCompiler
{
    public static void CompileScreenshotsToPdf(List<Capture> captures, string screenshotsDir, string outputPdfPath)
    {
        var document = new PdfDocument();
        foreach (var capture in captures)
        {
            string fileName = capture.Name;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            string screenshotPath = Path.Combine(screenshotsDir, $"{fileName}.png");
            if (!File.Exists(screenshotPath)) continue;
            using (var image = XImage.FromFile(screenshotPath))
            {
                var page = document.AddPage();
                double headerHeight = 40;
                page.Width = image.PixelWidth * 72 / image.HorizontalResolution;
                page.Height = (image.PixelHeight * 72 / image.VerticalResolution) + headerHeight;
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    var font = new XFont("Arial", 14, XFontStyleEx.Bold);
                    gfx.DrawString($"{capture.Name}", font, XBrushes.Black, new XRect(0, 0, page.Width, 20), XStringFormats.TopLeft);
                    var fontSmall = new XFont("Arial", 10, XFontStyleEx.Regular);
                    gfx.DrawString($"{capture.Url}", fontSmall, XBrushes.Black, new XRect(0, 20, page.Width, 20), XStringFormats.TopLeft);
                    gfx.DrawImage(image, 0, headerHeight, page.Width, page.Height - headerHeight);
                }
            }
        }
        document.Save(outputPdfPath);
        Console.WriteLine($"PDF compiled and saved to {outputPdfPath}");
    }
}
