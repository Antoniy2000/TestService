namespace TestService.Core.Interfaces;
public interface IConverterWorkerService
{
    public Task ConvertHtmlToPdfAsync(string htmlPath, string outputPath);
}
