using System;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace AncientRealms.Content.Biomes
{
    public class VoidBiome : ModBiome
    {
        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Void");
		}

        public override bool IsBiomeActive(Player player)
        {
            return SubworldLibrary.SubworldSystem.IsActive<SubSpaceHub>();
        }
    }

    /*public class VoidBiomeSystem : ModSystem
    {
        
    } */
}