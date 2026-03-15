using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;

namespace OrderManagementAPI.Services;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderResponse>> GetAllByUserAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Items = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<OrderResponse?> GetByIdAsync(int id, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null) return null;

        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponse
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }

    public async Task<OrderResponse?> CreateAsync(int userId, CreateOrderRequest request)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != request.Items.Count)
            return null;

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            UserId = userId,
            Status = OrderStatus.Pending
        };

        decimal total = 0;
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            var itemTotal = product.Price * item.Quantity;
            total += itemTotal;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                TotalPrice = itemTotal
            });

            product.StockQuantity -= item.Quantity;
        }

        order.TotalAmount = total;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.Id, userId);
    }

    public async Task<OrderResponse?> UpdateStatusAsync(int id, int userId, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null) return null;

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            return null;

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }
}