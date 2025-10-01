using System.ComponentModel.DataAnnotations;
#pragma warning disable
namespace Domain.Payload.Request.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Please enter UserName or Email")]
        public string UserNameOrEmail { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        public string Password { get; set; }
    }
}
