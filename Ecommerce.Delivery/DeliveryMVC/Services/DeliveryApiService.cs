//using DeliveryMVC.Models;

//namespace DeliveryMVC.Services
//{
//    public class DeliveryApiService(IHttpClientFactory http, ILogger<DeliveryApiService> logger)
//    {
//        private HttpClient Client() => http.CreateClient("DeliveryApi");

//        //public async Task<List<DeliveryViewModel>> GetAllAsync()
//        //{
//        //    try
//        //    {
//        //        var resp = await Client().GetFromJsonAsync<ApiResponse<List<DeliveryViewModel>>>("api/delivery");
//        //        return resp?.Data ?? [];
//        //    }
//        //    catch (Exception ex) { logger.LogError(ex, "GetAll failed"); return []; }
//        //}
//        public async Task<List<DeliveryViewModel>> GetAllAsync()
//        {
//            try
//            {
//                var resp = await Client().GetAsync("api/delivery");
//                if (!resp.IsSuccessStatusCode) return [];
//                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<List<DeliveryViewModel>>>();
//                return result?.Data ?? [];
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "GetAll failed");
//                return [];
//            }
//        }

//        //public async Task<DeliveryViewModel?> GetByIdAsync(int id)
//        //{
//        //    var resp = await Client().GetFromJsonAsync<ApiResponse<DeliveryViewModel>>($"api/delivery/{id}");
//        //    return resp?.Data;
//        //}
//        public async Task<DeliveryViewModel?> GetByIdAsync(int id)
//        {
//            try
//            {
//                var resp = await Client().GetAsync($"api/delivery/{id}");
//                if (!resp.IsSuccessStatusCode) return null;
//                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<DeliveryViewModel>>();
//                return result?.Data;
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "GetById failed for {Id}", id);
//                return null;
//            }
//        }

//        //public async Task<TrackingViewModel?> GetTrackingAsync(string trackingId)
//        //{
//        //    var resp = await Client().GetFromJsonAsync<ApiResponse<TrackingViewModel>>($"api/delivery/tracking/{trackingId}");
//        //    return resp?.Data;
//        //}
//        public async Task<TrackingViewModel?> GetTrackingAsync(string trackingId)
//        {
//            try
//            {
//                var resp = await Client().GetAsync($"api/delivery/tracking/{trackingId}");
//                if (!resp.IsSuccessStatusCode) return null;
//                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<TrackingViewModel>>();
//                return result?.Data;
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "GetTracking failed for {TrackingId}", trackingId);
//                return null;
//            }
//        }

//        public async Task<bool> UpdateStatusAsync(int deliveryId, string status, string updatedBy = "Admin")
//        {
//            var payload = new { DeliveryId = deliveryId, NewStatus = Enum.Parse<DeliveryStatus>(status), UpdatedBy = updatedBy };
//            var resp = await Client().PutAsJsonAsync("api/delivery/status", payload);
//            return resp.IsSuccessStatusCode;
//        }

//        public async Task<bool> MarkDeliveredAsync(int deliveryId)
//        {
//            var resp = await Client().PutAsJsonAsync("api/delivery/mark-delivered", new { DeliveryId = deliveryId });
//            return resp.IsSuccessStatusCode;
//        }
//    }
//}

using DeliveryMVC.Models;
using System.Text.Json;

namespace DeliveryMVC.Services
{
    public class DeliveryApiService(IHttpClientFactory http, ILogger<DeliveryApiService> logger)
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private HttpClient Client() => http.CreateClient("DeliveryApi");

        public async Task<List<DeliveryViewModel>> GetAllAsync()
        {
            try
            {
                var resp = await Client().GetAsync("api/delivery");
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("GetAll returned {Code}", resp.StatusCode);
                    return [];
                }
                var json = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<DeliveryViewModel>>>(json, JsonOptions);
                return result?.Data ?? [];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetAll failed");
                return [];
            }
        }

        public async Task<DeliveryViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var resp = await Client().GetAsync($"api/delivery/{id}");
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("GetById returned {Code} for {Id}", resp.StatusCode, id);
                    return null;
                }
                var json = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<DeliveryViewModel>>(json, JsonOptions);
                return result?.Data;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetById failed for {Id}", id);
                return null;
            }
        }

        public async Task<TrackingViewModel?> GetTrackingAsync(string trackingId)
        {
            try
            {
                logger.LogInformation("Fetching tracking for {TrackingId}", trackingId);

                var resp = await Client().GetAsync($"api/delivery/tracking/{trackingId}");

                logger.LogInformation("Tracking API response: {Code}", resp.StatusCode);

                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("Tracking returned {Code} for {Id}", resp.StatusCode, trackingId);
                    return null;
                }

                var json = await resp.Content.ReadAsStringAsync();
                logger.LogInformation("Tracking JSON: {Json}", json[..Math.Min(json.Length, 200)]);

                var result = JsonSerializer.Deserialize<ApiResponse<TrackingViewModel>>(json, JsonOptions);
                return result?.Data;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetTracking failed for {TrackingId}", trackingId);
                return null;
            }
        }

        public async Task<bool> UpdateStatusAsync(int deliveryId, string status, string updatedBy = "Admin")
        {
            try
            {
                var statusEnum = Enum.Parse<DeliveryStatus>(status);
                var payload = new
                {
                    DeliveryId = deliveryId,
                    NewStatus = (int)statusEnum,
                    UpdatedBy = updatedBy,
                    Remarks = $"Status updated to {status} by Admin",
                    Location = "Admin Dashboard"
                };
                var resp = await Client().PutAsJsonAsync("api/delivery/status", payload);
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    logger.LogWarning("UpdateStatus failed: {Error}", err);
                }
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UpdateStatus failed");
                return false;
            }
        }

        public async Task<bool> MarkDeliveredAsync(int deliveryId)
        {
            try
            {
                var resp = await Client().PutAsJsonAsync("api/delivery/mark-delivered",
                    new { DeliveryId = deliveryId, UpdatedBy = "Admin" });
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MarkDelivered failed");
                return false;
            }
        }
    }
}