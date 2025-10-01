namespace Domain.Payload.Response.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public Guid? userId {  get; set; }

    }
}
