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
using AncientRealms.Content.SubWorlds.SubSpaceHub;
using AncientRealms.Content.SubWorlds.Mushindigo;

namespace AncientRealms.Content.Tiles.Misc
{
    public class RealmGatewayTile : ModTile
    {
        public override string Texture => "AncientRealms/Content/Items/Placeable/Misc/RealmGateway"; // Use texture of item as tile texture

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Width = 2;
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(200, 200, 200), name);

            // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
			AddToArray(ref TileID.Sets.CountsAsPylon);
        }

        public override bool RightClick(int i, int j)
        {
            if(SubworldLibrary.SubworldSystem.IsActive<SubSpaceHub>())
                SubworldLibrary.SubworldSystem.Enter<Mushindigo>();
            else
                SubworldLibrary.SubworldSystem.Enter<SubSpaceHub>();
            return true;
        }
    }
}