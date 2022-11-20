using SQLite;

namespace KOF.Models
{
    [Table("route")]
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("zone")]
        public int Zone { get; set; }

        [Column("data")]
        public string Data { get; set; }
    }
}
