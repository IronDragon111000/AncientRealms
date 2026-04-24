using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System;
using System.IO;
using Terraria.ModLoader.IO;
using AncientRealms.Content.Bosses.EldritchVoid;
using Microsoft.Xna.Framework;

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public class EldritchVoidLaserSource : ModProjectile
    {
        public ref float timer => ref Projectile.ai[0];
        public ref float telegraphLength => ref Projectile.ai[1];
        public ref float targetPlayer => ref Projectile.ai[2];
        public EldritchVoid parent;
        private float AngularSpeed = MathHelper.ToRadians(0.75f); // 0.75 degrees per frame max turn speed

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 32;
            Projectile.penetrate = -1; // Infinite penetration
            Projectile.timeLeft = 10000; // Infinite time left
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // Doesn't collide with tiles
            Projectile.aiStyle = -1; // Custom AI
        }

        public override void AI()
        {
            timer++;
            if (parent == null || !parent.NPC.active)
            {
                for (int i = 0; i < Main.maxNPCs; i++){
                    NPC NPC = Main.npc[i];
                    if (NPC.active && NPC.type == NPCType<EldritchVoid>())
                    {
                        parent = NPC.ModNPC as EldritchVoid;
                    }
                }
            }
            if (parent == null || !parent.NPC.active)
            {
                Projectile.Kill();
                return;
            }
            Player player = Main.player[(int)targetPlayer];
            if(timer <= telegraphLength)
                UpdateAim(parent.NPC.Center + new Vector2(0, -66), AngularSpeed);

            if(timer == 2)
                FireBeam();

            Projectile.Center = parent.NPC.Center + new Vector2(0, -66) + (Projectile.velocity * 100f); // Move the Prism a bit in front of the boss's eye
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Rotate to face the direction of movement
        }

        private void FireBeam() {
			// If for some reason the beam velocity can't be correctly normalized, set it to a default value.
			Vector2 beamVelocity = Vector2.Normalize(Projectile.velocity);
			if (beamVelocity.HasNaNs()) {
				beamVelocity = -Vector2.UnitY;
			}

			// This UUID will be the same between all players in multiplayer, ensuring that the beams are properly anchored on the Prism on everyone's screen.
			int uuid = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);

			int damage = Projectile.damage;
			float knockback = Projectile.knockBack;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, beamVelocity, ModContent.ProjectileType<EldritchVoidLaser>(), damage, knockback, Projectile.owner, timer, uuid);
			

			// After creating the beams, mark the Prism as having an important network event. This will make Terraria sync its data to other players ASAP.
			Projectile.netUpdate = true;
		}

        private void UpdateAim(Vector2 source, float speed) {
			// Get the player's current aiming direction as a normalized vector.
			Vector2 aim = Vector2.Normalize(Main.player[(int)targetPlayer].Center - source);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}

			// Calculate current and target angles
			float currentAngle = Projectile.velocity.ToRotation();
			float targetAngle = aim.ToRotation();

			// Get the smallest angle difference
			float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);

			// Rotate by a constant amount towards the target, clamped to max speed
			float turnAmount = MathHelper.Clamp(angleDiff, -speed, speed);
			float newAngle = currentAngle + turnAmount;

			// Set new velocity
			Projectile.velocity = newAngle.ToRotationVector2();

			if (Projectile.velocity != aim) {
				Projectile.netUpdate = true;
			}
		}

    }
}