using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AddressAPI.DTOs;
using AddressAPI.Services;
 
namespace AddressAPI.Controllers
{
    [ApiController]
    [Route("api/users/{userId:int}/addresses")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // GET api/users/{userId}/addresses
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AddressResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(int userId)
        {
            var addresses = await _addressService.GetAllAsync(userId);
            return Ok(addresses);
        }

        // GET api/users/{userId}/addresses/{addressId}
        [HttpGet("{addressId:int}")]
        [ProducesResponseType(typeof(AddressResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int userId, int addressId)
        {
            try
            {
                var address = await _addressService.GetByIdAsync(userId, addressId);
                return Ok(address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST api/users/{userId}/addresses
        [HttpPost]
        [ProducesResponseType(typeof(AddressResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add(int userId, [FromBody] CreateAddressDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var address = await _addressService.AddAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { userId, addressId = address.Id }, address);
        }

        // PUT api/users/{userId}/addresses/{addressId}
        [HttpPut("{addressId:int}")]
        [ProducesResponseType(typeof(AddressResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int userId, int addressId, [FromBody] UpdateAddressDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var address = await _addressService.UpdateAsync(userId, addressId, dto);
                return Ok(address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PATCH api/users/{userId}/addresses/{addressId}/set-primary
        [HttpPatch("{addressId:int}/set-primary")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPrimary(int userId, int addressId)
        {
            try
            {
                await _addressService.SetPrimaryAsync(userId, addressId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE api/users/{userId}/addresses/{addressId}
        [HttpDelete("{addressId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int userId, int addressId)
        {
            try
            {
                await _addressService.DeleteAsync(userId, addressId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
