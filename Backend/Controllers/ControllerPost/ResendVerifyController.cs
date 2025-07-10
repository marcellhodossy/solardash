using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SolarDash.Config;
using SolarDash.Models;
using Npgsql;

namespace SolarDash.Controllers
{
    [ApiController]
    public class ResendVerifyController : ControllerBase
    {
        private readonly PostgreSQL _database;

        public ResendVerifyController(PostgreSQL database)
        {
            _database = database;
        }

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [HttpPost("/api/register/resend")]
        public async Task<IActionResult> RegisterResend([FromBody] ResendVerifyRequest request)
        {

            if (Regex.IsMatch(request.Email, pattern))
            {
                var conn = await _database.GetOpenConnectionAsync();

                await using (
                    var data = new NpgsqlCommand(
                        "SELECT * FROM users WHERE email = @email AND token_version = 0",
                        conn
                    )
                )
                {
                    data.Parameters.AddWithValue("email", request.Email);
                    var reader = await data.ExecuteReaderAsync();
                    if (await reader.ReadAsync()) {
                        var token = authentication.GenerateJWT(reader.GetInt32(reader.GetOrdinal("id")), 0);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("SolarDash", "USERNAME"));
                        message.To.Add(new MailboxAddress("", request.Email));
                        message.Subject = "Confirm Registration";
                        message.Body = new TextPart("plain")
                    {
                        Text =
                            $"Your confirmation link: {token}", 
                    };
                    
                    var mailHelper = new Mail();
                    var MailServer = mailHelper.CreateSMTPClient();
                    MailServer.Send(message);
                    MailServer.Disconnect(true);

                    } else {
                        return Ok(new {error = 11});
                    }
                }
            }
            else
            {
                        return Ok(new { error = 12 });
            }

            return BadRequest("server error");
        }
    }
}
