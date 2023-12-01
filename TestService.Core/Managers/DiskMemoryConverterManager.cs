using System.Text.Json;
using TestService.Core.Interfaces;
using TestService.Core.Models;

namespace TestService.Core.Managers;
public class DiskMemoryConverterManager : IConverterManager
{
    private const string FileInfoName = "info.json";
    private const string FileDataName = "data";
    private const string ConvertedFileDataName = "convertedData";

    private readonly string _cachePath;
    private readonly IConverterWorkerService _workerService;
    public DiskMemoryConverterManager(string cachePath, IConverterWorkerService workerService)
    {
        _cachePath = cachePath;
        _workerService = workerService;
    }

    public async Task QueueAsync(FileConvertInfo info, Stream data)
    {
        var dirPath = GetFileDirectoryPath(info.Id);
        Directory.CreateDirectory(dirPath);
        await StoreInfoAsync(info);

        using var fstream = File.Create(GetFileDataPath(info.Id));
        await data.CopyToAsync(fstream);
        await fstream.FlushAsync();

        StartAsync(info);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var info = await GetInfoAsync(id);
        if (info?.Status is FileConvertStatus.Completed or FileConvertStatus.Error)
        {
            var dirPath = GetFileDirectoryPath(id);
            Directory.Delete(dirPath, true);
            return true;
        }
        return false;
    }

    public async Task<(Stream stream, string fileName)?> GetConvertedDataAsync(Guid id)
    {
        var info = await GetInfoAsync(id);

        if (info?.Status is FileConvertStatus.Completed)
        {
            var filePath = GetFileConvertedDataPath(id);
            try
            {
                var fi = new FileInfo(info.FileName);
                var fileName = fi.Name;
                if (!string.IsNullOrEmpty(fi.Extension))
                {
                    fileName = fileName.Replace(fi.Extension, string.Empty);
                }
                return (File.OpenRead(filePath), fileName + ".pdf");
            }
            catch
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public async Task<FileConverterInfosResult> GetFilesInfoAsync(int skip = 0, int take = 0)
    {
        var dirInfo = new DirectoryInfo(_cachePath);

        var tasks = dirInfo.GetDirectories().Select(async x =>
        {
            var fileContent = await File.ReadAllTextAsync(Path.Combine(x.FullName, FileInfoName));
            return JsonSerializer.Deserialize<FileConvertInfo>(fileContent)!;
        });

        var taskResult = await Task.WhenAll(tasks);
        var data = taskResult.OrderByDescending(x => x.CreateDate).AsEnumerable();
        if (skip > 0)
        {
            data = data.Skip(skip);
        }
        if (take > 0)
        {
            data = data.Take(take);
        }
        var result = new FileConverterInfosResult
        {
            Count = taskResult.Length,
            Data = data.ToList(),
        };
        return result;
    }

    private async Task StoreInfoAsync(FileConvertInfo info)
    {
        var filePath = GetFileInfoPath(info.Id);

        Stream stream;
        if (File.Exists(filePath))
        {
            stream = File.OpenWrite(filePath);
        }
        else
        {
            stream = File.CreateText(filePath).BaseStream;
        }
        await JsonSerializer.SerializeAsync(stream, info);
        await stream.FlushAsync();
        await stream.DisposeAsync();
    }

    private async Task<FileConvertInfo?> GetInfoAsync(Guid id)
    {
        var dirPath = GetFileDirectoryPath(id);
        if (!Directory.Exists(dirPath))
        {
            return null;
        }
        var info = JsonSerializer.Deserialize<FileConvertInfo>(await File.ReadAllTextAsync(GetFileInfoPath(id)))!;
        return info;
    }

    private string GetFileDirectoryPath(Guid id) => Path.Combine(_cachePath, id.ToString());
    private string GetFileConvertedDataPath(Guid id) => Path.Combine(GetFileDirectoryPath(id), ConvertedFileDataName);
    private string GetFileDataPath(Guid id) => Path.Combine(GetFileDirectoryPath(id), FileDataName);
    private string GetFileInfoPath(Guid id) => Path.Combine(GetFileDirectoryPath(id), FileInfoName);

    private async void StartAsync(FileConvertInfo info)
    {
        var filePath = GetFileDataPath(info.Id);
        var outPath = GetFileConvertedDataPath(info.Id);

        info.Status = FileConvertStatus.Processing;
        await StoreInfoAsync(info);

        try
        {
            await _workerService.ConvertHtmlToPdfAsync(filePath, outPath);
            info.Status = FileConvertStatus.Completed;
        }
        catch
        {
            info.Status = FileConvertStatus.Error;
        }

        await StoreInfoAsync(info);
    }

    public async Task ReInitialize()
    {
        var infos = await GetFilesInfoAsync();
        foreach (var info in infos.Data.Where(x => x.Status is FileConvertStatus.Processing or FileConvertStatus.Created))
        {
            StartAsync(info);
        }
    }
}
