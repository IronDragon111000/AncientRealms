using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria;
using System.IO;

namespace AncientRealms.Core.Systems
{
    public class BossDownedSystem : ModSystem
    {
       public static bool downedEldritchVoid = false;
       public static bool downedGateKeeper = false;

        public override void ClearWorld()
        {
            downedEldritchVoid = false;
            downedGateKeeper = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedEldritchVoid"] = downedEldritchVoid;
            tag["downedGateKeeper"] = downedGateKeeper;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedEldritchVoid = tag.GetBool("downedEldritchVoid");
            downedGateKeeper = tag.GetBool("downedGateKeeper");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[1] = downedEldritchVoid;
            flags[0] = downedGateKeeper;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedEldritchVoid = flags[1];
            downedGateKeeper = flags[0];
        }
    }
}