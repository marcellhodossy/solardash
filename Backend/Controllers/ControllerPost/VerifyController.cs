using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SolarDash.Config;
using SolarDash.Models;

namespace SolarDash.Controllers
{
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly PostgreSQL _database;
        private readonly authentication _authentication;

        public VerifyController(PostgreSQL database, authentication authentication)
        {
            _database = database;
            _authentication = authentication;
        }

        [HttpPost("/api/register/verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyRequest request)
        {
            TokenModels user_data = await this._authentication.ValidateJWT(request.Token);

            if (user_data == null)
            {
                return Ok(new { error = 13 });
            }
            else
            {
                var conn = await _database.GetOpenConnectionAsync();

                await using (
                    var update = new NpgsqlCommand(
                        "UPDATE users SET token_version = 1 WHERE id = @id AND token_version = 0",
                        conn
                    )
                )
                {
                    update.Parameters.AddWithValue("id", user_data.id);
                    await update.ExecuteNonQueryAsync();
                    return Ok(new { status = 1 });
                }
            }
        }
    }
}
