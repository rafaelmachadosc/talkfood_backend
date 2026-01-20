using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class ItemRepository : BaseRepository<Item>, IRepository<Item>
{
    public ItemRepository(ApplicationDbContext context) : base(context)
    {
    }
}
