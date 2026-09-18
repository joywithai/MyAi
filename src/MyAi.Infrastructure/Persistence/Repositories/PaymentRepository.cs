using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class PaymentRepository : BaseRepository<PaymentTransaction>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
