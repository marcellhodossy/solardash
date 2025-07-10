using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Npgsql;
using SolarDash.Config;
using SolarDash.Models;

namespace SolarDash.Controllers
{
    [ApiController]
    public class ForgotMailController : ControllerBase
    {
        private readonly PostgreSQL _database;
        private readonly authentication _authentication;

        public ForgotMailController(PostgreSQL database, authentication authentication)
        {
            _database = database;
            _authentication = authentication;
        }

        [HttpPost("/api/forgot/send")]
        public async Task<IActionResult> ForgotMail([FromBody] ForgotMailRequest request)
        {
            if (request.Email != null)
            {
                var conn = await _database.GetOpenConnectionAsync();

                await using (
                    var data = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", conn)
                )
                {
                    data.Parameters.AddWithValue("email", request.Email);

                    var reader = await data.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        var MailHelper = new Mail();
                        var client = MailHelper.CreateSMTPClient();

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("SolarDash", "USERNAME"));
                        message.To.Add(new MailboxAddress("", request.Email));
                        message.Subject = "Forgotten Password";
                        var token = authentication.GenerateJWT(
                            reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetInt32(reader.GetOrdinal("token_version"))
                        );
                        message.Body = new TextPart("plain") { Text = $"Your link {token}" };
                        client.Send(message);
                        client.Disconnect(true);
                        return Ok(new {status = 1});
                    }
                    else
                    {
                        return Ok(new { error = 15 });
                    }
                }
            }
            else
            {
                return Ok(new { error = 14 });
            }

            return BadRequest("SERVER ERROR");
        }
    }
}
