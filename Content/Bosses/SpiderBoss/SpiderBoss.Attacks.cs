
namespace AncientRealms.Content.Bosses.SpiderBoss
{
    public sealed partial class SpiderBoss : ModNPC
    {
		internal Vector2 TargetPosition;
        public void ResetAttack()
		{
			AttackTimer = 0;
			NPC.netUpdate = true;
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

		private void DashAtPlayer(Player target, float speed, int timer, int tellTime = 60)
		{
			if (target == null)
				return;

			if(timer < tellTime)
			{
				TargetPosition = target.Center; // Set the target position to the player's current position at the start of the dash
				TargetPosition += target.velocity * 15; // Lead the target by adding a portion of their velocity to the target position
				// Adds to the magnitude of the target position vector to make the boss overshoot the target for a more aggressive dash
				Vector2 direction1 = TargetPosition - NPC.Center; 
				direction1.Normalize();
				TargetPosition += direction1 * 450f; // Overshoot by adding a fixed distance in the direction of the dash
				NPC.netUpdate = true; // Sync the target position with clients
			}
			Vector2 direction = TargetPosition - NPC.Center;
			if (direction.Length()  < 10f) // If the boss is close enough to the target position, stop moving to prevent jittery movement
			{
				NPC.velocity = Vector2.Zero;
				return;
			}
			direction.Normalize();
			NPC.rotation = direction.ToRotation() + MathHelper.PiOver2;
			if(timer > tellTime + 5)
				NPC.velocity = direction * speed;
		}
    }
}