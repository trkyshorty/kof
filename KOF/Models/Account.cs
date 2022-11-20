using SQLite;

namespace KOF.Models
{
    [Table("account")]
    public class Account
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("hash")]
        public string Hash { get; set; }

        [Column("path")]
        public string Path { get; set; }

        [Column("platform")]
        public string Platform { get; set; }
    }
}
