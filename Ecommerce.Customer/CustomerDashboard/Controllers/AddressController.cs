using CustomerDashboard.DTOs;
using CustomerDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerDashboard.Controllers
{
    public class AddressController : Controller
    {
        private readonly AddressService _service;

        public AddressController(AddressService service)
        {
            _service = service;
        }

        // 🟢 INDEX
        public async Task<IActionResult> Index()
        {
            var addresses = await _service.GetAddresses();
            return View(addresses);
        }

        // 🟢 SAVE (ADD + UPDATE)
        [HttpPost]
        public async Task<IActionResult> Save(CreateAddressDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", await _service.GetAddresses());
            }

            // 🔥 ADD
            if (dto.Id == null || dto.Id == 0)
            {
                var newId = await _service.AddAddress(dto);

                // ✅ Set primary AFTER insert
                if (dto.IsPrimary)
                {
                    await _service.SetPrimaryAddress(newId);
                }
            }
            else
            {
                // 🔥 UPDATE
                await _service.UpdateAddress(dto.Id.Value, new UpdateAddressDto
                {
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    PostalCode = dto.PostalCode,
                    City = dto.City,
                    State = dto.State,
                    Country = dto.Country,

                    // ✅ FIXED (API expects these)
                    Type = dto.Type,
                    Label = dto.Label,

                    IsPrimary = dto.IsPrimary
                });

                // ✅ Handle primary safely
                if (dto.IsPrimary)
                {
                    await _service.SetPrimaryAddress(dto.Id.Value);
                }
            }

            return RedirectToAction("Index");
        }

        // 🔴 DELETE
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAddress(id);
            return RedirectToAction("Index");
        }

        // 🟡 EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var address = await _service.GetAddress(id);

            var model = new CreateAddressDto
            {
                Id = address.Id,
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                PostalCode = address.PostalCode,
                City = address.City,
                State = address.State,
                Country = address.Country,

                // ✅ FIXED
                Type = address.Type,
                Label = address.Label,

                IsPrimary = address.IsPrimary
            };

            ViewBag.EditData = model;

            return View("Index", await _service.GetAddresses());
        }
    }
}