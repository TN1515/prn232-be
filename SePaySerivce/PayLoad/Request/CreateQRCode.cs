namespace SePaySerivce.PayLoad.Request
{
    public class CreateQRCode
    {
        public string BankAccount { get; set; } = string.Empty;  // acc
        public string BankCode { get; set; } = string.Empty;     // bank
        public decimal Amount { get; set; }                      // amount
        public string Description { get; set; } = string.Empty;  // des
        public string Template { get; set; } = "compact";        // template (optional)
        public bool Download { get; set; } = false;              // download (optional)


    }
}
