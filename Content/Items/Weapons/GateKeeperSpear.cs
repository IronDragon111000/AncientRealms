using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace AncientRealms.Content.Items.Weapons
{
    public class GateKeeperSpear : ModItem
    {
        public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true; // This skips use animation-tied sound playback, so that we're able to make it be tied to use time instead in the UseItem() hook.
			ItemID.Sets.Spears[Type] = true; // This allows the game to recognize our new item as a spear.
		}
        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.DamageType = DamageClass.Melee;
            Item.width = 18;
            Item.height = 18;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true; // Important for spears, as the damage is handled by the projectile
            Item.knockBack = 5;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<GateKeeperSpearProjectile>();
            Item.shootSpeed = 2f; // The speed of the spear projectile, which can be adjusted for desired reach and behavior.
            Item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player) {
			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override bool? UseItem(Player player) {
			// Because we're skipping sound playback on use animation start, we have to play it ourselves whenever the item is actually used.
			if (!Main.dedServ && Item.UseSound.HasValue) {
				SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
			}

			return null;
		}
    }
}   