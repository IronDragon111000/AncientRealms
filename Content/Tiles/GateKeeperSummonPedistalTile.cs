using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;
using AncientRealms.Content.Bosses.GateKeeper;
using Terraria.Audio;
using StructureHelper.Models;

namespace AncientRealms.Content.Tiles
{
    public class GateKeeperSummonPedistalTile : ModTile
    {
        public override string Texture => "AncientRealms/Content/Items/Placeable/GateKeeperSummonPedistal"; // Use texture of item as tile texture

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Width = 3;
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(200, 200, 200), name);

            // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
			AddToArray(ref TileID.Sets.CountsAsPylon);
        }

        public override bool RightClick(int i, int j)
        {
            
            int type = ModContent.NPCType<GateKeeper>();
            if(!NPC.AnyNPCs(type))
            {
                // If the player using the item is the client
                // (explicitely excluded serverside here)
                SoundEngine.PlaySound(SoundID.Roar, new Vector2(i,j));

                StructureData data = StructureHelper.API.Generator.GetStructureData("Structures/GateKeeperArena", AncientRealms.Instance);
                NPC.SpawnBoss(i* 16, (j - (data.height - 26) / 2) * 16, type, Main.myPlayer);
                return true;
            }
            return false;
        }
    }
}