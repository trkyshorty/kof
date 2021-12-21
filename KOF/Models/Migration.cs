using SQLite;

namespace KOF.Models
{
    [Table("migration")]
    public class Migration
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("file")]
        public string File { get; set; }
    }
}
