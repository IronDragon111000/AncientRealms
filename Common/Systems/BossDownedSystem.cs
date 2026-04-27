using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria;
using System.IO;

namespace AncientRealms.Common.Systems
{
    public class BossDownedSystem : ModSystem
    {
       public static bool downedEldritchVoid = false;

        public override void ClearWorld()
        {
            downedEldritchVoid = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedEldritchVoid"] = downedEldritchVoid;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedEldritchVoid = tag.GetBool("downedEldritchVoid");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = downedEldritchVoid;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedEldritchVoid = flags[0];
        }
    }
}