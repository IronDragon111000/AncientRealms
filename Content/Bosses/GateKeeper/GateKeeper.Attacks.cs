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
        public Vector2 AttackDirection;
        public List<GateKeeperLaser> Lasers = new List<GateKeeperLaser>();

        // Attack Damage for attacks
        public int CrystalSmashDamage = 20;
        public int CrystalSmashProjectileDamage = 10;
        public int LaserSpinDamage = 25;
        public int LaserConvergeDamage = 25;

        //How long before an attack starts - exists to give players time to setup for next attack
        public float AttackDelay = 60f;
        // Telegraph Lengths for attacks
        public float CrystalSmashTelegraphLength = 45f;
        public float CrystalSmashProjectileTelegraphLength = 20f;
        public float LaserSpinTelegraphLength = 80f;
        public float LaserConvergeTelegraphLength = 60f;

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

        private void ClosestTarget()
        {
            NPC.target = null;
            foreach (Player player in Main.player.Where(n => n.active && !n.dead && parent.arena.Contains(n.Center.ToPoint())))
            {
                if(target == null || Vector2.Distance(player.Center, NPC.Center) < Vector2.Distance(NPC.target, NPC.Center))
                {
                    NPC.target = player;
                }
            }
        }

        private void CrystalSmash()
        {
            if(AttackTimer >= 150)
            {
                for(int i= 0; i < Crystals.Count; i++)
                {
                    if(Crystals[i] != null && Crystals[i].NPC.active)
                    {
                        if(AttackTimer < 150 + CrystalSmashTelegraphLength * (i + 1))
                        {
                            Crystals[i].NPC.velocity = Vector2.Zero;
                            Crystals[i].NPC.rotation += 0.3f;
                        } else if (AttackTimer == 150 + CrystalSmashTelegraphLength * (i + 1))
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

        private void CrystalArcRing()
        {
            
        }

        private void LaserSpin()
        {
            if(AttackTimer == AttackDelay)
            {
                ClosestTarget()
                AttackDirection = Vector2.Normalize(NPC.target.Center - NPC.Center);
                Lasers.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, AttackDirection, ModContent.ProjectileType<GateKeeperLaser>(), LaserSpinDamage, NPC.knockBack, 0, 0, 0) as GateKeeperLaser);
                Lasers[0].source = this;
            }
            if(AttackTimer > AttackDelay)
            {
                //Adjust aim
                float AimSpeed = MathHelper.ToRadians(0.75f);
                // Get the player's current aiming direction as a normalized vector.
                Vector2 aim = Vector2.Normalize(NPC.target.Center - NPC.Center);
                if (aim.HasNaNs()) {
                    aim = -Vector2.UnitY;
                }

                // Calculate current and target angles
                float currentAngle = AttackDirection.ToRotation();
                float targetAngle = aim.ToRotation();

                // Get the smallest angle difference
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);

                // Rotate by a constant amount towards the target, clamped to max speed
                float turnAmount = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
                float newAngle = currentAngle + turnAmount;

                // Set new AttackDirection
                AttackDirection = newAngle.ToRotationVector2();

                if (Projectile.AttackDirection != aim) {
                    Projectile.netUpdate = true;
                }

                // Update the Laser Projectile
                Lasers[0].velocity = Vector2.Normalize(AttackDirection);
                if(AttackTimer > AttackDelay + LaserSpinTelegraphLength)
                    Lasers[0].Tell = false;

                //End Attack
                if(AttackTimer > AttackDelay + LaserSpinTelegraphLength + 200)
                {
                    ResetAttack();
                    for(int i = 0; i < Lasers.Count; i++)
                    {
                        Lasers[i].active = false;
                    }
                    Lasers = New List<GateKeeperLaser>();
                }
            }
        }

        private void LaserConverge()
        {
            if(AttackTimer < AttackDelay){

            } else if(AttackTimer < AttackDelay + LaserConvergeTelegraphLength){

            } else {

            }
        }
    }
}