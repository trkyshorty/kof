namespace KOF.Common
{
    public class RouteData
    {
        public enum Event : byte
        {
            START,
            MOVE,
            GATE,
            QUEST,
            TOWN,
            OBJECT,
            SUNDERIES,
            POTION,
            INN,
            MINER,
        }

        public Event Action { get; set; }
        public int Npc { get; set; }
        public string Packet { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
