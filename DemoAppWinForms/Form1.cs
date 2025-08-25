using System.Text.Json;
using Microsoft.Playwright;
using PdfSharp;
using PdfSharp.Drawing;

namespace DemoAppWinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        PdfSharp.Fonts.GlobalFontSettings.UseWindowsFontsUnderWindows = true;
    }

    private async void buttonCapture_Click(object sender, EventArgs e)
    {
        string projectName = textBoxProject.Text;
        string configPath = Path.Combine("Projects", projectName, "config.json");
        if (!File.Exists(configPath))
        {
            MessageBox.Show($"Config file not found for project: {projectName}");
            return;
        }
        var configJson = await File.ReadAllTextAsync(configPath);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var config = System.Text.Json.JsonSerializer.Deserialize<DemoAppWinForms.ProjectConfig>(configJson, options);
        if (config == null || config.Captures == null || config.Captures.Count == 0)
        {
            MessageBox.Show("No captures found in config.");
            return;
        }
        string screenshotsDir = Path.Combine("Projects", projectName, "Screenshots");
        textBoxProgress.Clear();
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new Microsoft.Playwright.BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        foreach (var capture in config.Captures)
        {
            textBoxProgress.AppendText($"Navigating to: {capture.Url}\r\n");
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
            await page.ScreenshotAsync(new Microsoft.Playwright.PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            textBoxProgress.AppendText($"Screenshot saved to {screenshotPath}\r\n");
        }
        await browser.CloseAsync();
        textBoxProgress.AppendText("Compiling PDF...\r\n");
        string pdfPath = Path.Combine(screenshotsDir, "Screenshots.pdf");
        ScreenshotPdfCompiler.CompileScreenshotsToPdf(config.Captures, screenshotsDir, pdfPath);
        textBoxProgress.AppendText($"PDF compiled and saved to {pdfPath}\r\n");
        MessageBox.Show($"Screenshots and PDF saved to {screenshotsDir}");
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
