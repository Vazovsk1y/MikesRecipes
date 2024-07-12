using Microsoft.AspNetCore.Mvc;

namespace MikesRecipes.WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BaseController : ControllerBase
{
}