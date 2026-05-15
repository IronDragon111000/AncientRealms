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
using Terraria.Graphics;
using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Build.Evaluation;


namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public class EldritchVoidExplosiveMinion : ModNPC
    {
        public ref float Timer => ref NPC.ai[0];
        public override void SetDefaults()
        {
            NPC.width = 45;
            NPC.height = 45;
            NPC.scale = 2f;
            NPC.damage = 65;
            NPC.defense = 30;
            NPC.lifeMax = 90;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0.8f; 
            Main.npcFrameCount[NPC.type] = 1; 
            NPC.frame.Width = 44; 
            NPC.frame.Height = 45; 
        }

        public override void AI()
        {
            float turningPower = MathHelper.ToRadians(2.5f);
            float speed = NPC.velocity.Length();
            if(speed == 0)
            {
                NPC.velocity = Vector2.Normalize(NPC.Center) * 0.1f;
                return;
            }
            if(speed <= 15f)
                speed += 0.1f;
            UpdateAim(NPC.Center, turningPower, speed);
            Timer++;
            if(Timer % 6)
            {
                NPC.life -= NPC.lifeMax / 55;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(target, hurtInfo);
        }

        public override void OnKill()
        {
            for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player Player = Main.player[k];
                if(CollisionHelper.CheckCircularCollision(Projectile.Center, 200, Player.Hitbox))
                {
                    Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(k, Projectile.whoAmI), (int)damage, 0, false, false, -1, false);
                }
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo info)
        {
             npc.KillMe();
        }

        private void UpdateAim(Vector2 source, float turnSpeed, float Speed) {
            Player targetPlayer = null;
			foreach (Player Player in Main.player.Where(n => n.active && !n.dead))
			{
				if(targetPlayer == null || Vector2.Distance(Player.Center, NPC.Center) < Vector2.Distance(targetPlayer.Center, NPC.Center))
                    targetPlayer = Player;
			}
			// Get the player's current aiming direction as a normalized vector.
			Vector2 aim = Vector2.Normalize(targetPlayer.Center - source);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}

			// Calculate current and target angles
			float currentAngle = NPC.velocity.ToRotation();
			float targetAngle = aim.ToRotation();

			// Get the smallest angle difference
			float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);

			// Rotate by a constant amount towards the target, clamped to max speed
			float turnAmount = MathHelper.Clamp(angleDiff, -turnSpeed, turnSpeed);
			float newAngle = currentAngle + turnAmount;

			// Set new velocity
			NPC.velocity = newAngle.ToRotationVector2() * Speed;

			if (NPC.velocity != aim) {
				NPC.netUpdate = true;
			}
		}

        public override bool PreDraw(ref Color lightColor)
        {
            Color tellColor = Color.DarkMagenta;
            float tellOpacity = 0.05f + (0.35f * (Timer % NPC.life)/NPC.life) * (NPC.lifeMax / (3 * NPC.life + NPC.lifeMax));                
            Texture2D telegraphTexture = Request<Texture2D>("AncientRealms/Content/Bosses/EldritchVoid/EldritchVoidExplodingProjectileTell").Value;
		    Main.spriteBatch.Draw(telegraphTexture, NPC.Center - Main.screenPosition - new Vector2(telegraphTexture.Width / 2, telegraphTexture.Height / 2), default, tellColor * tellOpacity);
        }
    }
}