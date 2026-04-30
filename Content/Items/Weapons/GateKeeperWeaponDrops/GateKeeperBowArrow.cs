using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace AncientRealms.Content.Items.Weapons.GateKeeperWeaponDrops
{
    public class GateKeeperBowArrow : ModProjectile
    {
        private bool isHoming = false; // Flag to check if the arrow has started homing in on a target
        private NPC targetNPC; // The NPC that the arrow is homing in on
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.arrow = true;
            Projectile.light = 0.5f;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // Use the built-in arrow AI style for movement and behavior.
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
            }
        }

        override public void AI()
        {
            if (!isHoming)
            {
                // Check for nearby NPCs to home in on
                float homingRange = 150f; // Range within which the arrow will start homing
                NPC closestNPC = null;
                float closestDistance = homingRange;

                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy() && !npc.friendly)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestNPC = npc;
                        }
                    }
                }

                if (closestNPC != null)
                {
                    targetNPC = closestNPC;
                    isHoming = true; // Start homing in on the target
                }
            }
            else
            {
                if (targetNPC == null || !targetNPC.active || !targetNPC.CanBeChasedBy() || Vector2.Distance(Projectile.Center, targetNPC.Center) > 200f)
                {
                    isHoming = false;
                    return;
                }

                Vector2 targetDirection = targetNPC.Center - Projectile.Center;
                if (targetDirection == Vector2.Zero)
                {
                    return;
                }

                targetDirection.Normalize();
                Vector2 currentDirection = Projectile.velocity;
                float speed = currentDirection.Length();

                if (speed == 0f)
                {
                    speed = 6f;
                    currentDirection = targetDirection;
                }
                else if(speed < 12f)
                {
                    speed += 0.1f; // Gradually increase speed up to a maximum
                    currentDirection.Normalize();
                } else
                {
                    speed = 12f; // Cap the speed to prevent it from becoming too fast
                    currentDirection.Normalize();
                }

                float homingStrength = 0.16f; // Lower values mean slower turning
                Vector2 newDirection = Vector2.Normalize(Vector2.Lerp(currentDirection, targetDirection, homingStrength));
                Projectile.velocity = newDirection * speed;
            }
        }
    }
}