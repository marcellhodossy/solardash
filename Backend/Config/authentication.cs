using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SolarDash.Models;

namespace SolarDash.Config
{
    public class authentication
    {
        private const string secretkey =
            "secret_key_here";

        private readonly PostgreSQL _database;

        public authentication(PostgreSQL database)
        {
            _database = database;
        }

        public static string GenerateJWT(int id, int token_version)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var claims = new[]
            {
                new Claim("id", id.ToString()),
                new Claim("token_version", token_version.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: "SolarDash",
                audience: "Client",
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<TokenModels?> ValidateJWT(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretkey);

            var validationparameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "SolarDash",
                ValidateAudience = true,
                ValidAudience = "Client",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            try
            {
                var principal = tokenHandler.ValidateToken(
                    token,
                    validationparameters,
                    out SecurityToken validatedToken
                );

                var conn = await _database.GetOpenConnectionAsync();

                await using (
                    var results = new NpgsqlCommand(
                        "SELECT * FROM users WHERE id = @id AND token_version = @token_version",
                        conn
                    )
                )
                {
                    results.Parameters.AddWithValue(
                        "id",
                        Convert.ToInt32(principal.FindFirst("id")?.Value)
                    );
                    results.Parameters.AddWithValue(
                        "token_version",
                        Convert.ToInt32(principal.FindFirst("token_version")?.Value)
                    );

                    var reader = await results.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        return (
                            new TokenModels
                            {
                                id = Convert.ToInt32(principal.FindFirst("id")?.Value),
                                token_version = reader.GetInt32(reader.GetOrdinal("token_version")),
                                username = reader.GetString(reader.GetOrdinal("username")),
                            }
                        );
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception Ex)
            {
                return null;
            }
        }
    }
}
