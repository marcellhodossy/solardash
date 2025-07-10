using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SolarDash.Models;
using SolarDash.Config;

namespace SolarDash.Controllers {
    
    [ApiController]
    [Route("api/users/solargraph")]
    public class ManageSolarGraphController : ControllerBase {

        private readonly PostgreSQL _database;
        private readonly authentication _authentication;

        public ManageSolarGraphController(PostgreSQL database, authentication authentication) {
            _database = database;
            _authentication = authentication;
        }

        [HttpPost]
        public async Task<IActionResult> ManageSolar([FromBody] SolarGraphRequest request) {

            var authHeader = Request.Headers["Authorization"].ToString();
            string token = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ")) {
                token = authHeader.Split(' ')[1];
            } else {
                return Ok(new { error = 15 });
            }

            var data = await _authentication.ValidateJWT(token);

            if (data == null || data.token_version <= 0) {
                return Ok(new { error = 15 });
            }

            using var conn = await _database.GetOpenConnectionAsync();

            if (request.Action == "Create") {
                
                await using (var insert = new NpgsqlCommand("INSERT INTO solargraph (name, lng, lat, user_id) VALUES (@name, @lng, @lat, @user_id)", conn)) {
                    insert.Parameters.AddWithValue("name", request.Name);
                    insert.Parameters.AddWithValue("lng", request.Longitude);
                    insert.Parameters.AddWithValue("lat", request.Latitude);
                    insert.Parameters.AddWithValue("user_id", data.id);

                    await insert.ExecuteNonQueryAsync();
                    return Ok(new { status = 1 });
                }
            } else if (request.Action == "Update") {

                if (request.Id > 0) {
                    await using (var update = new NpgsqlCommand("UPDATE solargraph SET name = @name, lng = @lng, lat = @lat WHERE id = @id AND user_id = @user_id", conn)) {
                        update.Parameters.AddWithValue("name", request.Name);
                        update.Parameters.AddWithValue("lat", request.Latitude);
                        update.Parameters.AddWithValue("lng", request.Longitude);
                        update.Parameters.AddWithValue("id", request.Id);
                        update.Parameters.AddWithValue("user_id", data.id);

                        await update.ExecuteNonQueryAsync();
                        return Ok(new { status = 1 });
                    }
                } else {
                    return Ok(new { error = 16 });
                }
            } else if(request.Action == "Delete"){
                    await using(var delete = new NpgsqlCommand("DELETE FROM solargraph WHERE id = @id AND user_id = @user_id", conn))
                    {
                        delete.Parameters.AddWithValue("id", request.Id);
                        delete.Parameters.AddWithValue("user_id", data.id);
                        await delete.ExecuteNonQueryAsync();
                        return Ok(new {status = 1});
                    }
            } else {
                return Ok(new { error = 17 });
            }
        }
    }
}
