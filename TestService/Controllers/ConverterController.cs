using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TestService.Core.Interfaces;
using TestService.Core.Models;

namespace TestService.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConverterController : ControllerBase
{
    private readonly IConverterManager _converterManager;
    public ConverterController(IConverterManager converterManager)
    {
        _converterManager = converterManager;
    }

    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<FileConvertInfo>))]
    public async Task<ActionResult<FileConverterInfosResult>> GetConverterItems(int skip, int take)
    {
        return Ok(await _converterManager.GetFilesInfoAsync(skip, take));
    }

    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(FileStreamResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(void))]
    public async Task<ActionResult> GetConvertedData([Required] Guid id)
    {
        var res = await _converterManager.GetConvertedDataAsync(id);
        if (res is null)
        {
            return BadRequest();
        }
        Response.Headers["Access-Control-Expose-Headers"] = "Content-Disposition";
        return File(res.Value.stream, "application/octet-stream", res.Value.fileName);
    }

    [HttpPost]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(void))]
    public async Task<OkResult> QueueItem([Required] IFormFile file)
    {
        var info = new FileConvertInfo
        {
            Id = Guid.NewGuid(),
            Status = FileConvertStatus.Created,
            CreateDate = DateTime.Now,
            FileName = file.FileName,
        };
        await _converterManager.QueueAsync(info, file.OpenReadStream());
        return Ok();
    }

    [HttpDelete]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(void))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(void))]
    public async Task<ActionResult> DeleteItem([Required] Guid id)
    {
        if (await _converterManager.DeleteAsync(id))
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
}
