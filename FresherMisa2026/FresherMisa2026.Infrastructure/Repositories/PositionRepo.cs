using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Position;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class PositionRepo : BaseRepository<Position>, IPositionRepo
    {
        public PositionRepo(IConfiguration configuration, IMemoryCache cache) : base(configuration, cache)
        {
        }
    }
}
