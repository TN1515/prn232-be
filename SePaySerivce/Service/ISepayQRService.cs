using SePaySerivce.PayLoad.Request;
using SePaySerivce.PayLoad.Response;

namespace SePaySerivce.Service
{
    public interface ISepayQRService
    {
        CreateQrResponse GenerateQR(CreateQRCode request);
    }
}
