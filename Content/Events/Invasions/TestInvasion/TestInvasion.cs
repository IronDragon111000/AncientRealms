using AncientRealms.Core.Systems;
using AncientRealms.Content.Events.Invasions.TestInvasion;

namespace AncientRealms.Content.Events.Invasions.TestInvasion
{
    public class TestInvasion : ModSystem
    {
        public static int Wave = 0; // 0 means not active
        public static int completion = 0;
        public static int WaveTimer = 0;
        public static int EventTimer = 0;
        public List<NPC> EventMinions = new List<NPC>();
        public static int MaxWave = 5;
        public override void ClearWorld()
        {
            Wave = 0;
            completion = 0;
            WaveTimer = 0;
            EventTimer = 0;
        }

        public static void StartEvent()
        {
            
        }

        public static void EventEnd()
        {
            Wave = 0;
             EventTimer = 0;
            WaveTimer = 0;
            completion = 0f;
        }

        public override void preUpdateWorld()
        {
            WaveTimer++;
            EventTimer++;
            if(WaveTimer == 1)
            {
                
            }
        }
    }

    public class TestInvasionNPC : GlobalNPC
    {
        public static int maxPoints = 15;
        public override void OnKill(Terraria.NPC npc)
        {
            if (TestInvasion.Wave > 0)
            {
                maxPoints = TestInvasion.Wave switch
                {
                    1 => 30,
                    2 => 40,
                    3 => 50,
                    4 => 80,
                    5 => 100,
                    6 => 150,
                    7 => 500,
                    _ => 15,
                };
                if (TestInvasion.completion >= maxPoints)
                {
                    if (TestInvasion.Wave >= 7)
                        TestInvasion.EventEnd();
                    else
                    {
                        TestInvasion.completion = 0;
                        TestInvasion.Wave++;

                        string waveText = GetWaveChatText(TestInvasion.Wave);
                        Color color = new(175, 75, 255);
                        if (Main.netMode == NetmodeID.Server)
                            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(waveText), color);
                        else if (Main.netMode == NetmodeID.SinglePlayer)
                            Main.NewText(Language.GetTextValue(waveText), color);
                    }
                }
            }
        }
        public static string GetWaveChatText(int wave)
        {
            string wavetext = "Wave: " + wave + ": ";
            IDictionary<int, float> spawnpool = SpawnPool.ElementAt(wave);
            wavetext += Lang.GetNPCName(spawnpool.First().Key);
            foreach (KeyValuePair<int, float> key in spawnpool.Skip(1))
            {
                wavetext += ", " + Lang.GetNPCName(key.Key);
            }
            return wavetext;
        }
        public static List<IDictionary<int, float>> SpawnPool
        {
            get => new()
            {
                new Dictionary<int, float> { {ModContent.NPCType<CrystalSlug>(), 1f} }, // 1
                new Dictionary<int, float> { // 2
                    {NPCID.Zombie, 1f},
                },
                new Dictionary<int, float> { // 3
                    {NPCID.Zombie, 1f},
                },
                new Dictionary<int, float> { // 4
                    {NPCID.Zombie, 1f},
                },
                new Dictionary<int, float> { // 5
                    {NPCID.Zombie, 1f},
                },
                new Dictionary<int, float> { // 6
                    {NPCID.Zombie, 1f},
                },
                new Dictionary<int, float> { // 7
                    {NPCID.Zombie, 1f},
                },
                new Dictionary<int, float> { // 8
                    {NPCID.Zombie, 1f},
                },
            };
        }
    }
}