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
        internal ref float crystalID => ref NPC.ai[1];
        const int stunnedDefence = 13;
        public GateKeeper parent;

        public Vector2 TargetPosition;

        public bool targetSet = false;
        public int stunnedTimer = 0;
        public bool IsSmashing = false;

        public static int StunnedDefence => stunnedDefence;

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 46;
            Main.npcFrameCount[NPC.type] = 3;
            NPC.frame.Width = 32; 

            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.aiStyle = -1;

            NPC.damage = 25;
            NPC.lifeMax = 350;
            NPC.defense = 1000; //This is the defence unless it is stunned
            NPC.friendly = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

        public override void AI()
        {
            if(IsSmashing)
            {
                Vector2 direction = Vector2.Normalize(TargetPosition - NPC.Center);
                NPC.velocity = direction * 15f;
                if (Vector2.Distance(NPC.Center, TargetPosition) < 20f)
                {
                    NPC.defense = stunnedDefence;
                    stunnedTimer = 180;
                    IsSmashing = false;
                    targetSet = false;
                    NPC.velocity = Vector2.Zero;
                }
            }
            if(stunnedTimer > 0)            {
                stunnedTimer--;
                if(stunnedTimer == 0)
                {
                    NPC.defense = 1000;
                }
            }
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

        public void SmashAttack()
        {
            if(!targetSet)
            {
                foreach (Player player in Main.player.Where(n => n.active && !n.dead && parent.arena.Contains(n.Center.ToPoint())))
                {
                    if(!targetSet || Vector2.Distance(player.Center, NPC.Center) < Vector2.Distance(TargetPosition, NPC.Center))
                    {
                        TargetPosition = player.Center;
                        targetSet = true;
                    }
                }
                float angle = (NPC.Center - TargetPosition).ToRotation();
                angle += MathHelper.Pi;
                if (angle > MathHelper.TwoPi) {
				    angle -= MathHelper.TwoPi;
                }
                else if (angle < 0) {
                                angle += MathHelper.TwoPi;
                }

                TargetPosition += angle.ToRotationVector2() * 100f;
            }

            IsSmashing = true;
        }
    }
}