﻿using System.Collections.Generic;
using KOF.Models;
using KOF.Core;

namespace KOF.Common
{
    class Storage
    {
        public static Client FollowedClient { get; set; }
        public static bool AutoPartyAccept { get; set; }
        public static Dictionary<int, Client> ClientCollection { get; set; } = new Dictionary<int, Client>();
        public static Dictionary<AddressEnum.Platform, List<AddressStorage>> AddressCollection { get; set; } = new Dictionary<AddressEnum.Platform, List<AddressStorage>>();
        public static Dictionary<string, List<Control>> ControlCollection { get; set; } = new Dictionary<string, List<Control>>();
        public static Dictionary<string, List<Skill>> SkillCollection { get; set; } = new Dictionary<string, List<Skill>>();
        public static Dictionary<string, List<SkillBar>> SkillBarCollection { get; set; } = new Dictionary<string, List<SkillBar>>();
        public static List<Item> ItemCollection { get; set; } = new List<Item>();
        public static List<Npc> NpcCollection { get; set; } = new List<Npc>();
        public static List<(string Type, string Control, string ControlCount, string ControlItem, string ItemConst)> SupplyCollection { get; set; } =
            new List<(string, string, string, string, string)>()
            {
                ("Potion", "SupplyHpPotion", "SupplyHpPotionCount", "SupplyHpPotionItem", null),
                ("Potion", "SupplyMpPotion", "SupplyMpPotionCount", "SupplyMpPotionItem", null),

                ("Sunderies", "SupplyArrow", "SupplyArrowCount", null, "Arrow"),
                ("Sunderies", "SupplyWolf", "SupplyWolfCount", null, "Blood of wolf"),
                ("Sunderies", "SupplyTsGem", "SupplyTsGemCount", null, "Transformation Gem"),
                ("Sunderies", "SupplyBook", "SupplyBookCount", null, "Prayer of god’s power"),
                ("Sunderies", "SupplyMasterStone", "SupplyMasterStoneCount", "SupplyMasterStoneItem", null),

                ("Inn", "SupplyInnHpPotion", "SupplyInnHpPotionCount", "SupplyInnHpPotionItem", null),
                ("Inn", "SupplyInnMpPotion", "SupplyInnMpPotionCount", "SupplyInnMpPotionItem", null),
                ("Inn", "SupplyInnArrow", "SupplyInnArrowCount", null, "Arrow"),
                ("Inn", "SupplyInnWolf", "SupplyInnWolfCount", null, "Blood of wolf"),
                ("Inn", "SupplyInnTsGem", "SupplyInnTsGemCount", null, "Transformation Gem"),
                ("Inn", "SupplyInnBook", "SupplyInnBookCount", null, "Prayer Of God's Power"),
                ("Inn", "SupplyInnMasterStone", "SupplyInnMasterStoneCount", "SupplyInnMasterStoneItem", null),
            };

        public static Dictionary<string, List<Sell>> SellCollection { get; set; } = new Dictionary<string, List<Sell>>();
        public static Dictionary<string, List<Loot>> LootCollection { get; set; } = new Dictionary<string, List<Loot>>();
        public static Dictionary<string, List<Target>> TargetCollection { get; set; } = new Dictionary<string, List<Target>>();

        public static List<int> MiningTrashItemCollection { get; set; } =
                    new List<int>()
                    {
                        389010000,
                        389011000,
                        389012000,
                        389013000,
                        389016000,
                        389017000,
                        389018000,
                        389019000,
                        389043000,
                        389050000
                    };
    }

    
}
