using AltaiTehAs_bot.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AltaiTehAs_bot.DAL
{
    public class UserDAL
    {
        private string _connectionString = "Data Source=./dataBase.db;";

        public UserDAL()
        {
            using (IDbConnection db = new SqliteConnection(_connectionString))
            {
                string query = $@"CREATE TABLE IF NOT EXISTS USERS( 
                                                                         {nameof(User.UserId)} INTEGER
                                                                       , {nameof(User.Name)} TEXT
                                                                       , {nameof(User.Phone)} TEXT);";
                db.Open();

                db.QueryAsync(query);
            }
        }

        public async Task CreateUser(User user)
        {
            string query = "insert into USERS values(@UserId, @Name, @Phone)";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, user);
        }

        public async Task<User> GetUserById(long userId)
        {
            string query = "select * from USERS WHERE UserId = @UserId";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                return await db.QueryFirstOrDefaultAsync<User>(query, new { @UserId = userId });
        }
    }
}
