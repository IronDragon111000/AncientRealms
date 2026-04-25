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

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    [AutoloadBossHead]
    public sealed partial class EldritchVoid : ModNPC
    {
        internal ref float GlobalTimer => ref NPC.ai[0];
        internal ref float Phase => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];

        private bool justRecievedPacket = false; //true for the frame this recieves a packet update to handle any syncronizing
        private float prevTickGlobalTimer; //since globalTimer can jump around from from to frame
        private float prevPhase = 0;
        private float prevAttackPhase = 0;
        public int fleeTimer;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true; // This makes it so that the NPC can be spawned in multiplayer using a boss summoning item.
            NPCID.Sets.BossBestiaryPriority.Add(Type); // This makes it so that the NPC will have a boss icon in the bestiary.
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 170;
            NPC.scale = 2f;
            NPC.damage = 65;
            NPC.defense = 30;
            NPC.lifeMax = 17500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f; // Bosses are immune to knockback, so we set this to 0.
            NPC.boss = true; 
            Main.npcFrameCount[NPC.type] = 1; 
            NPC.frame.Width = 104; 
            NPC.frame.Height = 178; 
            NPC.dontTakeDamage = true; // Don't take damage during the spawn animation
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
	        cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
	        if (Phase == (int)AIStates.SpawnAnimation || Phase == (int)AIStates.Dying || Phase == (int)AIStates.Leaving || Phase == (int)AIStates.SpawnEffects)
                return false; // Don't hit the player during the spawn & death animation
    
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Here we can modify the loot that the NPC drops when it dies. In this case, we are adding a new item drop rule that will drop a custom item called "ExampleBossBag" with a 100% chance.
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GateKeeperTreasureBag>())); (Todo: Add treasure bag drop rule when we have a treasure bag item)
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
            {
				GlobalTimer = 0;
                AttackTimer = 0;
            }
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
            //glowing effect
            Lighting.AddLight(new Vector2(NPC.position.X + NPC.width/2, NPC.position.Y + NPC.height/2), 2.1f, 2f, 2.2f);

            //Ticks the timer
            GlobalTimer++;
            AttackTimer++;

            // Handles fleeing logic. To make sure we dont force a client into having a fleeing boss too early we give the boss a 1 second "charge" to flee
			if (Phase != (int)AIStates.Leaving && Phase != (int)AIStates.Dying && (int)Phase > (int)AIStates.SpawnAnimation && !Main.player.Any(n => n.active && !n.dead )) //if no valid players are detected
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
                case (int)AIStates.SpawnEffects:
                    NPC.Opacity = 0f; // Start fully transparent for the spawn animation

					ChangePhase(AIStates.SpawnAnimation, true);
					break;
                case (int)AIStates.SpawnAnimation:
                    SpawnAnimation();
                    if (GlobalTimer > 155) // After the spawn animation is done, transition to the first phase
                    {
                        NPC.dontTakeDamage = false; // Allow the NPC to take damage after the spawn animation is complete
					    ChangePhase(AIStates.FirstPhase, true);
                    }
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
                    Leaving();
                    break;
                case (int)AIStates.Dying:
                    Dying();
                    break;
            }

            prevTickGlobalTimer = GlobalTimer;
            prevPhase = Phase;
            prevAttackPhase = AttackPhase;
        }

        private void SpawnAnimation()
        {
            if (GlobalTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }

            NPC.Opacity = Math.Min(GlobalTimer / 150f, 1f); // Fade in over 2.5 seconds (150 ticks);            
        }

        private void Leaving()
        {
            NPC.Opacity = Math.Max(1f - (GlobalTimer / 150f), 0f); // Fade out over 2.5 seconds (150 ticks)
            NPC.position.Y += -0.5f; // Move upwards slightly while fading out for a more dramatic effect
                if (GlobalTimer >= 150)
				{
					NPC.active = false; //leave
				}
        }

        private void FirstPhase()
        {
            if (AttackTimer == 1) //switching out attacks
            {
                AttackPhase++;
                if (AttackPhase > 2)
                    AttackPhase = 1;
            }
            switch (AttackPhase) //Attacks
            {
                case 0: break;
                case 1: break;
                case 2: break;
            }
            if(NPC.life < NPC.lifeMax * 0.6f) //transition to phase 2 at 60% health   
            {
                ChangePhase(AIStates.SecondPhase, true);
            }
        }

        private void SecondPhase()
        {
            if (AttackTimer == 1) //switching out attacks
            {
                AttackPhase++;
                if (AttackPhase > 2)
                    AttackPhase = 1;
            }
            switch (AttackPhase) //Attacks
            {
                case 0: break;
                case 1: break;
                case 2: break;
            }

            if(NPC.life < NPC.lifeMax * 0.15f) //transition to phase 3 at 15% health   
            {
                ChangePhase(AIStates.ThirdPhase, true);
            }
        }

        private void ThirdPhase()
        {
            //Pull the players in
            // Currently causes players to be unable to fall through platforms under the boss, this will need to be fixed before release, but for now when coupled with the mod InstantPlatformFallthrough it fixs it but is not an ideal solution as it allows instant fall though all the time instead of just during the fight
            foreach (Player Player in Main.player.Where(n => n.active && !n.dead))
            {
                Vector2 direction = NPC.Center - Player.Center;
                float pullStrength = 0.0075f; // Adjust this value to increase or decrease the pull strength
                Player.position += direction * pullStrength; // Apply the pulling force to the player's velocity
            }

            if(GlobalTimer == 1) // Chat Message telling the player they have infinite flight time
            {
                if(Main.netMode != NetmodeID.Server) // Only display the message on the client that is fighting the boss to avoid spamming the chat in multiplayer
                {
                    Main.NewText("The void grants you infinite flight...", Color.MediumPurple);
                }
            }
            // Give targets infinite flight time.
            foreach (Player player in Main.ActivePlayers)
            {
                player.wingTime = player.wingTimeMax;
            }

            // Spawn projectiles every 35 ticks
            if (GlobalTimer % 35 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 spawnPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1500f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(0f, 0f), ModContent.ProjectileType<EldritchVoidP3Projectile>(), 40, 0f, Main.myPlayer);
                }
            }

            if (AttackTimer == 1) //switching out attacks
            {
                AttackPhase++;
                if (AttackPhase > 3)
                    AttackPhase = 1;
            }
            switch (AttackPhase) //Attacks
            {
                case 0: break;
                case 1: finalLaser(); break;
                case 2: if(AttackTimer > 180) ResetAttack(); break; // delay between attacks
                case 3: finalExplodingProjectiles(); break;
            }

        }
        private void Dying()
        {
        }
    }
}