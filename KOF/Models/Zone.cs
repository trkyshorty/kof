using SQLite;

namespace KOF.Models
{
    [Table("zone")]
    public class Zone
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("scalex")]
        public int ScaleX { get; set; }

        [Column("scaley")]
        public int ScaleY { get; set; }

        [Column("image")]
        public string Image { get; set; }
    }
}
