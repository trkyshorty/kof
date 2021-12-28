using SQLite;

namespace KOF.Models
{
    [Table("item")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("buypacketcountsize")]
        public int BuyPacketCountSize { get; set; }

        [Column("buypacketend")]
        public string BuyPacketEnd { get; set; }

        [Column("warehousetype")]
        public int WarehouseType { get; set; }

        [Column("castskill")]
        public int CastSkill { get; set; }
    }
}
