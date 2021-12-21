using SQLite;

namespace KOF.Models
{
    [Table("skillbar")]
    public class SkillBar
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("user")]
        public string User { get; set; }
        [Column("skillid")]
        public int SkillId { get; set; }
        [Column("skilltype")]
        public int SkillType { get; set; }
        [Column("usetime")]
        public int UseTime { get; set; }
        [Column("platform")]
        public string Platform { get; set; }
    }
}
