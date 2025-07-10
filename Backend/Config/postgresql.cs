using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SolarDash.Config
{

    public class PostgreSQL
    {

    private readonly string _connectionString;

    public PostgreSQL(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<NpgsqlConnection> GetOpenConnectionAsync()
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
    }


}