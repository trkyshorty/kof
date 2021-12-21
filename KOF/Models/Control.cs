using SQLite;

namespace KOF.Models
{
    [Table("control")]
    public class Control
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("form")]
        public string Form { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("platform")]
        public string Platform { get; set; }
    }
}