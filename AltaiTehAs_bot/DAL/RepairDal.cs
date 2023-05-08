using AltaiTehAs_bot.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AltaiTehAs_bot
{
    public class RepairDal
    {
        private string _connectionString = "Data Source=./dataBase.db;";

        public RepairDal()
        {
            using (IDbConnection db = new SqliteConnection(_connectionString))
            {
                string query = $@"CREATE TABLE IF NOT EXISTS REPAIRS(                                                                          
                                                                         {nameof(Repair.Id)} INTEGER PRIMARY KEY AUTOINCREMENT
                                                                       , {nameof(Repair.TechType)} TEXT
                                                                       , {nameof(Repair.Description)} TEXT
                                                                       , {nameof(Repair.UserId)} INTEGER
                                                                       , {nameof(Repair.CreationDate)} TEXT);";
                db.Open();

                db.QueryAsync(query);
            }
        }
        public async Task<Repair> GetLastRepair(long userId)
        {
            string query = "SELECT * FROM REPAIRS WHERE UserId = @UserId order by CreationDate desc";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                return await db.QueryFirstOrDefaultAsync<Repair>(query, new { @UserId = userId });
        }

        public async Task CreateRepair(Repair repair)
        {
            string query = "insert into repairs (Description, UserId, CreationDate) values(@Description, @UserId, @CreationDate)";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, repair);
        }

        public async Task UpdateRepair(Repair repair)
        {
            string query = "update repairs set TechType = @TechType, Description = @Description WHERE Id = @Id";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, new { repair.TechType, repair.Description, repair.Id });
        }

        public async Task<IEnumerable<Repair>> GetRepairs()
        {
            string query = "select * from repairs";

            using IDbConnection db = new SqliteConnection(_connectionString);

            return await db.QueryAsync<Repair>(query);
        }
    }
}
