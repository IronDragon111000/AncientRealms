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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Build.Evaluation;

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public sealed partial class EldritchVoid : ModNPC
    {
		public Player targetPlayer;
        // Attack Damage for Phase 1 attacks
		public int teleportAttackDamage = 40;
		public int explodingMinionDamage = 45;
		public int explodingProjectileRainDamage = 50;
        // Attack Damage for Phase 2 attacks
        // Attack Damage for Phase 3 attacks
		public int finalLaserDamage = 60;
		public int finalExplodingProjectileDamage= 50;

		// Telegraph time for Phase 1 attacks
		public int teleportAttackTelegraphTime = 30;
		public int teleportAttackDelay = 80;
		public int explodingProjectileRainTelegraphTime = 150;
		// Telegraph time for Phase 2 attacks
		// Telegraph time for Phase 3 attacks
		public int finalLaserTelegraphTime = 120;
		public int finalExplodingProjectileTelegraphTime = 150;
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

		private void teleportAttack()
		{
			if(AttackTimer > 240)
				ResetAttack();
			if(AttackTimer % teleportAttackDelay == 1)
			{
				RandomizeTarget(out targetPlayer);
				float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
				NPC.Center = targetPlayer.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 500f;
			}
			int projectileCount = 10;
			if(AttackTimer % teleportAttackDelay > teleportAttackTelegraphTime && AttackTimer % teleportAttackDelay < teleportAttackTelegraphTime + projectileCount + 1)
			{
				Vector2 direction = Vector2.Normalize(targetPlayer.Center - NPC.Center).RotatedBy((((((AttackTimer % teleportAttackDelay) - teleportAttackTelegraphTime + ((projectileCount + 1) / 2)) / (projectileCount + 1))) * (MathHelper.Pi / 3)) - (MathHelper.Pi / 3));
				Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + direction * 30f, direction * 0.4f, ModContent.ProjectileType<EldritchVoidTeleportVolleyProjectile>(), teleportAttackDamage, 0.5f);
			}
		}

		private void explodingMinionSummon()
		{
			
		}

		private void explodingProjectileRain()
		{
			
		}

		private void finalLaser()
		{ 
			if(AttackTimer == 1)
			{
				RandomizeTarget(out Player target);
				Vector2 direction = Vector2.Normalize(target.Center - NPC.Center);
				Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, direction, ModContent.ProjectileType<EldritchVoidLaserSource>(), finalLaserDamage, 0f, -1, AttackTimer, finalLaserTelegraphTime, target.whoAmI);
			}
			if(AttackTimer > 150)
			{
				Main.projectile.Where(p => p.active && p.ModProjectile is EldritchVoidLaserSource).ToList().ForEach(p => p.Kill());
				ResetAttack();
			}
		}

		private void finalExplodingProjectiles()
		{
			if(AttackTimer == 1)
			{
				float velocityIncreament = MathHelper.TwoPi/5;
				for(int i = 0; i < 5; i++)
				{
					Vector2 velocity = new Vector2((float)Math.Cos(i * velocityIncreament), (float)Math.Sin(i * velocityIncreament)) * 5f;
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<EldritchVoidExplodingProjectile>(), finalExplodingProjectileDamage, 0 ,0 ,0);
				}
			}
			if(AttackTimer > 300)
				ResetAttack();
		}
    }
}