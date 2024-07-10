using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SampleApiDbContext _context;

        public OrderRepository(SampleApiDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders.Where(o => !o.WasOrderDeleted).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(DateTime fromDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate.Date >= fromDate && !o.WasOrderDeleted)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Where(o => o.Id == id && !o.WasOrderDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.WasOrderDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
