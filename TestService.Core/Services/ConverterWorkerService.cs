using PuppeteerSharp;
using TestService.Core.Interfaces;

namespace TestService.Core.Services;
public class ConverterWorkerService : IConverterWorkerService
{
    private readonly SemaphoreSlim _semaphore;
    public ConverterWorkerService(int threadCount)
    {
        _semaphore = new SemaphoreSlim(threadCount, threadCount);
        using var browserFetcher = new BrowserFetcher();
        browserFetcher.DownloadAsync().Wait();
    }

    public async Task ConvertHtmlToPdfAsync(string htmlPath, string outputPath)
    {
        await _semaphore.WaitAsync();
        try
        {
            var browser = await Puppeteer.LaunchAsync(new());
            using var page = await browser.NewPageAsync();

            var content = await File.ReadAllTextAsync(htmlPath);
            await page.SetContentAsync(content);
        
            var result = await page.GetContentAsync();
            await page.PdfAsync(outputPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
