using Ecommerece.Customer.Address.Data;
using Ecommerece.Customer.Address.DTOs;
using Ecommerece.Customer.Address.Models;
using Ecommerece.Customer.Address.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerece.Customer.Address.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _service;

        public AddressController(IAddressService service)
        {
            _service = service;
        }

        // TEMP: replace with real auth later
        private string GetUserId() => "user1";

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var result = await _service.GetAddresses(GetUserId());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddress(int id)
        {
            var result = await _service.GetAddressById(GetUserId(), id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(CreateAddressDto dto)
        {
            var result = await _service.AddAddress(GetUserId(), dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressDto dto)
        {
            var result = await _service.UpdateAddress(GetUserId(), id, dto);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var success = await _service.DeleteAddress(GetUserId(), id);

            if (!success)
                return NotFound();

            return Ok();
        }
    }
}
