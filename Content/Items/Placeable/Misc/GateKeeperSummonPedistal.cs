using AncientRealms.Content.Tiles.Misc;
using Terraria.Enums;
using Terraria.ModLoader;

namespace AncientRealms.Content.Items.Placeable.Misc
{
    public class GateKeeperSummonPedistal : ModItem
    {
        public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<GateKeeperSummonPedistalTile>());
			Item.SetShopValues(ItemRarityColor.Blue1, Terraria.Item.buyPrice(gold: 10));
		}
    }
}