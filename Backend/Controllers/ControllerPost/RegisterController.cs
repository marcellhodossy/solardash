using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Npgsql;
using SolarDash.Config;
using SolarDash.Models;

namespace SolarDash.Controllers
{
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly PostgreSQL _database;

        public RegisterController(PostgreSQL database)
        {
            _database = database;
        }

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [HttpPost("/api/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request.Username.Length < 8)
            {
                return Ok(new { error = 1 });
            }
            else if (request.Username.Length > 32)
            {
                return Ok(new { error = 2 });
            }
            else if (request.Password.Length < 8)
            {
                return Ok(new { error = 3 });
            }
            else if (request.Password.Length > 32)
            {
                return Ok(new { error = 4 });
            }
            else if (request.Email.Length > 100)
            {
                return Ok(new { error = 5 });
            }
            else if (Regex.IsMatch(request.Email, pattern, RegexOptions.IgnoreCase) == false)
            {
                return Ok(new { error = 6 });
            }
            else
            {
                await using var conn = await _database.GetOpenConnectionAsync();

                await using (
                    var checkparams = new NpgsqlCommand(
                        "SELECT * FROM users WHERE username = @username",
                        conn
                    )
                )
                {
                    checkparams.Parameters.AddWithValue("username", request.Username);
                    var check = await checkparams.ExecuteReaderAsync();
                    if (await check.ReadAsync() == true)
                    {
                        return Ok(new { error = 7 });
                    }
                    await check.DisposeAsync();
                }

                await using (
                    var checkparams = new NpgsqlCommand(
                        "SELECT * FROM users WHERE email = @email",
                        conn
                    )
                )
                {
                    checkparams.Parameters.AddWithValue("email", request.Email);
                    var check = await checkparams.ExecuteReaderAsync();
                    if (await check.ReadAsync() == true)
                    {
                        return Ok(new { error = 8 });
                    }
                    await check.DisposeAsync();
                }

                var salt = Argon2.GenerateSalt();
                var password = Argon2.HashPassword(request.Password, salt);

                await using (
                    var insert = new NpgsqlCommand(
                        "INSERT INTO users (username, email, password, salt) VALUES (@username, @email, @password, @salt) RETURNING id",
                        conn
                    )
                )
                {
                    insert.Parameters.AddWithValue("username", request.Username);
                    insert.Parameters.AddWithValue("email", request.Email);
                    insert.Parameters.AddWithValue("password", password);
                    insert.Parameters.AddWithValue("salt", salt);

                    var reader = await insert.ExecuteReaderAsync();
                    await reader.ReadAsync();

                    var mailHelper = new Mail();
                    using var client = mailHelper.CreateSMTPClient();

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("SolarDash", "USERNAME"));
                    message.To.Add(new MailboxAddress("", request.Email));
                    message.Subject = "Confirm Registration";
                    message.Body = new TextPart("plain")
                    {
                        Text =
                            $"Your confirmation link: {authentication.GenerateJWT(reader.GetInt32(0), 0)}",
                    };

                    client.Send(message);
                    client.Disconnect(true);
                }

                return Ok(new { status = 1 });
            }
        }
    }
}
