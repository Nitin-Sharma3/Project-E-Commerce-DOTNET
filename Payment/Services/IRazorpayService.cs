using RazorpayApi.Models;

namespace RazorpayApi.Services
{
    public interface IRazorpayService
    {
        CreateOrderResponse CreateOrder(CreateOrderRequest request);
        ChargeResponse VerifyAndCharge(ChargeRequest request);
        RefundResponse Refund(string paymentId);
        RefundResponse RefundPartial(string paymentId, int amount, string? notes = null);
        RefundResponse RefundIdempotent(RefundRequest request);
        RefundResponse GetRefundStatus(string refundId);
        List<RefundResponse> GetPaymentRefunds(string paymentId);

    }
}
