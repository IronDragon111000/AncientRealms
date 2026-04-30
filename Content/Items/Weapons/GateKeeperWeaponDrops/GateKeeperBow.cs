using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using AncientRealms.Content.Items.Weapons.GateKeeperWeaponDrops;
using Terraria.DataStructures;

namespace AncientRealms.Content.Items.Weapons.GateKeeperWeaponDrops
{
    public class GateKeeperBow : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 24;
            Item.height = 44;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<GateKeeperBowArrow>();
            Item.shootSpeed = 6.5f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GateKeeperBowArrow>(), damage, knockback, player.whoAmI);
            return false; // Return false to prevent the default shooting behavior, as we are manually spawning the projectile.
        }
    }
}