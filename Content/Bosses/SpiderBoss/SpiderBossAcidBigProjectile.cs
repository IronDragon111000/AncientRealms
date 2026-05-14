using AncientRealms.Content.Bosses.SpiderBoss;

namespace AncientRealms.Content.Bosses.SpiderBoss
{
    public class SpiderBossAcidBigProjectile : ModProjectile
    {
        public int phase = 0;
        private int spilts = 15;
            public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 7;
		}
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.penetrate = -1; // will be overiden by boss
			Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600; // will be overiden by boss
            Projectile.aiStyle = -1;
        }
        
        public override void AI()
        {
            if(phase >= 6)
            {
                Projectile.hostile = true;
                Projectile.velocity += new Vector2(0, 0.05f); // give projectile gravity
            }
        }

        public override void OnKill(int timeLeft)
        {
            for(int i = 0; i < spilts; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, 
                    Projectile.velocity.RotatedBy((MathHelper.TwoPi / 1.75f) - (i * MathHelper.TwoPi / (1.75 * spilts))), 
                    ModContent.ProjectileType<SpiderBossAcidSmallProjectile>(), 15, 1, 0, 0, 0);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if(phase < 6)
                {Projectile.frame.Y = frameHeight * phase;}
            else
                {Projectile.frame.Y = frameHeight * phase;} // Todo implement an animation during flight
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if(phase >= 6)
            {
                Texture2D texture = Request<Texture2D>(Texture).Value;
			    Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition - new Vector2(Projectile.width / 2, Projectile.height / 2), default, lightColor);
                return false;
            }
            return true;
        }
    }
}