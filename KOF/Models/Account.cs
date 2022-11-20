using SQLite;

namespace KOF.Models
{
    [Table("account")]
    public class Account
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("accountid")]
        public string AccountId { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("path")]
        public string Path { get; set; }

        [Column("platform")]
        public string Platform { get; set; }

        [Column("serverid")]
        public int ServerId { get; set; }

        [Column("characterid")]
        public int CharacterId { get; set; }

        [Column("charactername")]
        public string CharacterName { get; set; }
    }
}
