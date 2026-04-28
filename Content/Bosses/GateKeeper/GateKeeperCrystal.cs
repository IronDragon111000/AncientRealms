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
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                if (NPC.Center.Y > parent.arena.Bottom - 20 || NPC.Center.Y < parent.arena.Top + 20 || NPC.Center.X < parent.arena.Left + 20 || NPC.Center.X > parent.arena.Right - 20)
                {
                    NPC.defense = stunnedDefence;
                    stunnedTimer = 180;
                    IsSmashing = false;
                    targetSet = false;
                    NPC.velocity = Vector2.Zero;
                    if(Main.expertMode)
                        for(int i = 0; i < 8; i++)
                        {
                            Projectile shard = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(MathHelper.Pi / 4 * i), ProjectileType<GateKeeperCrystalShard>(), 12, 12f);
                            shard.damage = parent.CrystalSmashProjectileDamage;
                        }
                }
            }
            if(stunnedTimer > 0)            
            {
                stunnedTimer--;
                if(stunnedTimer == 0)
                {
                    NPC.defense = 1000;
                }
            }

            if(parent is null || !parent.NPC.active)
            {
                NPC.active = false;
                return;
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
            }
            Vector2 direction = Vector2.Normalize(TargetPosition - NPC.Center);
                NPC.velocity = direction * 15f;

            IsSmashing = true;
        }

        
        private void RandomizeTarget(out Player target)
		{
			target = null;
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			var Players = new List<int>();

			foreach (Player Player in Main.player.Where(n => n.active && !n.dead))
			{
				Players.Add(Player.whoAmI);
			}

			int random = Main.rand.Next(Players.Count);

			if (random < Players.Count)
				target = Main.player.Where(n => n.active && !n.dead).ToList()[Players[random]];

			NPC.netUpdate = true;
		}
    }
}