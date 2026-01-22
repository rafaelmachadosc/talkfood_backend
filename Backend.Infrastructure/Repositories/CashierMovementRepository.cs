using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class CashierMovementRepository : BaseRepository<CashierMovement>, IRepository<CashierMovement>
{
    public CashierMovementRepository(ApplicationDbContext context) : base(context)
    {
    }
}
