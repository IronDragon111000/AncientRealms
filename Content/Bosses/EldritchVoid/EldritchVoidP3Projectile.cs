using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ModLoader.IO;

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public class EldritchVoidP3Projectile : ModProjectile
    {
        public EldritchVoid parent;
        private Vector2 initialPosition = Vector2.Zero;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 45;
            Projectile.friendly = false;
            Projectile.penetrate = -1; // Infinite penetration
            Projectile.timeLeft = 10000; // Infinite time left
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // Doesn't collide with tiles
            Projectile.aiStyle = -1; // Custom AI
            Projectile.hostile = true; // Hostile to players
        }

        public override void AI()
        {
            if (parent == null || !parent.NPC.active)
            {
                for (int i = 0; i < Main.maxNPCs; i++){
                    NPC NPC = Main.npc[i];
                    if (NPC.active && NPC.type == NPCType<EldritchVoid>())
                    {
                        parent = NPC.ModNPC as EldritchVoid;
                    }
			    }
                if (parent == null || !parent.NPC.active)
                {
                    Projectile.Kill();
                    return;
                }
            }

            if (initialPosition == Vector2.Zero)
            {
                initialPosition = Projectile.position;
            }
            Projectile.velocity = Vector2.Normalize(Projectile.DirectionTo(parent.NPC.Center)) * 5f; // Move towards the parent NPC
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Rotate to face the direction of movement

            // Kill projectile if it reaches or passes the boss
            Vector2 toBoss = parent.NPC.Center - initialPosition;
            Vector2 toProjectile = Projectile.Center - initialPosition;
            if (toBoss != Vector2.Zero && Vector2.Dot(toBoss, toProjectile) > Vector2.Dot(toBoss, toBoss))
            {
                Projectile.Kill();
            }

            // Optional: Add some visual effects or behavior to the projectile here
        }
    }
}