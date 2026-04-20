using Ecommerce.Customer.WishlistAPI.Data;
using Ecommerce.Customer.WishlistAPI.DTOs;
using Ecommerce.Customer.WishlistAPI.Models;
using Ecommerce.Customer.WishlistAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Customer.WishlistAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly WishlistService _service;

        public WishlistController(WishlistService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWishlist(string userId)
        {
            var result = await _service.GetWishlist(userId);
            return Ok(result);
        }
        [HttpPost("move-to-cart/{productId}")]
        public async Task<IActionResult> MoveToCart(string productId)
        {
            string userId = "user1"; // replace with auth later

            await _service.MoveToCart(userId, productId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] WishlistItemDto dto)
        {
            string userId = "user1"; // replace with auth later

            await _service.AddToWishlist(userId, dto.ProductId);

            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Remove(string productId)
        {
            string userId = "1";

            await _service.RemoveFromWishlist(userId, productId);

            return Ok();
        }
    }
}
