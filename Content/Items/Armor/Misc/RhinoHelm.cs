using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AncientRealms.Content.Items.Armor.Misc
{
    [AutoloadEquip(EquipType.Head)]
    public class RhinoHelm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
			Item.height = 18;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.defense = 5;
        }
    }
}