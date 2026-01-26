using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class PrinterRepository : BaseRepository<Printer>, IRepository<Printer>
{
    public PrinterRepository(ApplicationDbContext context) : base(context)
    {
    }
}
