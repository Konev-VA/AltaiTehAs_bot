namespace AltaiTehAs_bot.Models
{
    public class Consultation
    {
        public int Id { get; set; }

        public string Question { get; set; }

        public long UserId { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
