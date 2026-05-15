using System.IO.Pipes;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;
using AncientRealms.Core.Systems;
using AncientRealms.Content.Bosses.GateKeeper;
using Terraria.Graphics.Effects;

namespace AncientRealms.Content.Bosses.ShroomCentipede
{
    //Head
    //[AutoloadBossHead]
    public sealed partial class ShroomCentipedeHead : ModNPC
    {
        internal ref float GlobalTimer => ref NPC.ai[0];
        internal ref float Phase => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
		internal ref float AttackTimer => ref NPC.ai[3];

        private bool justRecievedPacket = false; //true for the frame this recieves a packet update to handle any syncronizing
		private float prevTickGlobalTimer; //since globalTimer can jump around from from to frame from recieving packets, we want to make sure we catch logic for every number in the cutscenes if it fastforwarded from a packet (reversed is ignored so we don't double up on sounds/shake)
		private float prevPhase = 0;
		private float prevAttackPhase = 0;
        public int fleeTimer;
		public Rectangle arena;
		const int arenaWidth = 1280;
		const int arenaHeight = 896;
        public Color glowColor = Color.Transparent;
        public List<ShroomCentipedeBody> BodySegments = new List<ShroomCentipedeBody>();
        public ShroomCentipedeTail Tail;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true; // This makes it so that the NPC can be spawned in multiplayer using a boss summoning item.
            NPCID.Sets.BossBestiaryPriority.Add(Type); // This makes it so that the NPC will have a boss icon in the bestiary.
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 130;
            NPC.damage = 25;
            NPC.defense = 20;
            NPC.lifeMax = 2000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f; // Bosses are immune to knockback, so we set this to 0.
            NPC.boss = true;
            NPC.scale = 2f;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) 
        {
	        cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
	        return true;
        }

        private void ChangePhase(AIStates phase, bool resetTime = false)
		{
			Phase = (int)phase;
			if (resetTime)
				AttackTimer = 0;
		}

        public enum AIStates
		{
			SpawnEffects = 0,
			SpawnAnimation = 1,
			FirstPhase = 2,
			SecondPhase = 3,
            ThirdPhase = 4,
			Leaving = 5,
			Dying = 6
		}

        public override void AI()
        {
            //Ticks the timer
			GlobalTimer++;
			AttackTimer++;

			Lighting.AddLight(NPC.Center, new Vector3(1, 0.8f, 0.4f)); //glow

            // Handles fleeing logic. To make sure we dont force a client into having a fleeing boss too early we give the boss a 1 second "charge" to flee
			if (Phase != (int)AIStates.Leaving && Phase != (int)AIStates.Dying && (int)Phase > (int)AIStates.SpawnAnimation && arena != new Rectangle() && !Main.player.Any(n => n.active && !n.dead && arena.Contains(n.Center.ToPoint()))) //if no valid players are detected
				fleeTimer++;
			else
				fleeTimer = 0;

			if (fleeTimer > 60)
			{
				GlobalTimer = 0;
				Phase = (int)AIStates.Leaving; //begone thot!
				NPC.netUpdate = true;
			}

            //Main AI
            switch (Phase)
            {
                //on spawn effects
				case (int)AIStates.SpawnEffects:

					const int arenaWidth = 1600;
					const int arenaHeight = 1000;
					arena = new Rectangle((int)NPC.Center.X  - arenaWidth / 2, (int)NPC.Center.Y - arenaHeight / 2, arenaWidth, arenaHeight);
                    SpawnSegments();
					ChangePhase(AIStates.SpawnAnimation, true);

					break;

				case (int)AIStates.SpawnAnimation: //the animation that plays while the boss is spawning and the title card is shown

					SpawnAnimation();
					ChangePhase(AIStates.FirstPhase, true);
					break;

                case (int)AIStates.FirstPhase:
                    FirstPhase();
                    break;

                case (int)AIStates.SecondPhase:
                    SecondPhase();
                    break;

                case (int)AIStates.ThirdPhase:
                    ThirdPhase();
                    break;

                case (int)AIStates.Leaving:
                	NPC.position.Y += 7;
                    if (GlobalTimer >= 180)
					{
						NPC.active = false; //leave
					}
                    break;

                case (int)AIStates.Dying:
                    //DeathAnimation();
                    break;	
            }

            if (Main.netMode == NetmodeID.Server)
			{
				//instantly switch targets if no longer valid
				Player target = Main.player[NPC.target];
				if (!target.active || target.dead || !arena.Contains(target.Center.ToPoint()))
				{
					RandomizeTarget();
					NPC.netUpdate = true;
				}
			}

			if (Main.netMode == NetmodeID.Server && (Phase != prevPhase || AttackPhase != prevAttackPhase))
			{
				prevPhase = Phase;
				prevAttackPhase = AttackPhase;
				NPC.netUpdate = true;
			}

			prevTickGlobalTimer = GlobalTimer; //potentially just shifted so we store the previous value in case of fastforwarding
			justRecievedPacket = false; //at end of frame set to no longer just recieved

			//Dust perimeter of the arena
			Dust.QuickDustLine(arena.TopRight(), arena.TopLeft(), arenaWidth/20, Color.Purple);
			Dust.QuickDustLine(arena.TopLeft(), arena.BottomLeft(), arenaHeight/20, Color.Purple);
			Dust.QuickDustLine(arena.BottomLeft(), arena.BottomRight(), arenaWidth/20, Color.Purple);
			Dust.QuickDustLine(arena.BottomRight(), arena.TopRight(), arenaHeight/20, Color.Purple);
        }

