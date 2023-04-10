namespace AltaiTehAs_bot.Models
{
    public class Repair
    {
        public int Id { get; set; }

        public string TechType { get; set; }

        public string Description { get; set; }

        public long UserId { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
