namespace AncientRealms.Content.Bosses.SpiderBoss
{
    public class SpiderBossAcidSmallProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
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
            Projectile.velocity += new Vector2(0, 0.5f); // give projectile gravity    
        }
    }
}