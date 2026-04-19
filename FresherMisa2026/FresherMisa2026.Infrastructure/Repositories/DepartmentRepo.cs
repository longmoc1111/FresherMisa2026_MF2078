using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace FresherMisa2026.Infrastructure.Repositories
{                                                   
    public class DepartmentRepo : BaseRepository<Department>, IDepartmentRepo
    {
        public DepartmentRepo(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
