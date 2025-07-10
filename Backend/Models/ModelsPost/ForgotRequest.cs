namespace SolarDash.Models
{
    public class ForgotRequest
    {
        public required string Token {get;set;}
        public required string Password {get;set;}
    }
}