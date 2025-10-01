using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.BankSettings
{
    public class AddBankRequest
    {
        [Required(ErrorMessage = "Nhập tên ngân hàng thụ hưởng")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Nhập số tài khoản ngân hàng")]
        public string BankNo { get; set; }
    }
}
