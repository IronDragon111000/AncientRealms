using System;
using System.Linq;
using System.Reflection;
using AncientRealms.Content.Tiles.Misc;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using AncientRealms.Content.SubWorlds.SubSpaceHub;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Capture;

namespace AncientRealms.Content.Biomes
{
    public class RealmGatewayBiome : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;        
        public override bool IsBiomeActive(Player player)
        {
            return SubworldLibrary.SubworldSystem.IsActive<SubSpaceHub>() && ModContent.GetInstance<RealmGatewayBiomeSystem>().realmGateWayTileActive;
        }
    }

    public class RealmGatewayBiomeSystem : ModSystem
    {
        public bool realmGateWayTileActive;
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			realmGateWayTileActive = tileCounts[ModContent.TileType<RealmGatewayTile>()] > 0;
		}
    }
}