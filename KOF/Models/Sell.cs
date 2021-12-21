using SQLite;

namespace KOF.Models
{
    [Table("sell")]
    public class Sell
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("user")]
        public string User { get; set; }

        [Column("itemid")]
        public int ItemId { get; set; }

        [Column("itemname")]
        public string ItemName { get; set; }

        [Column("platform")]
        public string Platform { get; set; }
    }
}
