

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PrimeGlobalPeople;

class Program
{
    static async Task Main(string[] args)
    {
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
            if (config.MenuUrls != null && config.MenuUrls.Count > 0)
            {
                foreach (var menuUrl in config.MenuUrls)
                {
                    Console.WriteLine($"Taking screenshot of: {menuUrl}");
                    await TakeScreenshotAsync(menuUrl);
                }
            }
            else
            {
                Console.WriteLine($"Taking screenshot of: {config.Website}");
                await TakeScreenshotAsync(config.Website);
            }
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

    static async Task TakeScreenshotAsync(string url)
    {
        Console.WriteLine($"Navigating to: {url}");
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(url);
        string fileName = url;
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        // Store screenshots in Screenshots folder within the project folder
        string projectScreenshotsDir = Path.Combine("Projects", GetProjectFolderNameFromUrl(url), "Screenshots");
        if (!Directory.Exists(projectScreenshotsDir))
        {
            Directory.CreateDirectory(projectScreenshotsDir);
        }
        string screenshotPath = Path.Combine(projectScreenshotsDir, $"{fileName}.png");
    await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
        Console.WriteLine($"Screenshot saved to {screenshotPath}");
        await browser.CloseAsync();

    }

    static string GetProjectFolderNameFromUrl(string url)
    {
        // Example: https://primeglobalpeople.com/ -> PrimeGlobalPeople
        try
        {
            var uri = new Uri(url);
            string host = uri.Host.Replace("www.", "");
            string[] parts = host.Split('.');
            // Take the first part and PascalCase it
            if (parts.Length > 0)
            {
                string name = string.Join("", parts).Replace("com", "");
                // Capitalize each word
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name).Replace(" ", "");
            }
        }
        catch { }
        return "UnknownProject";
    }
}
