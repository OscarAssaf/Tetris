using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace Tetris.Tests;

public class BrowserTests
{
    [Fact]
    public async Task Player_can_start_control_pause_and_resume_the_game()
    {
        var root = FindRepositoryRoot();
        const string url = "http://127.0.0.1:5187";
        using var server = Process.Start(new ProcessStartInfo("dotnet",
            $"run --project \"{Path.Combine(root, "Tetris.csproj")}\" --no-launch-profile --urls {url}")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;

        IPlaywright? playwright = null;
        IBrowser? browser = null;
        try
        {
            using var client = new HttpClient();
            var ready = false;
            for (var attempt = 0; attempt < 90 && !server.HasExited; attempt++)
            {
                try
                {
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) { ready = true; break; }
                }
                catch (HttpRequestException) { }
                await Task.Delay(1000);
            }
            Assert.True(ready, "The Blazor app did not start within 90 seconds.");

            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(url);
            await page.GetByRole(AriaRole.Button, new() { Name = "Start Game" }).ClickAsync();
            await Assertions.Expect(page.Locator("h1")).ToHaveTextAsync("Tetris");

            var occupiedAtStart = await page.Locator(".board .cell.filled").CountAsync();
            Assert.Equal(4, occupiedAtStart);
            await page.Locator(".tetris-app").ClickAsync();
            await page.Keyboard.PressAsync("ArrowDown");
            await Assertions.Expect(page.Locator(".panel-box").Filter(new() { HasText = "Score" }).Locator(".panel-value"))
                .ToHaveTextAsync("1");

            await page.Locator(".side-panel .btn-small").ClickAsync();
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Paused" })).ToBeVisibleAsync();
            await page.Locator(".overlay .btn").ClickAsync();
            await Assertions.Expect(page.Locator(".overlay")).ToHaveCountAsync(0);
        }
        finally
        {
            if (browser is not null) await browser.DisposeAsync();
            playwright?.Dispose();
            if (!server.HasExited) server.Kill(entireProcessTree: true);
            await server.WaitForExitAsync();
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Tetris.csproj")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate Tetris.csproj.");
    }
}
