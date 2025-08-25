

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PrimeGlobalPeople;
using PdfSharp.Drawing;

class Program
{
    static async Task Main(string[] args)
    {
        PdfSharp.Fonts.GlobalFontSettings.UseWindowsFontsUnderWindows = true;

        if (args.Length > 0)
        {
            string projectName = args[0];
            var config = await ReadConfigAsync(projectName);
            if (config == null)
            {
                Console.WriteLine("Config is null.");
                return;
            }
            if (string.IsNullOrWhiteSpace(config.Project))
            {
                Console.WriteLine("Project name is missing in config.");
                return;
            }
            if (string.IsNullOrWhiteSpace(config.Website))
            {
                Console.WriteLine("Website URL is missing in config.");
                return;
            }
            if (!Uri.IsWellFormedUriString(config.Website, UriKind.Absolute))
            {
                Console.WriteLine("Website URL in config is not valid.");
                return;
            }
            string screenshotsDir = Path.Combine("Projects", config.Project, "Screenshots");
            if (config.Captures != null && config.Captures.Count > 0)
            {
                await TakeScreenshotsAsync(config.Captures, screenshotsDir);
            }
            else
            {
                await TakeScreenshotsAsync(new System.Collections.Generic.List<Capture> { new Capture { Name = config.Project, Url = config.Website } }, screenshotsDir);
            }
            // PDF creation is now handled in TakeScreenshotsAsync
        }
        else
        {
            Console.WriteLine("Please provide the project name as the first command line argument.");
        }
    }

    static async Task<ProjectConfig?> ReadConfigAsync(string projectName)
    {
        string configPath = Path.Combine("Projects", projectName, "config.json");
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Config file not found for project: {projectName}");
            return null;
        }
        var configJson = await File.ReadAllTextAsync(configPath);
        Console.WriteLine(configJson);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ProjectConfig>(configJson, options);
    }

    static async Task TakeScreenshotsAsync(System.Collections.Generic.List<Capture> captures, string screenshotsDir)
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        var pdfDocument = new PdfSharp.Pdf.PdfDocument();
        foreach (var capture in captures)
        {
            Console.WriteLine($"Navigating to: {capture.Url}");
            await page.GotoAsync(capture.Url);
            string fileName = capture.Name;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            if (!Directory.Exists(screenshotsDir))
            {
                Directory.CreateDirectory(screenshotsDir);
            }
            string screenshotPath = Path.Combine(screenshotsDir, $"{fileName}.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            Console.WriteLine($"Screenshot saved to {screenshotPath}");

            // Add to PDF
            using (var image = PdfSharp.Drawing.XImage.FromFile(screenshotPath))
            {
                var pagePdf = pdfDocument.AddPage();
                double headerHeight = 40;
                pagePdf.Width = image.PixelWidth * 72 / image.HorizontalResolution;
                pagePdf.Height = (image.PixelHeight * 72 / image.VerticalResolution) + headerHeight;
                using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(pagePdf))
                {
                    var font = new XFont("Arial", 14,  XFontStyleEx.Bold);
                    gfx.DrawString($"{capture.Name}", font, XBrushes.Black, new XRect(0, 0, pagePdf.Width, 20), XStringFormats.TopLeft);
                    var fontSmall = new XFont("Arial", 10, XFontStyleEx.Regular);
                    gfx.DrawString($"{capture.Url}", fontSmall, XBrushes.Black, new XRect(0, 20, pagePdf.Width, 20), XStringFormats.TopLeft);
                    gfx.DrawImage(image, 0, headerHeight, pagePdf.Width, pagePdf.Height - headerHeight);
                }
            }
        }
        await browser.CloseAsync();
        string pdfPath = Path.Combine(screenshotsDir, "Screenshots.pdf");
        pdfDocument.Save(pdfPath);
        Console.WriteLine($"PDF compiled and saved to {pdfPath}");
    }
}
