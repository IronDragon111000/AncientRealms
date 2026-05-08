
using AncientRealms.Core.Systems;
using AncientRealms.Content.Events.Invasions.TestInvasion;

namespace AncientRealms.Events.Invasions
{
    public class InvasionSystem : ModSystem
    {
    }

    public class InvasionNPC : ModNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
             if (TestInvasion.Wave != 0 && spawnInfo.Player.ZoneOverworldHeight && !spawnInfo.Player.ZoneTowerNebula && !spawnInfo.Player.ZoneTowerSolar && !spawnInfo.Player.ZoneTowerStardust && !spawnInfo.Player.ZoneTowerVortex)
            {
                pool.Clear();
                if (Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1).WallType is not WallID.DirtUnsafe)
                {
                    IDictionary<int, float> spawnpool = TestInvasionNPC.SpawnPool.ElementAt(TestInvasion.Wave);
                    foreach (KeyValuePair<int, float> key in spawnpool)
                    {
                        pool.Add(key.Key, key.Value);
                    }
                }
                return;
            }
        }
    }
}