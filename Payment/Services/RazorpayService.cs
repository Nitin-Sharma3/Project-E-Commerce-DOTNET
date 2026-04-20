using Microsoft.Extensions.Options;
using Razorpay.Api;
using RazorpayApi.Models;

namespace RazorpayApi.Services;

public class RazorpayService : IRazorpayService
{
    private readonly RazorpayClient _client;
    private readonly ILogger<RazorpayService> _logger;

    public RazorpayService(IOptions<RazorpaySettings> settings, ILogger<RazorpayService> logger)
    {
        var s = settings.Value;
        _client = new RazorpayClient(s.Key, s.Secret);
        _logger = logger;
    }

    /// <summary>
    /// Extracts the note text from refund notes field (handles both string and object formats)
    /// </summary>
    private string? GetNotesFromRefund(Razorpay.Api.Refund refund)
    {
        try
        {
            var notesValue = refund["notes"];
            if (notesValue == null)
                return null;

            // If notes is a dictionary/object, try to extract the "note" key
            if (notesValue is Dictionary<string, object> noteDict && noteDict.ContainsKey("note"))
            {
                return noteDict["note"]?.ToString();
            }

            // If notes is already a string, return it
            var notesString = notesValue.ToString();
            if (!string.IsNullOrEmpty(notesString) && notesString != "{}")
            {
                // If it's a JSON-like string, try to parse it
                if (notesString.Contains("\"note\""))
                {
                    // Try to extract note value from JSON string like { "note": "refund1" }
                    var noteStart = notesString.IndexOf("\"note\"") + 7;
                    var noteEnd = notesString.IndexOf("\"", noteStart + 3);
                    if (noteStart > 6 && noteEnd > noteStart)
                    {
                        return notesString.Substring(noteStart + 3, noteEnd - noteStart - 3);
                    }
                }
                return notesString;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse notes from refund");
            return null;
        }
    }

    /// <summary>
    /// Converts Unix timestamp from Razorpay API to DateTime in local timezone
    /// </summary>
    private DateTime? GetCreatedAtFromRefund(Razorpay.Api.Refund refund)
    {
        try
        {
            var createdAtValue = refund["created_at"]?.ToString();
            if (string.IsNullOrEmpty(createdAtValue))
            {
                return null;
            }

            // Razorpay returns created_at as Unix timestamp (seconds since epoch)
            if (long.TryParse(createdAtValue, out long unixTimestamp))
            {
                // Convert Unix timestamp to DateTime in local timezone
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(unixTimestamp).ToLocalTime();
                return dateTime;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse created_at from refund response");
            return null;
        }
    }

    public CreateOrderResponse CreateOrder(CreateOrderRequest request)
    {
        var input = new Dictionary<string, object>
        {
            { "amount", request.Amount },
            { "currency", request.Currency },
            { "receipt", request.Receipt },
            { "payment_capture", 1 }
        };
        Order order = _client.Order.Create(input);
        return new CreateOrderResponse
        {
            OrderId = order["id"].ToString()!,
            Amount = request.Amount,
            Currency = request.Currency,
            Receipt = request.Receipt
        };
    }

    public ChargeResponse VerifyAndCharge(ChargeRequest request)
    {
        var attributes = new Dictionary<string, string>
        {
            { "razorpay_payment_id", request.RazorpayPaymentId },
            { "razorpay_order_id",   request.RazorpayOrderId },
            { "razorpay_signature",  request.RazorpaySignature }
        };

        Utils.verifyPaymentSignature(attributes);
        return new ChargeResponse
        {
            Success = true,
            PaymentId = request.RazorpayPaymentId,
            Message = "Payment verified successfully."
        };
    }

    /// <summary>
    /// Initiates a full refund for a payment
    /// </summary>
    public RefundResponse Refund(string paymentId)
    {
        try
        {
            Razorpay.Api.Refund refund = new Payment(paymentId).Refund();
            var refundId = refund["id"]?.ToString();
            var createdAt = GetCreatedAtFromRefund(refund) ?? DateTime.Now;

            _logger?.LogInformation($"Full refund initiated for payment {paymentId}, refund {refundId}, created at {createdAt}");

            return new RefundResponse
            {
                Success = true,
                RefundId = refundId,
                PaymentId = paymentId,
                Amount = refund["amount"]?.ToString() == null ? null : int.Parse(refund["amount"].ToString()),
                Status = refund["status"]?.ToString(),
                Speed = "normal",
                Message = "Full refund initiated successfully.",
                CreatedAt = createdAt
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Full refund failed for payment {paymentId}");
            return new RefundResponse
            {
                Success = false,
                PaymentId = paymentId,
                ErrorMessage = ex.Message,
                Message = "Full refund failed."
            };
        }
    }

    /// <summary>
    /// Initiates a partial refund for a payment
    /// </summary>
    public RefundResponse RefundPartial(string paymentId, int amount, string? notes = null)
    {
        try
        {
            var refundParams = new Dictionary<string, object>
            {
                { "amount", amount }
            };

            // Add notes as a dictionary if provided and not empty
            // Razorpay API requires notes to be a dictionary/object
            if (!string.IsNullOrWhiteSpace(notes))
            {
                refundParams["notes"] = new Dictionary<string, string>
                {
                    { "note", notes }
                };
            }

            Razorpay.Api.Refund refund = new Payment(paymentId).Refund(refundParams);
            var refundId = refund["id"]?.ToString();
            var createdAt = GetCreatedAtFromRefund(refund) ?? DateTime.Now;

            _logger?.LogInformation($"Partial refund initiated for payment {paymentId}, amount {amount} paise, refund {refundId}, created at {createdAt}, notes: {notes ?? "N/A"}");

            return new RefundResponse
            {
                Success = true,
                RefundId = refundId,
                PaymentId = paymentId,
                Amount = amount,
                Status = refund["status"]?.ToString(),
                Speed = "normal",
                Notes = notes,
                Message = $"Partial refund of ₹{(amount / 100m):N2} initiated successfully.",
                CreatedAt = createdAt
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Partial refund failed for payment {paymentId}, amount {amount} paise");
            return new RefundResponse
            {
                Success = false,
                PaymentId = paymentId,
                Amount = amount,
                ErrorMessage = ex.Message,
                Message = "Partial refund failed."
            };
        }
    }
    public RefundResponse RefundIdempotent(RefundRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaymentId))
            {
                throw new ArgumentException("PaymentId is required.");
            }

            var refundParams = new Dictionary<string, object>();

            // Add amount if specified (partial refund)
            if (request.Amount.HasValue && request.Amount.Value > 0)
            {
                refundParams["amount"] = request.Amount.Value;
            }

            // Add notes if provided (as a dictionary, as Razorpay API requires)
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                refundParams["notes"] = new Dictionary<string, string>
                {
                    { "note", request.Notes }
                };
            }

            // Add idempotency key for safe retries
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                refundParams["idempotency_key"] = request.IdempotencyKey;
            }

            // Add receipt if provided
            if (!string.IsNullOrWhiteSpace(request.Receipt))
            {
                refundParams["receipt"] = request.Receipt;
            }

            Razorpay.Api.Refund refund = new Payment(request.PaymentId).Refund(refundParams);
            var refundId = refund["id"]?.ToString();
            var createdAt = GetCreatedAtFromRefund(refund) ?? DateTime.Now;

            var amountValue = refund["amount"]?.ToString();
            int? refundAmount = string.IsNullOrEmpty(amountValue) ? null : int.Parse(amountValue);

            var refundType = request.Amount.HasValue ? "Partial" : "Full";
            _logger?.LogInformation($"{refundType} refund initiated for payment {request.PaymentId}, refund {refundId}, created at {createdAt}, idempotency {request.IdempotencyKey ?? "N/A"}");

            return new RefundResponse
            {
                Success = true,
                RefundId = refundId,
                PaymentId = request.PaymentId,
                Amount = refundAmount,
                Status = refund["status"]?.ToString(),
                Speed = request.RefundSpeed,
                Receipt = request.Receipt,
                Notes = request.Notes,
                Message = $"{refundType} refund initiated successfully (Idempotent).",
                CreatedAt = createdAt
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Idempotent refund failed for payment {request.PaymentId}, idempotency {request.IdempotencyKey ?? "N/A"}");
            return new RefundResponse
            {
                Success = false,
                PaymentId = request.PaymentId,
                ErrorMessage = ex.Message,
                Message = "Idempotent refund failed."
            };
        }
    }

