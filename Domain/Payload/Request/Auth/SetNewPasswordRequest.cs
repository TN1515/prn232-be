namespace Domain.Payload.Request.Auth
{
    public class SetNewPasswordRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
