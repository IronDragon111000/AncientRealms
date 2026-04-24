using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using Terraria.Graphics;
using Humanizer;

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public sealed partial class EldritchVoid : ModNPC
    {
        // Attack Damage for Phase 1 attacks
        // Attack Damage for Phase 2 attacks
        // Attack Damage for Phase 3 attacks
		public int finalLaserDamage = 50;

		// Telegraph time for Phase 1 attacks
		// Telegraph time for Phase 2 attacks
		// Telegraph time for Phase 3 attacks
		public int finalLaserTelegraphTime = 120;
        public void ResetAttack()
		{
			AttackTimer = 0;
			NPC.netUpdate = true;
		}

        private void RandomizeTarget(out Player target)
		{
			target = null;
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			var Players = new List<int>();

			foreach (Player Player in Main.player.Where(n => n.active && !n.dead))
			{
				Players.Add(Player.whoAmI);
			}

			int random = Main.rand.Next(Players.Count);

			if (random < Players.Count)
				target = Main.player.Where(n => n.active && !n.dead).ToList()[Players[random]];

			NPC.netUpdate = true;
		}

		private void finalLaser()
		{ 
			if(AttackTimer == 1)
			{
				RandomizeTarget(out Player target);
				Vector2 direction = Vector2.Normalize(target.Center - NPC.Center);
				Vector2 velocity = direction;

				Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<EldritchVoidLaserSource>(), finalLaserDamage, 0f, -1, AttackTimer, finalLaserTelegraphTime, target.whoAmI);
			}
			if(AttackTimer > 150)
			{
				Main.projectile.Where(p => p.active && p.ModProjectile is EldritchVoidLaserSource).ToList().ForEach(p => p.Kill());
				ResetAttack();
			}
		}
    }
}