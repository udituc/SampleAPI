using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleAPI.Controllers;
using SampleAPI.Entities;
using SampleAPI.Requests;
using SampleAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SampleAPI.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderRepository> _mockRepo;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockRepo = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResult_WithAListOfOrders()
        {
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderDate = DateTime.UtcNow, Description = "Test Order 1", CustomerName = "Customer 1", WasOrderInvoiced = false, WasOrderDeleted = false },
                new Order { Id = 2, OrderDate = DateTime.UtcNow, Description = "Test Order 2", CustomerName = "Customer 2", WasOrderInvoiced = true, WasOrderDeleted = false }
            };
            _mockRepo.Setup(repo => repo.GetOrdersAsync()).ReturnsAsync(orders);

            var result = await _controller.GetOrders();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrders = Assert.IsType<List<Order>>(okResult.Value);
            Assert.Equal(2, returnOrders.Count);
            Assert.Equal("Customer 1", returnOrders[0].CustomerName);
            Assert.Equal("Customer 2", returnOrders[1].CustomerName);
        }

        [Fact]
        public async Task GetOrder_ReturnsOkResult_WithAnOrder()
        {
            var order = new Order { Id = 1, OrderDate = DateTime.UtcNow, Description = "Test Order", CustomerName = "Customer 1", WasOrderInvoiced = false, WasOrderDeleted = false };
            _mockRepo.Setup(repo => repo.GetOrderByIdAsync(1)).ReturnsAsync(order);
            
            var result = await _controller.GetOrder(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrder = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(1, returnOrder.Id);
            Assert.Equal("Test Order", returnOrder.Description);
            Assert.Equal("Customer 1", returnOrder.CustomerName);
        }

        [Fact]
        public async Task GetOrder_ReturnsNotFoundResult()
        {
            _mockRepo.Setup(repo => repo.GetOrderByIdAsync(1)).ReturnsAsync((Order)null);
            
            var result = await _controller.GetOrder(1);
            
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetRecentOrders_ReturnsOkResult_WithAListOfOrders()
        {
            int daysBefore = 2;
            var fromDate = DateTime.UtcNow.AddDays(-daysBefore).Date;
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderDate = DateTime.UtcNow.AddHours(-1), Description = "Recent Order 1", CustomerName = "Customer 1", WasOrderInvoiced = false, WasOrderDeleted = false },
                new Order { Id = 2, OrderDate = DateTime.UtcNow.AddHours(-2), Description = "Recent Order 2", CustomerName = "Customer 2", WasOrderInvoiced = true, WasOrderDeleted = false }
            };
            _mockRepo.Setup(repo => repo.GetRecentOrdersAsync(fromDate)).ReturnsAsync(orders);

            var result = await _controller.GetRecentOrders(daysBefore);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrders = Assert.IsType<List<Order>>(okResult.Value);
            Assert.Equal(2, returnOrders.Count);
            Assert.Equal("Recent Order 1", returnOrders[0].Description);
            Assert.Equal("Recent Order 2", returnOrders[1].Description);
        }

        [Fact]
        public async Task GetRecentOrders_ReturnsOkResult_WithEmptyList()
        {
            var fromDate = DateTime.UtcNow.AddDays(-1).Date;
            var orders = new List<Order>();
            _mockRepo.Setup(repo => repo.GetRecentOrdersAsync(fromDate)).ReturnsAsync(orders);

            var result = await _controller.GetRecentOrders(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrders = Assert.IsType<List<Order>>(okResult.Value);
            Assert.Empty(returnOrders);
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedAtActionResult()
        {
            var createOrderRequest = new OrderRequest
            {
                OrderDate = "01-01-2024",
                Description = "New Order",
                CustomerName = "Customer 1",
                WasOrderInvoiced = false,
                WasOrderDeleted = false
            };
            var order = new Order
            {
                Id = 3,
                OrderDate = DateTime.ParseExact("01-01-2024", "dd-MM-yyyy", null),
                Description = "New Order",
                CustomerName = "Customer 1",
                WasOrderInvoiced = false,
                WasOrderDeleted = false
            };
            _mockRepo.Setup(repo => repo.AddOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);

            var result = await _controller.CreateOrder(createOrderRequest);
            
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnOrder = Assert.IsType<Order>(createdAtActionResult.Value);
            Assert.Equal(3, returnOrder.Id);
            Assert.Equal("New Order", returnOrder.Description);
            Assert.Equal("Customer 1", returnOrder.CustomerName);
            Assert.False(returnOrder.WasOrderInvoiced);
            Assert.False(returnOrder.WasOrderDeleted);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNoContentResult()
        {
            var updateOrderRequest = new OrderRequest
            {
                OrderDate = "01-01-2024",
                Description = "Updated Order",
                CustomerName = "Customer 1",
                WasOrderInvoiced = true,
                WasOrderDeleted = false
            };
            var order = new Order
            {
                Id = 1,
                OrderDate = DateTime.ParseExact("01-01-2024", "dd-MM-yyyy", null),
                Description = "Updated Order",
                CustomerName = "Customer 1",
                WasOrderInvoiced = true,
                WasOrderDeleted = false
            };
            _mockRepo.Setup(repo => repo.UpdateOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);
            
            var result = await _controller.UpdateOrder(1, updateOrderRequest);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNotFoundResult()
        {
            var updateOrderRequest = new OrderRequest
            {
                OrderDate = "01-01-2024",
                Description = "Updated Order",
                CustomerName = "Customer 1",
                WasOrderInvoiced = true,
                WasOrderDeleted = false
            };
            _mockRepo.Setup(repo => repo.UpdateOrderAsync(It.IsAny<Order>())).ReturnsAsync((Order)null);

            var result = await _controller.UpdateOrder(1, updateOrderRequest);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_ReturnsNoContentResult()
        {
            var order = new Order { Id = 1, OrderDate = DateTime.UtcNow, Description = "Test Order", CustomerName = "Customer 1", WasOrderInvoiced = false, WasOrderDeleted = false };
            _mockRepo.Setup(repo => repo.GetOrderByIdAsync(1)).ReturnsAsync(order);
            _mockRepo.Setup(repo => repo.DeleteOrderAsync(1)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteOrder(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_ReturnsNotFoundResult()
        {
            _mockRepo.Setup(repo => repo.GetOrderByIdAsync(1)).ReturnsAsync((Order)null);

            var result = await _controller.DeleteOrder(1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
