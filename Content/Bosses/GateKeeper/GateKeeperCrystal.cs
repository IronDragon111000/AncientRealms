using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace AncientRealms.Content.Bosses.GateKeeper
{
    public class GateKeeperCrystal : ModNPC
    {
        public int crystalID;
        const int stunnedDefence = 13;
        public GateKeeper parent;

        public static int StunnedDefence => stunnedDefence;

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 46;
            Main.npcFrameCount[NPC.type] = 3;
            NPC.frame.Width = 32; 

            NPC.noGravity = true;
            NPC.knockBackResist = 0;
            NPC.aiStyle = -1;

            NPC.damage = 25;
            NPC.lifeMax = 350;
            NPC.defense = 1000; //This is the defence unless it is stunned
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

        public override void AI()
        {

        }

        public override void FindFrame(int frameHeight)
        {
            if(NPC.life > NPC.lifeMax / 2)
            {
                NPC.frame.Y = 0;
            } else if(NPC.life > NPC.lifeMax/5)
            {
                NPC.frame.Y = frameHeight;
            } else
            {
                NPC.frame.Y = 2 * frameHeight;
            }
        }
    }
}