        private void SpawnSegments()
        {
            if(BodySegments.Count != 0)
				return;
			int BodySegmentCount = 4;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				// Because we want to spawn minions, and minions are NPCs, we have to do this on the server (or singleplayer, "!= NetmodeID.MultiplayerClient" covers both)
				// This means we also have to sync it after we spawned and set up the minion
				return;
			}
			for(int i = 0; i < BodySegmentCount; i++)
			{
				NPC bodySegmentNPC = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<ShroomCentipedeBody>(), NPC.whoAmI);
				ShroomCentipedeBody bodySegment = bodySegmentNPC.ModNPC as ShroomCentipedeBody;
                bodySegment.Head = this;
                bodySegment.SegmentID = i;
                BodySegments.Add(bodySegment);

				// Finally, syncing, only sync on server and if the NPC actually exists (Main.maxNPCs is the index of a dummy NPC, there is no point syncing it)
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.SyncNPC, number: bodySegmentNPC.whoAmI);
				}
			}
            NPC tailSegmentNPC = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<ShroomCentipedeTail>(), NPC.whoAmI);
			ShroomCentipedeTail tailSegment = tailSegmentNPC.ModNPC as ShroomCentipedeTail;
            tailSegment.Head = this;
            Tail = tailSegment;

			// Finally, syncing, only sync on server and if the NPC actually exists (Main.maxNPCs is the index of a dummy NPC, there is no point syncing it)
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncNPC, number: tailSegmentNPC.whoAmI);
            }
        }

        private void SpawnAnimation()
        {
            
        }

        private void FirstPhase()
        {
            if (AttackTimer == 1) //switching out attacks
			{
				AttackPhase++;
				if (AttackPhase > 0)
				AttackPhase = 0;
			}

            switch (AttackPhase) //Attacks
            {
                case 0:
                    break;
            }

            //temp follow player
            NPC.velocity =  5f * Vector2.Normalize(NPC.Center - Main.player[NPC.target].Center);
        }

        private void SecondPhase()
        {
            if (AttackTimer == 1) //switching out attacks
			{
				AttackPhase++;
				if (AttackPhase > 0)
				AttackPhase = 0;
			}

            switch (AttackPhase) //Attacks
            {
                case 0:
                    break;
            }
        }

        private void ThirdPhase()
        {
            if (AttackTimer == 1) //switching out attacks
			{
				AttackPhase++;
				if (AttackPhase > 0)
				AttackPhase = 0;
			}

            switch (AttackPhase) //Attacks
            {
                case 0:
                    break;
            }
        }
    }

    //Body
    public sealed partial class ShroomCentipedeBody : ModNPC
    {
        public ShroomCentipedeHead Head;
        public int SegmentID;

        public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
			NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<ShroomCentipedeHead>();
		}
        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 130;
            NPC.damage = 25;
            NPC.defense = 20;
            NPC.lifeMax = 2000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f; // Bosses are immune to knockback, so we set this to 0.
            NPC.scale = 2f;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
	        cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
	        return true;
        }

        public override bool PreAI() 
        {
            if (!Head.NPC.active) {
                NPC.active = false; // Kill segment if head is gone
                return false;
            }

            // Keep segment health visually synced with the head
            NPC.life = Head.NPC.life;
            NPC.lifeMax = Head.NPC.lifeMax;
            return true;
        }

        public override void AI()
        {
            float maxAngleSeperation = MathHelper.Pi / 6;
            if(SegmentID == 0)
            {
                NPC.Center = Head.NPC.Center + Head.NPC.rotation.ToRotationVector2() * (Head.NPC.width / 2);
                NPC.rotation = MovementHelper.AdjustAim(MathHelper.ToRadians(1.5f), NPC.rotation, Head.NPC.rotation);
                if(Head.NPC.rotation - NPC.rotation > maxAngleSeperation)
                {
                    NPC.rotation = Head.NPC.rotation - maxAngleSeperation;
                }
                else if(Head.NPC.rotation - NPC.rotation < -maxAngleSeperation)
                {
                    NPC.rotation = Head.NPC.rotation + maxAngleSeperation;
                }
            }
            else
            {
                NPC.Center = Head.BodySegments[SegmentID - 1].NPC.Center + Head.BodySegments[SegmentID - 1].NPC.rotation.ToRotationVector2() * (NPC.height / 2);
                NPC.rotation = MovementHelper.AdjustAim(MathHelper.ToRadians(1.5f), NPC.rotation, Head.BodySegments[SegmentID - 1].NPC.rotation);
                if(Head.BodySegments[SegmentID - 1].NPC.rotation - NPC.rotation > maxAngleSeperation)
                {
                    NPC.rotation = Head.BodySegments[SegmentID - 1].NPC.rotation - maxAngleSeperation;
                }
                else if(Head.BodySegments[SegmentID - 1].NPC.rotation - NPC.rotation < -maxAngleSeperation)
                {
                    NPC.rotation = Head.BodySegments[SegmentID - 1].NPC.rotation + maxAngleSeperation;
                }
            }
        }
    }

    //Tail
    public sealed partial class ShroomCentipedeTail : ModNPC
    {
        public ShroomCentipedeHead Head;
        public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
			NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<ShroomCentipedeHead>();
		}
        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 130;
            NPC.damage = 25;
            NPC.defense = 20;
            NPC.lifeMax = 2000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f; // Bosses are immune to knockback, so we set this to 0.
            NPC.scale = 2f;
        }
        
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
	        cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
	        return true;
        }

        public override bool PreAI() 
        {
            if (!Head.NPC.active) {
                NPC.active = false; // Kill segment if head is gone
                return false;
            }

            // Keep segment health visually synced with the head
            NPC.life = Head.NPC.life;
            NPC.lifeMax = Head.NPC.lifeMax;
            return true;
        }

        public override void AI()
        {
            float maxAngleSeperation = MathHelper.Pi / 6;
            NPC.Center = Head.BodySegments[Head.BodySegments.Count - 1].NPC.Center + Head.BodySegments[Head.BodySegments.Count - 1].NPC.rotation.ToRotationVector2() * (Head.BodySegments[Head.BodySegments.Count - 1].NPC.height / 2);                
            NPC.rotation = MovementHelper.AdjustAim(MathHelper.ToRadians(1.5f), NPC.rotation, Head.BodySegments[Head.BodySegments.Count - 1].NPC.rotation);
            if(Head.BodySegments[Head.BodySegments.Count - 1].NPC.rotation - NPC.rotation > maxAngleSeperation)
            {
                NPC.rotation = Head.BodySegments[Head.BodySegments.Count - 1].NPC.rotation - maxAngleSeperation;
            }
            else if(Head.BodySegments[Head.BodySegments.Count - 1].NPC.rotation - NPC.rotation < -maxAngleSeperation)
            {
                NPC.rotation = Head.BodySegments[Head.BodySegments.Count - 1].NPC.rotation + maxAngleSeperation;
            }
        }
    }
}
    