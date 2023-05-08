using AltaiTehAs_bot.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AltaiTehAs_bot
{
    public class ConsultationDAL
    {
        private string _connectionString = "Data Source=./dataBase.db;";
        public ConsultationDAL()
        {
            using (IDbConnection db = new SqliteConnection(_connectionString))
            {
                string query = $@"CREATE TABLE IF NOT EXISTS CONSULTATIONS( 
                                                                         {nameof(Consultation.Id)} INTEGER PRIMARY KEY AUTOINCREMENT
                                                                       , {nameof(Consultation.Question)} TEXT
                                                                       , {nameof(Consultation.UserId)} INTEGER
                                                                       , {nameof(Consultation.CreationDate)} TEXT);";
                db.Open();

                db.QueryAsync(query);
            }
        }

        public async Task<Consultation> GetLastConsultation(long userId)
        {
            string query = "SELECT * FROM CONSULTATIONS WHERE UserId = @UserId order by CreationDate desc";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                return await db.QueryFirstOrDefaultAsync<Consultation>(query, new { @UserId = userId });
        }

        public async Task CreateConsultation(Consultation consultation)
        {
            string query = "insert into consultations (Question, UserId, CreationDate) values(@Question, @UserId, @CreationDate)";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, consultation);
        }

        public async Task UpdateConsultation(Consultation consultation)
        {
            string query = "update consultations set Question = @Question WHERE Id = @Id";

            using (IDbConnection db = new SqliteConnection(_connectionString))
                await db.ExecuteAsync(query, new { consultation.Question, consultation.Id });
        }

        public async Task<IEnumerable<Consultation>> GetConsultations()
        {
            string query = "select * from consultations";

            using IDbConnection db = new SqliteConnection(_connectionString);

            return await db.QueryAsync<Consultation>(query);
        }
    }
}
