using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Entities.OrderAggregate;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class OrderRepository(StoreContext context,IMapper mapper) : IOrderRepository
    {
        public void AddOrder(Order order)
        {
            context.Orders.Add( order );
        }

        public async Task<Order> CreateOrder(Basket basket,string username,ShippingAddress shippingAddress)
        {
            var items = new List<OrderItem>();
            foreach(var item in basket.Items)
            {
                var productItem = await context.Products.FindAsync(item.ProductId);
                var productOrdered = new ProductItemOrdered{
                    ProductId = productItem.Id,
                    Name = productItem.Name,
                    PhotoUrl = productItem.PictureUrl
                };
                var OrderItem = new OrderItem
                {
                    ItemOrdered = productOrdered,
                    Price = productItem.Price,
                    Quantity= item.Quantity,
                };
                items.Add(OrderItem);
                productItem.QuantityInStock -= item.Quantity; 
            }
            var subtotal = items.Sum(x=>x.Price);
            var DeliveryFee = subtotal>1000 ? 0:500;
            var order = new Order{
                BuyerId = username,
                shippingAddress = shippingAddress,
                orderItems = items,
                SubTotal = subtotal,
                DeliveryFee = DeliveryFee,
            }; 
            return order;
        }

        public void DeleteOrder(Order order)
        {
            context.Orders.Remove( order );
        }

        public async Task<OrderDto> GetOrder(string username, int orderId)
        {
            // return await context.Orders
            // .Include(o=>o.orderItems)
            // .ProjectOrderToOrderDto()
            // .Where(x=>x.BuyerId == username && x.Id == orderId)
            // .FirstOrDefaultAsync();

            var query = context.Orders
            .Include(o=>o.orderItems)
            .Where(x=>x.BuyerId == username && x.Id == orderId)
            .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .AsQueryable();
            return await query.FirstOrDefaultAsync();
            
        }

        public async Task<List<OrderDto>> GetOrders(string username)
        {
            // return await context.Orders.
            // Include(x=>x.orderItems)
            // .ProjectOrderToOrderDto()
            // .Where(x=>x.BuyerId == username)
            // .ToListAsync();

            var query = context.Orders
            .Include(o=>o.orderItems)
            .Where(x=>x.BuyerId == username)
            .AsNoTracking()
            .AsQueryable();
            return await query.ProjectTo<OrderDto>(mapper.ConfigurationProvider).ToListAsync();

        }
    }
}