    /// <summary>
    /// Retrieves status of a specific refund
    /// </summary>
    public RefundResponse GetRefundStatus(string refundId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refundId))
            {
                throw new ArgumentException("RefundId is required.");
            }

            Razorpay.Api.Refund refund = new Razorpay.Api.Refund().Fetch(refundId);
            var amountValue = refund["amount"]?.ToString();
            int? refundAmount = string.IsNullOrEmpty(amountValue) ? null : int.Parse(amountValue);
            var createdAt = GetCreatedAtFromRefund(refund);
            var notes = GetNotesFromRefund(refund);

            _logger?.LogInformation($"Refund status fetched for {refundId}, status {refund["status"]?.ToString()}, created at {createdAt}");

            return new RefundResponse
            {
                Success = true,
                RefundId = refundId,
                PaymentId = refund["payment_id"]?.ToString(),
                Amount = refundAmount,
                Status = refund["status"]?.ToString(),
                Receipt = refund["receipt"]?.ToString(),
                Notes = notes,
                CreatedAt = createdAt,
                Message = "Refund status retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to fetch refund status for {refundId}");
            return new RefundResponse
            {
                Success = false,
                RefundId = refundId,
                ErrorMessage = ex.Message,
                Message = "Failed to fetch refund status."
            };
        }
    }

    /// <summary>
    /// Retrieves all refunds for a specific payment
    /// </summary>
    public List<RefundResponse> GetPaymentRefunds(string paymentId)
    {
        var refunds = new List<RefundResponse>();

        try
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                throw new ArgumentException("PaymentId is required.");
            }

            // Use higher limit to fetch all refunds (default is 10)
            var queryParams = new Dictionary<string, object>
            {
                { "payment_id", paymentId },
                { "count", 100 }  // Fetch up to 100 refunds
            };

            List<Razorpay.Api.Refund> refundList = new Razorpay.Api.Refund().All(queryParams);

            // Use HashSet to track unique refund IDs and prevent duplicates
            var seenRefundIds = new HashSet<string>();

            foreach (var refund in refundList)
            {
                var refundId = refund["id"]?.ToString();

                // Skip duplicate refunds (same refund ID)
                if (string.IsNullOrEmpty(refundId) || seenRefundIds.Contains(refundId))
                {
                    continue;
                }

                seenRefundIds.Add(refundId);

                var amountValue = refund["amount"]?.ToString();
                int? refundAmount = string.IsNullOrEmpty(amountValue) ? null : int.Parse(amountValue);
                var createdAt = GetCreatedAtFromRefund(refund);
                var notes = GetNotesFromRefund(refund);

                refunds.Add(new RefundResponse
                {
                    Success = true,
                    RefundId = refundId,
                    PaymentId = paymentId,
                    Amount = refundAmount,
                    Status = refund["status"]?.ToString(),
                    Receipt = refund["receipt"]?.ToString(),
                    Notes = notes,
                    CreatedAt = createdAt
                });
            }

            _logger?.LogInformation($"Retrieved {refunds.Count} unique refunds for payment {paymentId} (total API responses: {refundList.Count})");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to fetch refunds for payment {paymentId}");
            refunds.Add(new RefundResponse
            {
                Success = false,
                PaymentId = paymentId,
                ErrorMessage = ex.Message,
                Message = "Failed to fetch refunds."
            });
        }

        return refunds;
    }
}