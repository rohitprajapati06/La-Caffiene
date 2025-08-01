using Razorpay.Api;

namespace MyProject.Services
{
    public class PaymentService
    {
        private readonly string _keyId;
        private readonly string _keySecret;

        public PaymentService(IConfiguration configuration)
        {
            _keyId = configuration["Razorpay:KeyId"];
            _keySecret = configuration["Razorpay:KeySecret"];
        }

        public string CreateOrder(decimal amount, string currency, string receipt)
        {
            RazorpayClient client = new RazorpayClient(_keyId, _keySecret);

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amount * 100); // Razorpay expects amount in paise
            options.Add("currency", currency);
            options.Add("receipt", receipt);
            options.Add("payment_capture", 1); // Auto-capture payment

            Order order = client.Order.Create(options);
            return order["id"].ToString();
        }

        public bool VerifyPayment(string paymentId, string orderId, string signature)
        {
            RazorpayClient client = new RazorpayClient(_keyId, _keySecret);

            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", paymentId);
            attributes.Add("razorpay_order_id", orderId);
            attributes.Add("razorpay_signature", signature);

            Utils.verifyPaymentSignature(attributes);
            return true;
        }
    }
}
