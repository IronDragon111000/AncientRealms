using AncientRealms.Content.Tiles;
using Terraria.Enums;
using Terraria.ModLoader;

namespace AncientRealms.Content.Items.Placeable
{
	public class RealmGateway : ModItem
	{
		public override void SetDefaults() {
			// Basically, this a just a shorthand method that will set all default values necessary to place
			// the passed in tile type; in this case, the Example Pylon tile.
			Item.DefaultToPlaceableTile(ModContent.TileType<RealmGatewayTile>());

			// Another shorthand method that will set the rarity and how much the item is worth.
			Item.SetShopValues(ItemRarityColor.Blue1, Terraria.Item.buyPrice(gold: 10));
		}
	}
}