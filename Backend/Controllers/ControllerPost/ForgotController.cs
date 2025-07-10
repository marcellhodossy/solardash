using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SolarDash.Config;
using SolarDash.Models;

namespace SolarDash.Controllers
{
    [ApiController]
    public class ForgotController : ControllerBase
    {
        private readonly PostgreSQL _database;
        private readonly authentication _authentication;

        public ForgotController(PostgreSQL database, authentication authentication)
        {
            _database = database;
            _authentication = authentication;
        }

        [HttpPost("/api/forgot/change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] ForgotRequest request)
        {
            if (request.Token != null && request.Password != null)
            {
                TokenModels token_data = await this._authentication.ValidateJWT(request.Token);
                if (token_data != null)
                {
                    var salt = Argon2.GenerateSalt();
                    var password = Argon2.HashPassword(request.Password, salt);

                    var conn = await _database.GetOpenConnectionAsync();

                    await using (
                        var data = new NpgsqlCommand(
                            "UPDATE users SET salt = @salt, password = @password, token_version = token_version + 1 WHERE id = @id AND token_version = @tv ",
                            conn
                        )
                    )
                    {
                        data.Parameters.AddWithValue("salt", salt);
                        data.Parameters.AddWithValue("password", password);
                        data.Parameters.AddWithValue("id", token_data.id);
                        data.Parameters.AddWithValue("tv", token_data.token_version);

                        var reader = await data.ExecuteNonQueryAsync();
                        return Ok(new { status = 1 });
                    }
                }
                else
                {
                    return Ok(new { error = 17 });
                }
            }
            else
            {
                return Ok(new { error = 16 });
            }
        }
    }
}
