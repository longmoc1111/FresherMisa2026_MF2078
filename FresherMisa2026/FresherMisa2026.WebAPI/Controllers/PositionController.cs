using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Position;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    public class PositionController : BaseController<Position>
    {
        IPositionService _PositionService;
        public PositionController(IPositionService positionService) : base(positionService)
        {
            _PositionService  = positionService;
        }
    }
}
