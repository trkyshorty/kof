using SQLite;

namespace KOF.Models
{
    [Table("npc")]
    public class Npc
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("realid")]
        public int RealId { get; set; }
        [Column("zone")]
        public int Zone { get; set; }
        [Column("type")]
        public string Type { get; set; }
        [Column("x")]
        public int X { get; set; }
        [Column("y")]
        public int Y { get; set; }
        [Column("town")]
        public int Town { get; set; }
        [Column("nation")]
        public int Nation { get; set; }
        [Column("platform")]
        public string Platform { get; set; }
    }
}
