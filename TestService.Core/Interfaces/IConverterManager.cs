using TestService.Core.Models;

namespace TestService.Core.Interfaces;
public interface IConverterManager
{
    public Task<FileConverterInfosResult> GetFilesInfoAsync(int skip = 0, int take = 0);
    public Task QueueAsync(FileConvertInfo info, Stream data);
    public Task<bool> DeleteAsync(Guid id);
    public Task<(Stream stream, string fileName)?> GetConvertedDataAsync(Guid id);
    public Task ReInitialize();
}
