namespace TestService.Core.Models;
public class FileConvertInfo
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public FileConvertStatus Status { get; set; }
    public DateTime CreateDate { get; set; }
}

public enum FileConvertStatus
{
    Created,
    Processing,
    Completed,
    Error
}