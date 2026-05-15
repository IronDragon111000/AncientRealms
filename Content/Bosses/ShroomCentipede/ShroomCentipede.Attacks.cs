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
        public void ResetAttack()
		{
			AttackTimer = 0;
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