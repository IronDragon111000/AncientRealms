namespace AncientRealms.Content.Events.Invasions.TestInvasion
{
    public class CrystalSlug : ModNPC
    {
        public ref float timer => NPC.AI[1];
        public override void SetDefaults()
        {
            NPC.width = 58;
            NPC.damage = 46;
            NPC.defense = 30;
            NPC.lifeMax = 175;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
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