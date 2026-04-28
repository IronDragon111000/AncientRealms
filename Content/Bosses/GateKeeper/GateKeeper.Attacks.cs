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

namespace AncientRealms.Content.Bosses.GateKeeper
{
    public sealed partial class GateKeeper : ModNPC
    {
        // Attack Damage for Phase 1 attacks
        public static int CrystalSmashDamage = 20;
        public static int CrystalSmashProjectileDamage = 10;
        // Attack Damage for Phase 2 attacks
        public static int LaserSpinDamage = 25;
        // Attack Damage for Phase 3 attacks
        public static int LaserConvergeDamage = 25;

        //How long before an attack starts - exists to give players time to setup for next attack
        public static float AttackDelay = 60f;
        // Telegraph Lengths for Phase 1 attacks
        public static float CrystalSmashTelegraphLength = 45f;
        public static float CrystalSmashProjectileTelegraphLength = 20f;
        // TelegraphLengths for Phase 2 attacks
        public static float LaserSpinTelegraphLength = 80f;
        // TelegraphLengths for Phase 3 attacks
        public static float LaserConvergeTelegraphLength = 60f;

        public void ResetAttack()
		{
			AttackTimer = 0;
			NPC.netUpdate = true;
		}

        private void RandomizeTarget()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			var Players = new List<int>();

			foreach (Player Player in Main.player.Where(n => n.active && !n.dead && arena.Contains(n.Center.ToPoint())))
			{
				Players.Add(Player.whoAmI);
			}

			int random = Main.rand.Next(Players.Count);

			if (random < Players.Count)
				NPC.target = Players[random];

			NPC.netUpdate = true;
		}

        //Phase 1 Attacks
        private void CrystalSmash()
        {
            if(AttackTimer < AttackDelay){
                for(int i= 0; i < Crystals.Count; i++)
                {
                    if(Crystals[i] != null && Crystals[i].NPC.active)
                    {
                        Vector2 endPos = arena.Center.ToVector2() + new Vector2(0, arenaHeight/2).RotatedBy(MathHelper.ToRadians(360/Crystals.Count * i));
                        
                        Crystals[i].NPC.velocity = Vector2.Normalize(endPos - Crystals[i].NPC.Center) * 1f;
                    }
                }
            } else {
                for(int i= 0; i < Crystals.Count; i++)
                {
                    if(Crystals[i] != null && Crystals[i].NPC.active)
                    {
                        if(AttackTimer < AttackDelay + CrystalSmashTelegraphLength * (i + 1))
                        {
                            Crystals[i].NPC.rotation += 0.1f;
                        } else if (AttackTimer == AttackDelay + CrystalSmashTelegraphLength * (i + 1))
                        {
                            Crystals[i].SmashAttack();
                        }
                    }
                }
            } 
            if(AttackTimer > AttackDelay + CrystalSmashTelegraphLength * Crystals.Count + 120)
            {
                ResetAttack();
            }
        }

        //Phase 2 Attacks
        private void LaserSpin()
        {
            if(AttackTimer < AttackDelay){
                
            } else if (AttackTimer == AttackDelay){
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(4, 0), Vector2.Zero, ProjectileType<GateKeeperLaser>(), LaserSpinDamage, 0, Main.myPlayer, 0, 0);
            } else if(AttackTimer < AttackDelay + 150){

            } else {
                ResetAttack();
            }

        }

        //Phase 3 Attacks
        private void LaserConverge()
        {
            if(AttackTimer < AttackDelay){

            } else if(AttackTimer < AttackDelay + LaserConvergeTelegraphLength){

            } else {

            }
        }
    }
}