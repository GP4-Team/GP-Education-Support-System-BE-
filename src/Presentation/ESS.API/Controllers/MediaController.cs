[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] string tempFolder)
    {
        var filePath = await _mediator.Send(new UploadFileCommand(file, tempFolder));
        return Ok(new { FilePath = filePath });
    }

    [HttpGet("{resourceId}/collection/{collection}")]
    public async Task<IActionResult> GetByCollection(Guid resourceId, string collection)
    {
        var media = await _mediator.Send(new GetMediaByCollectionQuery(resourceId, collection));
        return Ok(media);
    }
}
