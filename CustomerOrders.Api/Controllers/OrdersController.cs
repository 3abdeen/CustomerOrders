using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerOrder.Infrastructure;
using CustomerOrder.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using CustomerOrder.Infrastructure.Enums;
using Microsoft.OpenApi.Extensions;

namespace CustomerOrders.Api.Controllers
{
    /// <summary>
    /// Order Endpoint
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List of all Orders
        /// </summary>
        /// <returns>Collection of all orders</returns>
        [HttpGet]
        [Route("ListOrders")]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
          if (_context.Orders == null)
          {
                return NotFound();
            }
            return await _context.Orders.Include(x=>x.OrderDetails).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Get Specific order by id
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <returns>Order with spcific Id</returns>
        [HttpGet("GetOrder/{id:Guid}")]
        public async Task<ActionResult<Order>> GetOrder(Guid id)
        {
          if (_context.Orders == null)
          {
                return NotFound();
            }
            var order = await _context.Orders.Include(x => x.OrderDetails).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }
        /// <summary>
        /// Get Specific order by number
        /// </summary>
        /// <param name="no">Order Number</param>
        /// <returns>Order with spcific Number</returns>
        [HttpGet("GetOrder/{no:int}")]
        public async Task<ActionResult<Order>> GetOrder(int no)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.Include(x => x.OrderDetails).AsNoTracking().FirstOrDefaultAsync(x => x.OrderNo == no);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }
        /// <summary>
        /// List of all Orders
        /// </summary>
        /// <param name="customerid">Customer Id : a18be9c0-aa65-4af8-bd17-00bd9344e575</param>
        /// <returns>Collection of all user orders</returns>
        [HttpGet]
        [Route("GetCustomerOrders/{customerid}")]
        public async Task<ActionResult<List<Order>>> GetOrders(string customerid)
        {
            if (string.IsNullOrEmpty(customerid))
            {
                return Problem("Customer Id can not be null !");
            }
            if(await _context.Users.FindAsync(customerid) == null)
            {
                return Problem("Customer not found !");
            }
            if(_context.Orders == null)
            {
                return NotFound();
            }
            return await _context.Orders.Include(x => x.OrderDetails).Where(x=>x.CustomerId==customerid).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Update Order Status
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <param name="status">new Status, should not be smaller than the old one</param>
        /// <returns></returns>
        [HttpPut("UpdateOrderStatus")]
        public async Task<IActionResult> PutOrder(Guid id, OrderStatus status)
        {
            return await UpdateOrder(id, status);
        }

        /// <summary>
        /// Create new order and update stock
        /// </summary>
        /// <param name="CustomerId">Customer Id : a18be9c0-aa65-4af8-bd17-00bd9344e575</param>
        /// <param name="Items">
        /// <para><strong>Use scheme : ProductId:Quantity</strong></para>
        /// <para>Product 1 Id : 1E082A57-C3B7-41B6-AF56-179542087041</para>
        /// <para>Product 2 Id : 1E082A57-C3B7-41B6-AF56-179542087042</para>
        /// <para>Product 3 Id : 1E082A57-C3B7-41B6-AF56-179542087043</para>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateOrder")]
        public async Task<ActionResult<Order>> PostOrder(string CustomerId, Dictionary<Guid,int> Items)
        {
            // Check validations
            if (_context.Orders == null)
            {
                return Problem("Order can not be null !");
            }
            if (string.IsNullOrEmpty(CustomerId))
            {
                return Problem("Customer can not be null !");
            }
            if(await _context.Users.FindAsync(CustomerId)==null)
            {
                return Problem("Customer not found !");
            }
            if(!Items.Any())
            {
                return Problem("Order can not be empty !");
            }
            // If the user already has an order but has not checked it out yet, then we will use it.
            Order order =await _context.Orders.FirstOrDefaultAsync(x=>x.CustomerId==CustomerId && x.Status<OrderStatus.Shipped);
            if (order == null)
            {
                order = new Order()
                {
                    CustomerId = CustomerId,
                    Status = OrderStatus.Pending
                };
                _context.Orders.Add(order);
            }
            foreach(var item in Items)
            {
                Guid pId = item.Key;
                int Quantity = item.Value;
                // Validations
                if(item.Value<=0)
                {
                    return Problem("Quantity must be greater than 0 !");
                }
                Product product = await _context.Products.FindAsync(pId);
                if (product==null)
                {
                    return Problem("Product not found !");
                }
                if(product.Quantity<Quantity)
                {
                    return Problem($"Product {product.Name} quantity not enough !");
                }
                // if the same product we will increase the quantity only, prevent duplicate
                OrderDetails od = await _context.OrderDetails.FirstOrDefaultAsync(x=>x.OrderId==order.Id && x.ProductId==pId);
                if (od == null)
                {
                    od = new OrderDetails()
                    {
                        OrderId = order.Id,
                        ProductId = pId,
                        Name=product.Name,
                        Description=product.Description,
                        Quantity = Quantity
                    };
                    await _context.OrderDetails.AddAsync(od);
                }
                else
                {
                    od.Quantity += Quantity;
                    _context.Entry(od).State = EntityState.Modified;
                }
                // calculate total amount Price*Quantity
                order.TotalAmount += product.Price*item.Value;
                product.Quantity -= Quantity;
                _context.Entry(product).State = EntityState.Modified;

            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        /// <summary>
        /// Cancel the order and updates the stock
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <returns>No Content</returns>
        [HttpDelete("CancelOrder/{id}")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            return await UpdateOrder(id, OrderStatus.Canceled);
        }
        private async Task<IActionResult> UpdateOrder(Guid id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status > status)
            {
                return Problem($"An order status cannot be changed.");
            }
            if (order.Status == OrderStatus.Canceled)
            {
                return Problem("An order cannot be canceled, either it's returned or it's already canceled !");
            }
            if (status == OrderStatus.Canceled)
            {
                await ReturnProducts(order);
            }
            order.Status = status;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        async Task ReturnProducts(Order order)
        {
            var orderDetails = await _context.OrderDetails.Where(x => x.OrderId == order.Id).ToListAsync();
            foreach (var od in order.OrderDetails)
            {
                var p = await _context.Products.FindAsync(od.ProductId);
                if (p != null)
                {
                    p.Quantity += od.Quantity;
                    _context.Entry(p).State = EntityState.Modified;
                }
            }
        }
        private bool OrderExists(Guid id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
    }
}
