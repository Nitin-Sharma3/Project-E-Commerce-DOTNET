using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using UserDashboardMVC.ViewModels;

namespace UserDashboardMVC.Controllers
{
    public class AddressController : Controller
    {
        private readonly HttpClient _addressClient;
        private readonly HttpClient _postalClient;
        private readonly ILogger<AddressController> _logger;
        private const int CurrentUserId = 1; // replace with session/auth later

        public AddressController(IHttpClientFactory factory, ILogger<AddressController> logger)
        {
            _addressClient = factory.CreateClient("AddressAPI");
            _postalClient = factory.CreateClient("PostalAPI");
            _logger = logger;
        }

        // ── Helper: load existing addresses into a model ──────
        private async Task<AddressPageViewModel> BuildBaseModel()
        {
            var model = new AddressPageViewModel { UserId = CurrentUserId };
            try
            {
                var resp = await _addressClient.GetAsync($"api/users/{CurrentUserId}/addresses");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    model.Addresses = JsonSerializer.Deserialize<List<AddressViewModel>>(json, options) ?? new();
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to load addresses"); }
            return model;
        }

        // ── GET /Address ──────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var model = await BuildBaseModel();
            return View(model);
        }

        // ── GET /Address/AddAddress ───────────────────────────
        // FIX: Use TempData["ShowAddForm"] so the view picks it up correctly.
        [HttpGet]
        public async Task<IActionResult> AddAddress()
        {
            var model = await BuildBaseModel();
            model.NewAddress = new AddressViewModel();
            TempData["ShowAddForm"] = "true";   // <-- view reads this
            return View("Index", model);
        }

        // ── POST /Address/Add ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddressPageViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewAddress.FullName) ||
                string.IsNullOrWhiteSpace(model.NewAddress.Phone) ||
                string.IsNullOrWhiteSpace(model.NewAddress.AddressLine1) ||
                string.IsNullOrWhiteSpace(model.NewAddress.PostalCode))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var payload = new
                {
                    fullName = model.NewAddress.FullName,
                    phone = model.NewAddress.Phone,
                    addressLine1 = model.NewAddress.AddressLine1,
                    addressLine2 = model.NewAddress.AddressLine2,
                    city = model.NewAddress.City,
                    state = model.NewAddress.State,
                    postalCode = model.NewAddress.PostalCode,
                    country = string.IsNullOrWhiteSpace(model.NewAddress.Country) ? "India" : model.NewAddress.Country,
                    isPrimary = model.NewAddress.IsPrimary
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _addressClient.PostAsync($"api/users/{CurrentUserId}/addresses", content);

                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode
                        ? "Address added successfully."
                        : "Failed to add address. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add address");
                TempData["Error"] = "Something went wrong. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Address/EditAddress/{id} ─────────────────────
        [HttpGet]
        public async Task<IActionResult> EditAddress(int id)
        {
            var model = await BuildBaseModel();
            var target = model.Addresses.FirstOrDefault(a => a.Id == id);
            if (target == null)
            {
                TempData["Error"] = "Address not found.";
                return RedirectToAction(nameof(Index));
            }

            model.EditAddressId = id;
            model.EditAddress = target;
            return View("Index", model);
        }

        // ── POST /Address/EditAddress/{id} ────────────────────
        // FIX: Bind EditAddress explicitly to avoid null binding issues.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(int id, [Bind(Prefix = "EditAddress")] AddressViewModel editAddress)
        {
            if (editAddress == null ||
                string.IsNullOrWhiteSpace(editAddress.FullName) ||
                string.IsNullOrWhiteSpace(editAddress.Phone) ||
                string.IsNullOrWhiteSpace(editAddress.AddressLine1) ||
                string.IsNullOrWhiteSpace(editAddress.PostalCode))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(EditAddress), new { id });
            }

            try
            {
                var payload = new
                {
                    id = id,
                    fullName = editAddress.FullName,
                    phone = editAddress.Phone,
                    addressLine1 = editAddress.AddressLine1,
                    addressLine2 = editAddress.AddressLine2,
                    city = editAddress.City,
                    state = editAddress.State,
                    postalCode = editAddress.PostalCode,
                    country = string.IsNullOrWhiteSpace(editAddress.Country) ? "India" : editAddress.Country,
                    isPrimary = editAddress.IsPrimary
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _addressClient.PutAsync($"api/users/{CurrentUserId}/addresses/{id}", content);

                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode
                        ? "Address updated successfully."
                        : "Failed to update address. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update address {Id}", id);
                TempData["Error"] = "Something went wrong. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Address/Delete/{id} ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _addressClient.DeleteAsync($"api/users/{CurrentUserId}/addresses/{id}");
                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode ? "Address removed." : "Failed to remove address.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete address {Id}", id);
                TempData["Error"] = "Something went wrong.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Address/SetPrimary/{id} ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimary(int id)
        {
            try
            {
                var response = await _addressClient.PatchAsync(
                    $"api/users/{CurrentUserId}/addresses/{id}/set-primary", null);
                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode ? "Primary address updated." : "Failed to set primary address.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set primary {Id}", id);
                TempData["Error"] = "Something went wrong.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Address/LookupPostal ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> LookupPostal(
            string code,
            string? returnFullName,
            string? returnPhone,
            string? returnLine1,
            string? returnLine2,
            bool returnIsPrimary = false,
            int? editId = null)
        {
            var model = await BuildBaseModel();

            var addr = new AddressViewModel
            {
                FullName = returnFullName ?? "",
                Phone = returnPhone ?? "",
                AddressLine1 = returnLine1 ?? "",
                AddressLine2 = returnLine2,
                PostalCode = code,
                IsPrimary = returnIsPrimary
            };

            if (editId.HasValue)
            {
                addr.Id = editId.Value;
                model.EditAddressId = editId;
                model.EditAddress = addr;
            }
            else
            {
                model.NewAddress = addr;
            }

            if (!string.IsNullOrWhiteSpace(code) && code.Length == 6)
            {
                try
                {
                    var resp = await _postalClient.GetAsync($"https://api.postalpincode.in/pincode/{code}");
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var data = JsonSerializer.Deserialize<List<PincodeApiResponse>>(json, options);
                        var first = data?.FirstOrDefault();

                        if (first?.Status == "Success" && first.PostOffice?.Count > 0)
                        {
                            var po = first.PostOffice[0];
                            if (editId.HasValue && model.EditAddress != null)
                            {
                                model.EditAddress.City = po.District ?? "";
                                model.EditAddress.State = po.State ?? "";
                                model.EditAddress.Country = "India";
                            }
                            else
                            {
                                model.NewAddress.City = po.District ?? "";
                                model.NewAddress.State = po.State ?? "";
                                model.NewAddress.Country = "India";
                            }
                            TempData["PostalSuccess"] = $"Found: {po.District}, {po.State}";
                        }
                        else
                        {
                            TempData["PostalError"] = "Pincode not found. Please enter city and state manually.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Postal lookup failed for {Code}", code);
                    TempData["PostalError"] = "Could not look up pincode. Please enter city and state manually.";
                }
            }

            return View("Index", model);
        }

        // ── Internal API response models ──────────────────────
        private class PincodeApiResponse
        {
            public string? Status { get; set; }
            public List<PostOfficeEntry>? PostOffice { get; set; }
        }
        private class PostOfficeEntry
        {
            public string? Name { get; set; }
            public string? District { get; set; }
            public string? State { get; set; }
            public string? Country { get; set; }
            public string? Pincode { get; set; }
        }
    }
}