namespace AncientRealms.Content.Events.Invasions.TestInvasion
{
    public class CrystalSlug : ModNPC
    {
        internal ref float timer => ref NPC.ai[1];
        public override void SetDefaults()
        {
            NPC.width = 58;
            NPC.height = 46;
            NPC.damage = 46;
            NPC.defense = 30;
            NPC.lifeMax = 175;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0.2f;
        }

        public override void AI()
        {
            timer++;
            if(timer % 330 > 300)
            {
                
            } else if(timer % 330 > 210)
            {
                
            }
        }
    }
}