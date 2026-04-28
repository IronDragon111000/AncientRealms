using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using Terraria.ID;

namespace AncientRealms.Content.Bosses.GateKeeper
{
    
    public class GateKeeperCrystalShard : ModProjectile
    {
        private float acceleration = 0.1f;
        private float maxSpeed = 50f;
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < maxSpeed)
            {
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.velocity += direction * acceleration;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}