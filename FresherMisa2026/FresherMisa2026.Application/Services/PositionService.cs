using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Position;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Services
{
    public class PositionService : BaseService<Position> , IPositionService
    {
        IPositionRepo _PositionRepo;
        public PositionService(IPositionRepo positionRepo) : base(positionRepo)
        {
            _PositionRepo=positionRepo;
        }
    }
}
