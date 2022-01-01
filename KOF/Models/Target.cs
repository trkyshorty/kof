using SQLite;

namespace KOF.Models
{
    [Table("target")]
    public class Target
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("user")]
        public string User { get; set; }
        [Column("platform")]
        public string Platform { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("checked")]
        public int Checked { get; set; }
    }
}
