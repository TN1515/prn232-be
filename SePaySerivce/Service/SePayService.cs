using System.Web;
using SePaySerivce.PayLoad.Request;
using SePaySerivce.PayLoad.Response;

namespace SePaySerivce.Service
{
    public class SePayService : ISepayQRService
    {
        private const string BaseUrl = "https://qr.sepay.vn/img";
        public CreateQrResponse GenerateQR(CreateQRCode request)
        {
            if (string.IsNullOrWhiteSpace(request.BankAccount))
                throw new ArgumentException("Bank account is required");
            if (string.IsNullOrWhiteSpace(request.BankCode))
                throw new ArgumentException("Bank code is required");
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            var queryParams = new Dictionary<string, string>
            {
                { "acc", request.BankAccount },
                { "bank", request.BankCode },
                { "amount", ((long)request.Amount).ToString() },
                { "des", HttpUtility.UrlEncode(request.Description) }
            };

            if (!string.IsNullOrEmpty(request.Template))
                queryParams.Add("template", request.Template);

            if (request.Download)
                queryParams.Add("download", "1");

            string query = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));
            string qrUrl = $"{BaseUrl}?{query}";

            return new CreateQrResponse
            {
                QrUrl = qrUrl,
                ImageTag = $"<img src='{qrUrl}' alt='QR Code' />"
            };
        }
    }
}
