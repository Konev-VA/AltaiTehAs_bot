using AltaiTehAs_bot.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AltaiTehAs_bot
{
    public class MessageDAL
    {
        private string _connectionString = "Data Source=./dataBase.db;";

        public MessageDAL()
        {
            using (IDbConnection db = new SqliteConnection(_connectionString))
            {
                string query = $@"CREATE TABLE IF NOT EXISTS MESSAGES( 
                                                                         {nameof(MyMessage.MessageId)} INTEGER
                                                                       , {nameof(MyMessage.Text)} TEXT
                                                                       , {nameof(MyMessage.UserId)} INTEGER
                                                                       , {nameof(MyMessage.Date)} TEXT);";
                db.Open();

                db.QueryAsync(query);
            }
        }

        public async Task<MyMessage> GetLastMyMessage(long userId)
        {
            string query = "SELECT * FROM MESSAGES WHERE UserId = @UserId order by Date desc";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                return await db.QueryFirstOrDefaultAsync<MyMessage>(query, new { userId });
        }

        public async Task<MyMessage> GetMyMessageById(int messageId, long userId)
        {
            string query = "SELECT * FROM MESSAGES WHERE MessageId = @MessageId AND UserId = @UserId";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                return await db.QueryFirstOrDefaultAsync<MyMessage>(query, new { @MessageId = messageId, @UserId = userId });
        }

        public async Task CreateMyMessage(MyMessage message)
        {
            string query = "insert into messages (MessageId, Text, UserId, Date) values(@MessageId, @Text, @UserId, @Date)";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, message);
        }

        public async Task UpdateMyMessage(MyMessage message)
        {
            string query = "update messages set Text = @Text WHERE MessageId = @MessageId";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, new { message.Text, message.MessageId });
        }
    }
}
