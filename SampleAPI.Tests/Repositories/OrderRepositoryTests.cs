using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SampleAPI.Tests.Repositories
{
    [TestClass]
    public class OrderRepositoryTests
    {
        private SampleApiDbContext _context;
        private IOrderRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _context = MockSampleApiDbContextFactory.GenerateMockContext();
            _repository = new OrderRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_OrderExists_ReturnsOrder()
        {
            int orderId = 1;

            var order = await _repository.GetOrderByIdAsync(orderId);

            Assert.IsNotNull(order);
            Assert.AreEqual(orderId, order.Id);
            Assert.AreEqual("Customer 1", order.CustomerName);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_OrderDoesNotExist_ReturnsNull()
        {
            int orderId = 999;

            var order = await _repository.GetOrderByIdAsync(orderId);

            Assert.IsNull(order);
        }

        [TestMethod]
        public async Task AddOrderAsync_ValidOrder_OrderIsAdded()
        {
            var newOrder = new SampleAPI.Entities.Order
            {
                Id = 3,
                OrderDate = DateTime.Now,
                Description = "Order 3 Description",
                CustomerName = "Customer 3",
                WasOrderInvoiced = false,
                WasOrderDeleted = false
            };

            var order = await _repository.AddOrderAsync(newOrder);

            var addedOrder = await _context.Orders.FindAsync(3);
            Assert.IsNotNull(addedOrder);
            Assert.AreEqual("Order 3 Description", addedOrder.Description);
            Assert.AreEqual("Customer 3", addedOrder.CustomerName);
        }

        [TestMethod]
        public async Task GetOrdersAsync_ReturnsAllOrders()
        {
            var orders = await _repository.GetOrdersAsync();

            Assert.AreEqual(2, orders.Count());
        }

        [TestMethod]
        public async Task GetRecentOrdersAsync_ReturnsRecentOrders()
        {
            var fromDate = DateTime.Now.AddDays(-4);

            var orders = await _repository.GetRecentOrdersAsync(fromDate);

            Assert.AreEqual(1, orders.Count());
            Assert.AreEqual("Order 2 Description", orders.First().Description);
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ValidOrder_OrderIsUpdated()
        {
            var existingOrder = await _context.Orders.FindAsync(1);
            existingOrder.Description = "Updated Description";

            var order = await _repository.UpdateOrderAsync(existingOrder);

            var updatedOrder = await _context.Orders.FindAsync(1);
            Assert.IsNotNull(updatedOrder);
            Assert.AreEqual("Updated Description", updatedOrder.Description);
        }

        [TestMethod]
        public async Task DeleteOrderAsync_OrderExists_OrderIsDeleted()
        {
            int orderId = 1;

            await _repository.DeleteOrderAsync(orderId);

            var order = await _context.Orders.FindAsync(orderId);
            Assert.IsNotNull(order);
            Assert.IsTrue(order.WasOrderDeleted);
        }
    }
}
