using System;
using System.Linq;
using System.Reflection;
using AncientRealms.Content.Tiles.Misc;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using AncientRealms.Content.SubWorlds.SubSpaceHub;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Capture;

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
			Item.defense = 7;
        }

        public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<RhinoArmorDash>().DashArmorEquipped = true;
		}
    }

    public class RhinoArmorDash : ModPlayer
    {
        public const int DashDown = 0;
		public const int DashUp = 1;
		public const int DashRight = 2;
		public const int DashLeft = 3;

        public const int DashCooldown = 35; 
        public const int DashDuration = 30;

        // Initial Velocity of the dash
        public const float DashVelocity = 12f;

        // The direction the player has double tapped.  Defaults to -1 for no dash double tap
		public int DashDir = -1;

        public bool DashArmorEquipped;
		public int DashDelay = 0;
		public int DashTimer = 0;

        public override void ResetEffects() {
			DashArmorEquipped = false; //Reset always so the effect isnt perminant. if player still has it on it will fix fixed before movement

			if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15) {
				DashDir = DashDown;
			}
			else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15) {
				DashDir = DashUp;
			}
			else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15 && Player.doubleTapCardinalTimer[DashLeft] == 0) {
				DashDir = DashRight;
			}
			else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15 && Player.doubleTapCardinalTimer[DashRight] == 0) {
				DashDir = DashLeft;
			}
			else {
				DashDir = -1;
			}
		}

        public override void PreUpdateMovement() {
			if (CanUseDash() && DashDir != -1 && DashDelay == 0) {
				Vector2 newVelocity = Player.velocity;

				switch (DashDir) {
					// Only apply the dash velocity if our current speed in the wanted direction is less than DashVelocity
					case DashUp when Player.velocity.Y > -DashVelocity:
					case DashDown when Player.velocity.Y < DashVelocity: {
							float dashDirection = DashDir == DashDown ? 1 : -1.3f;
							newVelocity.Y = dashDirection * DashVelocity;
							break;
						}
					case DashLeft when Player.velocity.X > -DashVelocity:
					case DashRight when Player.velocity.X < DashVelocity: {
							float dashDirection = DashDir == DashRight ? 1 : -1;
							newVelocity.X = dashDirection * DashVelocity;
							break;
						}
					default:
						return;
				}

				// start our dash
				DashDelay = DashCooldown;
				DashTimer = DashDuration;
				Player.velocity = newVelocity;
			}

			if (DashDelay > 0)
				DashDelay--;

			if (DashTimer > 0) { // dash is active
				Player.eocDash = DashTimer;
				Player.armorEffectDrawShadowEOCShield = true;

				DashTimer--;
			}
		}
        private bool CanUseDash() {
			return DashArmorEquipped
				&& Player.dashType == DashID.None
				&& !Player.setSolar
				&& !Player.mount.Active; 
		}
    }
}