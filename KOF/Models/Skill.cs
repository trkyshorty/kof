using SQLite;

namespace KOF.Models
{
    [Table("skill")]
    public class Skill
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("realid")]
        public int RealId { get; set; }

        [Column("type")]
        public int Type { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("job")]
        public string Job { get; set; }

        [Column("cooldown")]
        public int Cooldown { get; set; }

        [Column("mana")]
        public int Mana { get; set; }

        [Column("item")]
        public int Item { get; set; }

        [Column("itemcount")]
        public int ItemCount { get; set; }

        [Column("point")]
        public int Point { get; set; }

        [Column("tab")]
        public int Tab { get; set; }

        [Column("listed")]
        public int Listed { get; set; }
    }
}
