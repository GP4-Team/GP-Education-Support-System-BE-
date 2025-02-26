// src/Presentation/ESS.API/Controllers/ApiControllerBase.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ESS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}