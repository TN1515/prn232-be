namespace Domain.Payload.Response.BankSettings
{
    public class GetBanksResponse
    {
        public Guid ID { get; set; }
        public string BankName { get; set; }
        public string BankNo { get; set; }
        public bool IsUse { get; set; }
    }
}
