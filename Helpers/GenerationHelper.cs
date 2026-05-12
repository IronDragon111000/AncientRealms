using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;

namespace AncientRealms.Helpers
{
    public static class GenerationHelper
    {
        public int GetHighestBlockY(int x) 
        {
            for (int y = 0; y < Main.maxTilesY; y++) {
                // Check if the tile is active (not air)
                if (Main.tile[x, y].HasTile) {
                    // Optional: add checks for solid, platform, etc.
                    // if (Main.tileSolid[Main.tile[x, y].TileType])
                    return y; // Returns the highest Y coordinate
                }
            }
            return Main.maxTilesY; // Returns bottom if no block found
        }

    }
}