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
    public sealed partial class ShroomCentipedeHead : ModNPC
    {
		private Vector2 targetDirection;
        public void ResetAttack()
		{
			AttackTimer = 0;
			targetDirection = Vector2.Zero;
			NPC.netUpdate = true;
		}
        private void RandomizeTarget()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			var Players = new List<int>();

			foreach (Player Player in Main.player.Where(n => n.active && !n.dead && arena.Contains(n.Center.ToPoint())))
			{
				Players.Add(Player.whoAmI);
			}

			int random = Main.rand.Next(Players.Count);

			if (random < Players.Count)
				NPC.target = Players[random];

			NPC.netUpdate = true;
		}

		private void DashAttack(int dashTimer, float dashSpeed = 38f)
		{
			if (dashTimer == 1)// Randomize target at the start of the dash
			{
				RandomizeTarget();
			}else if (dashTimer < 90)
			{
				targetDirection = Main.player[NPC.target].Center - NPC.Center;
				targetDirection.Normalize();
				NPC.rotation = targetDirection.ToRotation();
				NPC.velocity = targetDirection * 0.5f;
			} else
			{
				NPC.velocity = targetDirection * dashSpeed;
				NPC.rotation = NPC.velocity.ToRotation();
			}
		}
    }

    //Body
    public sealed partial class ShroomCentipedeBody : ModNPC
    {
        
    }

    //Tail
    public sealed partial class ShroomCentipedeTail : ModNPC
    {
        
    }
}