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
using AncientRealms.Core.Systems;

namespace AncientRealms.Content.Bosses.SpiderBoss
{
    public sealed partial class SpiderBoss : ModNPC
    {
        internal ref float GlobalTimer => ref NPC.ai[0];
        internal ref float Phase => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];

        private float AttackPhaseCycle = 0;
        internal ref float AttackTimer => ref NPC.ai[3];

        private bool justRecievedPacket = false; //true for the frame this recieves a packet update to handle any syncronizing
        private float prevTickGlobalTimer; //since globalTimer can jump around from from to frame
        private float prevPhase = 0;
        private float prevAttackPhase = 0;
        public int fleeTimer;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 10f; 
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.lifeMax = 6000;
            NPC.aiStyle = -1;
            NPC.defense = 15;
            NPC.scale = 2f;
            NPC.width = 88;
            NPC.height = 88;
        }
    }
}