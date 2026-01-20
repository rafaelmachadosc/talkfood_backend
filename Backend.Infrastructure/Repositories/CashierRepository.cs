using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class CashierRepository : BaseRepository<Cashier>, IRepository<Cashier>
{
    public CashierRepository(ApplicationDbContext context) : base(context)
    {
    }
}
