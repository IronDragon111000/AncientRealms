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

namespace AncientRealms.Content.Bosses.GateKeeper
{
	[AutoloadBossHead]
    public sealed partial class GateKeeper : ModNPC
    {
        internal ref float GlobalTimer => ref NPC.ai[0];
        internal ref float Phase => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
		internal ref float AttackTimer => ref NPC.ai[3];

		private bool SpawnedCrystals = false;

		private List<GateKeeperCrystal> Crystals = new List<GateKeeperCrystal>();

        private bool justRecievedPacket = false; //true for the frame this recieves a packet update to handle any syncronizing
		private float prevTickGlobalTimer; //since globalTimer can jump around from from to frame from recieving packets, we want to make sure we catch logic for every number in the cutscenes if it fastforwarded from a packet (reversed is ignored so we don't double up on sounds/shake)
		private float prevPhase = 0;
		private float prevAttackPhase = 0;
        public int fleeTimer;

		public int CrystalsTotalMaxHealth { get; set;} //used for the health bar to show the combined health of the crystals and the core
		public int CrystalsCurrentHealth ;
		public Rectangle arena;
		const int arenaWidth = 1280;
		const int arenaHeight = 896;

		private bool returnToCenter = true;

         public Color glowColor = Color.Transparent;

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
            Main.npcFrameCount[NPC.type] = 81; 
            NPC.frame.Width = 140; 
            NPC.frame.Height = 140; 
			NPC.BossBar = ModContent.GetInstance<GateKeeperBossBar>();


        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
	        cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
	        return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Here we can modify the loot that the NPC drops when it dies. In this case, we are adding a new item drop rule that will drop a custom item called "ExampleBossBag" with a 100% chance.
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GateKeeperTreasureBag>())); (Todo: Add treasure bag drop rule when we have a treasure bag item)
        }

        public override void OnKill()
		{
			NPC.SetEventFlagCleared(ref BossDownedSystem.downedGateKeeper, -1);
		}

        //Used for the various differing passive animations of the different forms
		/*private void SetFrameX(int frame)
		{
			NPC.frame.X = NPC.frame.Width * frame;
		}

		private void SetFrameY(int frame)
		{
			NPC.frame.Y = NPC.frame.Height * frame;
		}*/

        // changes phase
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

			//resets return to center, will be undone later on if it should still be false
			returnToCenter = true;

            //Main AI
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

            switch (Phase)
            {
                //on spawn effects
				case (int)AIStates.SpawnEffects:

					const int arenaWidth = 1600;
					const int arenaHeight = 1000;
					arena = new Rectangle((int)NPC.Center.X  - arenaWidth / 2, (int)NPC.Center.Y - arenaHeight / 2, arenaWidth, arenaHeight);

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

			if(returnToCenter)
			{
				if(NPC.Center.Distance(arena.Center.ToVector2()) > 5)
                {
                    NPC.velocity = Vector2.Normalize(arena.Center.ToVector2() - NPC.Center) * 4f; 
                } else                {
                    NPC.velocity = Vector2.Zero;
                }
			}
        }
        public override void FindFrame(int frameHeight)
        {
            // First Phase
            int startFrame = 0;
            int endFrame = 80;

            

            // Handle Animation
            int frameSpeed = 2;

            // Increment Frame Counters
            NPC.frameCounter += 1f;

            // Adjust Frame
            if(NPC.frameCounter >= frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                // Loop back to start
                if(NPC.frame.Y > endFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }
        }


		private void SpawnAnimation()
		{
			SummonCrystals();
		} 

		private void FirstPhase()
		{
			NPC.dontTakeDamage = true;

			CrystalsCurrentHealth = 0;
			foreach (GateKeeperCrystal Crystal in Crystals)
			{
				if (Crystal != null && Crystal.NPC.active)
				{
					CrystalsCurrentHealth += Crystal.NPC.life;
				}
			}

			if (AttackTimer == 1) //switching out attacks
			{
				AttackPhase++;
				if (AttackPhase > 5)
				AttackPhase = 0;
			}

            switch (AttackPhase) //Attacks
			{
				case 0: 
					ShardVolley(90);
					if(Main.expertMode)
						CrystalArcRing();
					if(AttackTimer > 360 + AttackDelay)
						ResetAttack();
					break;
				case 1: 
					CrystalSmash(); 
					if(AttackTimer > AttackDelay + CrystalSmashTelegraphLength * Crystals.Count + 240)
						ResetAttack();
					break;
				case 2: 
					LaserSpin(); 
					if(Main.expertMode)
						CrystalArcRing();
					if(AttackTimer > AttackDelay + LaserSpinTelegraphLength + 200)
                    	ResetAttack();
					break;
				case 3: 
					CrystalSmash();
					if(AttackTimer > AttackDelay + CrystalSmashTelegraphLength * Crystals.Count + 240)
						ResetAttack();
					break;
				case 4: 
					LaserSweeps();
					if (AttackTimer >= AttackDelay + (LaserSweepLength * 4))
                		ResetAttack();
					break;
				case 5: 
					CrystalSmash();
					if(AttackTimer > AttackDelay + CrystalSmashTelegraphLength * Crystals.Count + 240)
						ResetAttack();
					break;
			}

			if (CrystalsCurrentHealth <= 0)
			{
				ChangePhase(AIStates.SecondPhase, true);
			}
		}

		private void SecondPhase()
		{
			NPC.dontTakeDamage = false;
			if (AttackTimer == 1) //switching out attacks
			{
				AttackPhase++;
				if (AttackPhase > 2)
				AttackPhase = 0;
			}

            switch (AttackPhase) //Attacks
			{
				case 0: 
					LaserSpin(); 
					ShardVolley(120);
					if(AttackTimer > AttackDelay + LaserSpinTelegraphLength + 200)
                    	ResetAttack();
					break;
				case 1: 
					LaserConverge();
					if(AttackTimer > AttackDelay + LaserConvergeTelegraphLength + 200)
						ResetAttack();
					break;
				case 2: 
					Slam();
					if(AttackTimer > AttackDelay + SalmTelegraphLength + 150)
						ResetAttack();
					break;
				case 3: break;
				case 4: break;
			}
		}

		private void ThirdPhase()
		{
			if (AttackTimer == 1) //switching out attacks
			{
				AttackPhase++;
				if (AttackPhase > 1)
					AttackPhase = 1;
			}

            switch (AttackPhase) //Attacks
			{
				case 0: break;
				case 1: break;
				case 2: break;
				case 3: break;
				case 4: break;
			}
		}

		public void SummonCrystals() {
			if(SpawnedCrystals)
				return;
			int minionCount = 4;
			if (Main.expertMode) {
				minionCount += 1; // Increase by 5 if expert or master mode
			}

			if (Main.getGoodWorld) {
				minionCount += 1; // Increase by 5 if using the "For The Worthy" seed
			}

			SpawnedCrystals = true;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				// Because we want to spawn minions, and minions are NPCs, we have to do this on the server (or singleplayer, "!= NetmodeID.MultiplayerClient" covers both)
				// This means we also have to sync it after we spawned and set up the minion
				return;
			}
			
			CrystalsTotalMaxHealth = 0;
			for(int i = 0; i < minionCount; i++)
			{
				NPC CrystalNPC = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<GateKeeperCrystal>(), NPC.whoAmI);
				GateKeeperCrystal Crystal = CrystalNPC.ModNPC as GateKeeperCrystal;
				Crystal.parent = this;
				Crystal.HomePosition = arena.Center.ToVector2() + new Vector2(0, 100f).RotatedBy(MathHelper.ToRadians(360/minionCount * i));
				Crystals.Add(Crystal);
				CrystalsTotalMaxHealth += CrystalNPC.lifeMax;

				// Finally, syncing, only sync on server and if the NPC actually exists (Main.maxNPCs is the index of a dummy NPC, there is no point syncing it)
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.SyncNPC, number: CrystalNPC.whoAmI);
				}
			}

			
		}

    }
}