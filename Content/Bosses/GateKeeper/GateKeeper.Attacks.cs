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
        public Vector2 destination;
        public List<GateKeeperLaser> Lasers = new List<GateKeeperLaser>();

        // Attack Damage for attacks
        public int CrystalSmashDamage = 20;
        public int CrystalSmashProjectileDamage = 10;
        public int LaserSpinDamage = 25;
        public int LaserConvergeDamage = 25;
        public int LaserSweepDamage = 20;
        public int SlamDamage = 45;
        public int ShardVolleyDamage = 15;

        //How long before an attack starts - exists to give players time to setup for next attack
        public float AttackDelay = 60f;
        // Telegraph Lengths for attacks
        public float CrystalSmashTelegraphLength = 45f;
        public float CrystalSmashProjectileTelegraphLength = 20f;
        public float LaserSpinTelegraphLength = 90f;
        public float LaserConvergeTelegraphLength = 30f;
        public float SlamTelegraphLength = 90f;

        //Attack Lengths
        public float LaserSweepLength = 120f;

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.damage *= (int)balance;
			NPC.lifeMax = (int)(2000 + (1000 * balance));
        }
        public void ResetAttack()
		{
			AttackTimer = 0;
            for(int i = 0; i < Crystals.Count; i++)
            {
                Crystals[i].Arcing = false;
                Crystals[i].NPC.netUpdate = true;
            }
            for(int i = 0; i < Lasers.Count; i++)
        	{
            	Lasers[i].Projectile.active = false; 
           	}
            Lasers = new List<GateKeeperLaser>();
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
            Player target = null;
            foreach (Player player in Main.player.Where(n => n.active && !n.dead && arena.Contains(n.Center.ToPoint())))
            {
                if(target == null || Vector2.Distance(player.Center, NPC.Center) < Vector2.Distance(target.Center, NPC.Center))
                {
                    target = player;
                }
            }

            NPC.target = target.whoAmI;
            NPC.netUpdate = true;
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
        }

        private void CrystalArcRing()
        {
            if(AttackTimer >= AttackDelay)
            {
                for(int i= 0; i < Crystals.Count; i++)
                {
                    if(Crystals[i] != null && Crystals[i].NPC.active && Crystals[i].stunnedTimer <= 0)
                    {
                        Crystals[i].Arcing = true;
                        Vector2 DirectionToBoss = Vector2.Normalize(NPC.Center - Crystals[i].NPC.Center);;
                        if(AttackTimer < AttackDelay + 30)
                        {
                            Crystals[i].NPC.velocity = DirectionToBoss * (-5f);
                            Crystals[i].NPC.friendly = true;
                        } else
                        {
                            Crystals[i].NPC.velocity = Vector2.Zero;
                            Crystals[i].NPC.friendly = false;
                        }
                        Crystals[i].NPC.rotation = DirectionToBoss.ToRotation() + MathHelper.PiOver2;
                        float AngluarSpeed = MathHelper.TwoPi / 540;
                        Crystals[i].NPC.velocity += ((NPC.Center - Crystals[i].NPC.Center) * AngluarSpeed).RotatedBy(MathHelper.PiOver2);
                    }
                }
            }
        }

        private void LaserSpin()
        {
            if(AttackTimer == AttackDelay)
            {
                ClosestTarget();
                AttackDirection = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);
                Projectile laser = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, AttackDirection, ModContent.ProjectileType<GateKeeperLaser>(), LaserSpinDamage, 1, 0, 0, 0);
                Lasers.Add(laser.ModProjectile as GateKeeperLaser);
                Lasers[0].source = this;
            }
            if(AttackTimer > AttackDelay)
            {
                //Adjust aim
                float AimSpeed = MathHelper.ToRadians(0.64f);
                // Get the player's current aiming direction as a normalized vector.
                Vector2 aim = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);
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

                if (AttackDirection != aim) {
                    NPC.netUpdate = true;
                }

                // Update the Laser Projectile
                Lasers[0].Projectile.velocity = Vector2.Normalize(AttackDirection);
                if(AttackTimer > AttackDelay + LaserSpinTelegraphLength)
                    Lasers[0].Tell = false;
            }
        }

        private void LaserConverge()
        {
            returnToCenter = false;
            if(AttackTimer == 1)
            {
                Random rand = new Random();
                if(rand.Next(10) >= 5)
                {
                    destination = new Vector2(arena.Right, arena.Center.Y);
                    AttackDirection = (MathHelper.Pi + MathHelper.ToRadians((float)rand.Next(-30, 30))).ToRotationVector2();
                }
                else
                {
                    destination = new Vector2(arena.Left, arena.Center.Y);
                    AttackDirection = MathHelper.ToRadians(rand.Next(-30, 30)).ToRotationVector2();
                }
            }
            if((destination - NPC.Center).Length() > 5f)
            {
               NPC.velocity = Vector2.Normalize(destination - NPC.Center) * 15f; 
            } 
            else
            {
                NPC.velocity = Vector2.Zero;
            }
            if(AttackTimer == AttackDelay)
            {
                //The Actual lasers
                    // Points up
                Projectile laser0 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, 
                    new Vector2(0, -1), ModContent.ProjectileType<GateKeeperLaser>(), LaserSpinDamage, 1, 0, 0, 0);
                Lasers.Add(laser0.ModProjectile as GateKeeperLaser);
                Lasers[0].source = this;   
                    // Points down
                Projectile laser1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, 
                    new Vector2(0, 1), ModContent.ProjectileType<GateKeeperLaser>(), LaserSpinDamage, 1, 0, 0, 0);
                Lasers.Add(laser1.ModProjectile as GateKeeperLaser);
                Lasers[1].source = this;

                // These 2 show where the other 2 lasers final positions will be. They will always have tell = true
                Projectile laser2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, 
                    AttackDirection.RotatedBy(-MathHelper.Pi / 12f), ModContent.ProjectileType<GateKeeperLaser>(), LaserSpinDamage, 1, 0, 0, 0);
                Lasers.Add(laser2.ModProjectile as GateKeeperLaser);
                Lasers[2].source = this;
                Projectile laser3 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, 
                    AttackDirection.RotatedBy(MathHelper.Pi / 12f), ModContent.ProjectileType<GateKeeperLaser>(), LaserSpinDamage, 1, 0, 0, 0);
                Lasers.Add(laser3.ModProjectile as GateKeeperLaser);
                Lasers[3].source = this;
            }
            if(AttackTimer > AttackDelay + LaserConvergeTelegraphLength){
                Lasers[0].Tell = false;
                Lasers[1].Tell = false;
                //Adjust aim
                float AimSpeed = MathHelper.ToRadians(0.85f);
                Vector2 aim = Lasers[2].Projectile.velocity;
                Vector2 aim1 = Lasers[3].Projectile.velocity;
                if(destination.X < arena.Center.X)
                {
                    aim = Lasers[2].Projectile.velocity;
                    aim1 = Lasers[3].Projectile.velocity;
                }
                else
                {
                    aim = Lasers[3].Projectile.velocity;
                    aim1 = Lasers[2].Projectile.velocity;
                }
                if (aim.HasNaNs()) {
                    aim = -Vector2.UnitY;
                }

                // Calculate current and target angles
                float currentAngle = Lasers[0].Projectile.velocity.ToRotation();
                float targetAngle = aim.ToRotation();

                // Get the smallest angle difference
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);

                // Rotate by a constant amount towards the target, clamped to max speed
                float turnAmount = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
                float newAngle = currentAngle + turnAmount;

                // Set new AttackDirection
                Lasers[0].Projectile.velocity = newAngle.ToRotationVector2();
                if (aim1.HasNaNs()) {
                    aim1 = -Vector2.UnitY;
                }

                // Calculate current and target angles
                float currentAngle1 = Lasers[1].Projectile.velocity.ToRotation();
                float targetAngle1 = aim1.ToRotation();

                // Get the smallest angle difference
                float angleDiff1 = MathHelper.WrapAngle(targetAngle1 - currentAngle1);

                // Rotate by a constant amount towards the target, clamped to max speed
                float turnAmount1 = MathHelper.Clamp(angleDiff1, -AimSpeed, AimSpeed);
                float newAngle1 = currentAngle1 + turnAmount1;

                // Set new AttackDirection
                Lasers[1].Projectile.velocity = newAngle1.ToRotationVector2();

                if (AttackDirection != aim) {
                    NPC.netUpdate = true;
                }

                if(currentAngle == targetAngle && currentAngle1 == targetAngle1)
                {
                    FocusedBulletHell(); 
                }
            }
        }

        private void FocusedBulletHell()
        {
            if(AttackTimer % 30 == 0)
            {
                Random rand = new Random();
                Vector2 v = Vector2.Zero;
                if(NPC.Center.X < arena.Center.X)              {
                    v = Lasers[1].Projectile.velocity.RotatedBy((-MathHelper.Pi / 54f) * rand.Next(1,8));
                }
                else                {
                    v = Lasers[1].Projectile.velocity.RotatedBy((MathHelper.Pi / 54f) * rand.Next(1,8));
                }
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, v, ModContent.ProjectileType<GateKeeperCrystalShard>(), 15, 1, 0, 0 ,0);
            }
        }

        private void LaserSweeps()
        {
            returnToCenter = false;
            if(AttackTimer > AttackDelay)
            {
                if(NPC.Center.Y - Main.player[NPC.target].Center.Y + (10 * 16) > 5)
                {
                    NPC.velocity.Y = -5f;
                }
                else if(NPC.Center.Y - Main.player[NPC.target].Center.Y + (10 * 16) < -5)
                {
                    NPC.velocity.Y = 5f;
                }
                else
                {
                    NPC.velocity.Y = 0f;
                }
                LaserSweep((int)((AttackTimer - AttackDelay) % LaserSweepLength));
            }
        }
        private void LaserSweep(int timer)
        {
            bool leftSide = true;
            AttackDirection = new Vector2(0, 1);
            if(timer == 0)
            {
                NPC.velocity.X = 0f;
                RandomizeTarget();
            }
            if(destination.X - arena.Center.X > 0)
            {
                leftSide = false;
            }
            if(timer < 35)
            {
                destination = new Vector2(Main.player[NPC.target].Center.X, arena.Top);
                if(leftSide){destination.X += -50f;}else{destination.X += 50f;}
                if(Math.Abs(NPC.Center.X - destination.X) > 5){
                    NPC.velocity.X = (destination.X - NPC.Center.X) / 10f;
                }
                else
                {
                    NPC.velocity.X = 0f;
                }
                
            }
            else if (timer == 35)
            {
                Projectile laser = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, AttackDirection, ModContent.ProjectileType<GateKeeperLaser>(), LaserSweepDamage, 1, 0, 0, 0);
                Lasers.Add(laser.ModProjectile as GateKeeperLaser);
                Lasers[0].source = this;
                Lasers[0].Tell = false;
            } 
            else 
            {
                if(leftSide){NPC.velocity.X = 5f;}else{NPC.velocity.X = -5f;}
                //End Attack
                if(timer >= LaserSweepLength - 1)
                {
                    for(int i = 0; i < Lasers.Count; i++)
                    {
                        Lasers[i].Projectile.active = false;
                    }
                    Lasers = new List<GateKeeperLaser>();
                }
                if (!arena.Contains(NPC.Center.ToPoint()))
                {
                    AttackTimer += LaserSweepLength - timer -1; //Skip to next sweep if we leave the arena
                    for(int i = 0; i < Lasers.Count; i++)
                    {
                        Lasers[i].Projectile.active = false;
                    }
                    Lasers = new List<GateKeeperLaser>();
                }
            }
        }

        private void ShardVolley(float VolleyTimeInterval, float ShardSpacing = MathHelper.Pi / 8f, float AngleWidth = MathHelper.TwoPi, float TargetAngle = 0f)
        {
            if(AttackTimer % VolleyTimeInterval == 0)
            {
                Random rand = new Random();
                float randomOffset = MathHelper.ToRadians(rand.Next(-((int)MathHelper.ToDegrees(ShardSpacing/2)), ((int)(MathHelper.ToDegrees(ShardSpacing/2)))));
                for(int i = 0; i < AngleWidth/ShardSpacing; i++)
                {
                    Projectile shard = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, 
                        ((i * ShardSpacing) + TargetAngle - (AngleWidth/2) + randomOffset).ToRotationVector2() * 25f, 
                        ModContent.ProjectileType<GateKeeperShardVolleyProjectile>(), ShardVolleyDamage, 1, 0, 40f, 40f);
                    (shard.ModProjectile as GateKeeperShardVolleyProjectile).source = this;
                }
            }
        }

        private void Slam()
        {
            returnToCenter = false;
            if(AttackTimer < AttackDelay)
                return;
            if(AttackTimer == AttackDelay)
                RandomizeTarget();
            if(AttackTimer < AttackDelay + (0.6 * SlamTelegraphLength))
            {
                NPC.Center = Main.player[NPC.target].Center + new Vector2(0,-20 * 16);
            } 
            else if(AttackTimer < AttackDelay + SlamTelegraphLength)
            {
                NPC.Center += new Vector2(0, -0.05f);
            }
            else
            {
                if(NPC.Center.Y >= arena.Bottom - (NPC.height / 2.2))
                {
                    NPC.velocity = Vector2.Zero;

                    ShardVolley(60 , MathHelper.Pi / 7, MathHelper.Pi, -MathHelper.PiOver2);
                }
                else
                {
                    NPC.velocity = new Vector2(0, 30f);
                }
            }
        }
    }
}