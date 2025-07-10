using MailKit.Net.Smtp;
using MailKit.Security;

namespace SolarDash.Config
{
    public class Mail
    {
        public SmtpClient CreateSMTPClient()
        {
            var client = new SmtpClient();

            client.Connect("HOST", 465, SecureSocketOptions.SslOnConnect);
            client.Authenticate("USERNAME", "PASSWORD");

            return client;
        }
    }
}