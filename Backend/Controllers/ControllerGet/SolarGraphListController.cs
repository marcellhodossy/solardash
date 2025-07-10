using Microsoft.AspNetCore.Mvc;
using SolarDash.Models;
using SolarDash.Config;
using Npgsql;

namespace SolarDash.Controllers
{
    [ApiController]
    public class SolarGraphListController : ControllerBase
    {
        private readonly PostgreSQL _database;
        private readonly authentication _authentication;

        public SolarGraphListController(PostgreSQL database, authentication authentication)
        {
            _authentication = authentication;
            _database = database;
        }

        [HttpGet("/api/users/solargraph")]
        public async Task<IActionResult> ListSolarGraph()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            string token = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Split(' ')[1];
            }
            else
            {
                return Ok(new { error = 15 });
            }

            var data = await _authentication.ValidateJWT(token);

            if (data == null || data.token_version <= 0)
            {
                return Ok(new { error = 15 });
            }

            var conn = await _database.GetOpenConnectionAsync();

            await using (var results = new NpgsqlCommand("SELECT * FROM solargraph WHERE user_id = @user_id", conn))
            {
                results.Parameters.AddWithValue("user_id", data.id);

                var reader = await results.ExecuteReaderAsync();
                var rows = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    rows.Add(row);
                }

                return Ok(rows);
            }
        }
    }
}
