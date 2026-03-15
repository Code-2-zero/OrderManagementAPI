using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Services;

namespace OrderManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllByUserAsync(GetUserId());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id, GetUserId());
        if (order == null)
            return NotFound(new { message = "Order not found" });

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var order = await _orderService.CreateAsync(GetUserId(), request);
        if (order == null)
            return BadRequest(new { message = "One or more products not found" });

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await _orderService.UpdateStatusAsync(id, GetUserId(), request);
        if (order == null)
            return BadRequest(new { message = "Order not found or invalid status" });

        return Ok(order);
    }
}