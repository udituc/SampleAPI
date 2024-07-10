using Microsoft.AspNetCore.Mvc;
using SampleAPI.Entities;
using SampleAPI.Middleware;
using SampleAPI.Repositories;
using SampleAPI.Requests;

namespace SampleAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet]
        [Route("recentOrders/{daysBefore}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetRecentOrders(int daysBefore=1)
        {
            var fromDate = DateTime.UtcNow.AddDays(-daysBefore).Date;
            var orders = await _orderRepository.GetRecentOrdersAsync(fromDate);
            return Ok(orders);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Order>> CreateOrder(OrderRequest createOrderRequest)
        {
            DateTime orderDate;
            if (!DateTime.TryParseExact(createOrderRequest.OrderDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out orderDate) &&
                !DateTime.TryParseExact(createOrderRequest.OrderDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out orderDate))
            {
                ModelState.AddModelError("OrderDate", "Invalid date format. Expected dd/MM/yyyy or dd-MM-yyyy.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = new Order
            {
                OrderDate = orderDate,
                Description = createOrderRequest.Description,
                CustomerName = createOrderRequest.CustomerName,
                WasOrderInvoiced = createOrderRequest.WasOrderInvoiced,
                WasOrderDeleted = createOrderRequest.WasOrderDeleted
            };

            var createdOrder = await _orderRepository.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrder(int id, OrderRequest updateOrderRequest)
        {
            DateTime orderDate;
            if (!DateTime.TryParseExact(updateOrderRequest.OrderDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out orderDate) &&
                !DateTime.TryParseExact(updateOrderRequest.OrderDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out orderDate))
            {
                ModelState.AddModelError("OrderDate", "Invalid date format. Expected dd/MM/yyyy or dd-MM-yyyy.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = new Order
            {
                Id = id,
                OrderDate = orderDate,
                Description = updateOrderRequest.Description,
                CustomerName = updateOrderRequest.CustomerName,
                WasOrderInvoiced = updateOrderRequest.WasOrderInvoiced,
                WasOrderDeleted = updateOrderRequest.WasOrderDeleted
            };

            var updatedOrder = await _orderRepository.UpdateOrderAsync(order);

            if (updatedOrder == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            await _orderRepository.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
