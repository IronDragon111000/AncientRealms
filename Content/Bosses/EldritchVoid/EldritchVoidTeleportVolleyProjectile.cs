using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Graphics.Texture2D;
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AncientRealms.Content.Bosses.EldritchVoid;
using AncientRealms.Helpers;
using AncientRealms;

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public class EldritchVoidTeleportVolleyProjectile : ModProjectile
    {
        private float acceleration = 0.1f;
        private float maxSpeed = 35f;
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20; 
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.light = 0.5f;
            Projectile.timeLeft = 300;
            Projectile.hostile = true;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < maxSpeed && Projectile.timeLeft < 280)
            {
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.velocity += direction * acceleration;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}