using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class DailySalesRepository : BaseRepository<DailySales>, IRepository<DailySales>
{
    public DailySalesRepository(ApplicationDbContext context) : base(context)
    {
    }
}
