using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IRepository<Product>
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}
