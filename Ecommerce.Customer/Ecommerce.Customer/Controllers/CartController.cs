using Ecommerce.Customer.CartAPI.DTOs;
using Ecommerce.Customer.CartAPI.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string GetUserId() => "user1";

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCart(GetUserId());
        return Ok(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(AddToCartDto dto)
    {
        var result = await _cartService.AddToCart(GetUserId(), dto);
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCart(UpdateCartDto dto)
    {
        var result = await _cartService.UpdateCart(GetUserId(), dto);
        return Ok(result);
    }

    [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> RemoveItem(string productId)
    {
        var result = await _cartService.RemoveItem(GetUserId(), productId);
        return Ok(result);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _cartService.ClearCart(GetUserId());
        return Ok(result);
    }
}