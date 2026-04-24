using DeliveryMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryMVC.Controllers
{
    //public class DeliveryController(DeliveryApiService api) : Controller
    //{
    //    public async Task<IActionResult> Index(string? status)
    //    {
    //        var all = await api.GetAllAsync();
    //        if (!string.IsNullOrEmpty(status))
    //            all = all.Where(d => d.Status == status).ToList();
    //        ViewBag.StatusFilter = status ?? "All";
    //        return View(all);
    //    }

    //    public async Task<IActionResult> Details(int id)
    //    {
    //        var d = await api.GetByIdAsync(id);
    //        return d == null ? NotFound() : View(d);
    //    }

    //    public async Task<IActionResult> Tracking(string trackingId)
    //    {
    //        var t = await api.GetTrackingAsync(trackingId);
    //        return t == null ? NotFound() : View(t);
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> UpdateStatus(int deliveryId, string status)
    //    {
    //        var ok = await api.UpdateStatusAsync(deliveryId, status);
    //        TempData[ok ? "Success" : "Error"] = ok
    //            ? $"Status updated to {status}"
    //            : "Invalid status transition. Check allowed flow.";
    //        return RedirectToAction(nameof(Details), new { id = deliveryId });
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> MarkDelivered(int deliveryId)
    //    {
    //        var ok = await api.MarkDeliveredAsync(deliveryId);
    //        TempData[ok ? "Success" : "Error"] = ok ? "Marked as Delivered!" : "Could not mark as delivered.";
    //        return RedirectToAction(nameof(Details), new { id = deliveryId });
    //    }
    //}

    public class DeliveryController(DeliveryApiService api) : Controller
    {
        public async Task<IActionResult> Index(string? status)
        {
            var all = await api.GetAllAsync();
            if (!string.IsNullOrEmpty(status) && status != "All")
                all = all.Where(d => d.Status == status).ToList();
            ViewBag.StatusFilter = status ?? "All";
            return View(all);
        }

        public async Task<IActionResult> Details(int id)
        {
            var d = await api.GetByIdAsync(id);
            if (d == null)
            {
                TempData["Error"] = $"Delivery #{id} not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(d);
        }

        public async Task<IActionResult> Tracking(string trackingId)
        {
            if (string.IsNullOrEmpty(trackingId))
                return RedirectToAction(nameof(Index));

            var t = await api.GetTrackingAsync(trackingId);
            if (t == null)
            {
                TempData["Error"] = $"Tracking ID '{trackingId}' not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(t);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int deliveryId, string status)
        {
            var ok = await api.UpdateStatusAsync(deliveryId, status);
            TempData[ok ? "Success" : "Error"] = ok
                ? $"Status updated to {status}"
                : "Invalid status transition. Check allowed flow.";
            return RedirectToAction(nameof(Details), new { id = deliveryId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkDelivered(int deliveryId)
        {
            var ok = await api.MarkDeliveredAsync(deliveryId);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Marked as Delivered!"
                : "Could not mark as delivered. Must be OutForDelivery first.";
            return RedirectToAction(nameof(Details), new { id = deliveryId });
        }
        public async Task<IActionResult> LiveMap()
        {
            var points = await api.GetAllForMapAsync();
            return View(points);
        }
    }
}
