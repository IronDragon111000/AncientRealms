
namespace AncientRealms.Content.Bosses.SpiderBoss
{
    public sealed partial class SpiderBoss : ModNPC
    {
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
    }
}