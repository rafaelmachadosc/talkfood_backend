using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories;

public class OrderPaymentRepository : BaseRepository<OrderPayment>, IRepository<OrderPayment>
{
    public OrderPaymentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
