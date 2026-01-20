using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, IRepository<Category>
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }
}
