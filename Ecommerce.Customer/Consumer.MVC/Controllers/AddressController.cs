using Microsoft.AspNetCore.Mvc;
  using Consumer.MVC.ViewModel;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Text;
    using System.Text.Json.Serialization;

namespace Consumer.MVC.Controllers
{
  
    public class AddressController : Controller
    {
        private readonly HttpClient _httpClient;

        public AddressController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7064/api/");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("Address");

            if (!response.IsSuccessStatusCode)
                return View(new List<AddressViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AddressViewModel>>(json);

            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAddressViewModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Address", content);

            if (!response.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"Address/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<AddressViewModel>(json);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateAddressViewModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"Address/{id}", content);

            if (!response.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"Address/{id}");
            return RedirectToAction("Index");
        }
    }
}
