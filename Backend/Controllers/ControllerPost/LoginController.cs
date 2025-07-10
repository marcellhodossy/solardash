using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Npgsql;
using SolarDash.Config;
using SolarDash.Models;

namespace SolarDash.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly PostgreSQL _database;

        public LoginController(PostgreSQL database)
        {
            _database = database;
        }

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [HttpPost("/api/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            await using var conn = await _database.GetOpenConnectionAsync();

            if (Regex.IsMatch(request.Username, pattern, RegexOptions.IgnoreCase) == false)
            {
                await using (
                    var query = new NpgsqlCommand(
                        "SELECT * FROM users WHERE username = @username",
                        conn
                    )
                )
                {
                    query.Parameters.AddWithValue("username", request.Username);
                    var reader = await query.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        if (reader.GetInt32(reader.GetOrdinal("token_version")) > 0)
                        {
                            string saltHex = reader.GetString(reader.GetOrdinal("salt"));

                            if (saltHex.StartsWith("\\x"))
                                saltHex = saltHex.Substring(2);

                            saltHex = new string(saltHex.Where(c => Uri.IsHexDigit(c)).ToArray());

                            byte[] saltBytes = Convert.FromHexString(saltHex);

                            var passwd = Argon2.HashPassword(request.Password, saltBytes);
                            if (passwd == reader.GetString(reader.GetOrdinal("password")))
                            {
                                return Ok(
                                    new
                                    {
                                        success = 1,
                                        token = authentication.GenerateJWT(
                                            reader.GetInt32(reader.GetOrdinal("id")),
                                            reader.GetInt32(reader.GetOrdinal("token_version"))
                                        ),
                                    }
                                );
                            }
                            else
                            {
                                return Ok(new { error = 9 });
                            }
                        }
                        else
                        {
                            return Ok(
                                new
                                {
                                    error = 10,
                                    email = reader.GetString(reader.GetOrdinal("email")),
                                }
                            );
                        }
                    }
                }
            }
            else
            {
                await using (
                    var query = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", conn)
                )
                {
                    query.Parameters.AddWithValue("email", request.Username);
                    var reader = await query.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        if (reader.GetInt32(reader.GetOrdinal("token_version")) > 0)
                        {
                            string saltHex = reader.GetString(reader.GetOrdinal("salt"));

                            if (saltHex.StartsWith("\\x"))
                                saltHex = saltHex.Substring(2);

                            saltHex = new string(saltHex.Where(c => Uri.IsHexDigit(c)).ToArray());

                            byte[] saltBytes = Convert.FromHexString(saltHex);

                            var passwd = Argon2.HashPassword(request.Password, saltBytes);
                            if (passwd == reader.GetString(reader.GetOrdinal("password")))
                            {
                                return Ok(
                                    new
                                    {
                                        success = 1,
                                        token = authentication.GenerateJWT(
                                            reader.GetInt32(reader.GetOrdinal("id")),
                                            reader.GetInt32(reader.GetOrdinal("token_version"))
                                        ),
                                    }
                                );
                            }
                            else
                            {
                                return Ok(new { error = 9 });
                            }
                        }
                        else
                        {
                            return Ok(
                                new
                                {
                                    error = 10,
                                    email = reader.GetString(reader.GetOrdinal("email")),
                                }
                            );
                        }
                    }
                }
            }
            return Ok(new { error = 9 });
        }
    }
}
