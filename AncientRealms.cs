global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using AncientRealms.Common;
global using AncientRealms.Helpers;
global using Terraria;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;

namespace AncientRealms
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class AncientRealms : Mod
	{
		public static AncientRealms Instance { get; set; }
		public AncientRealms()
		{
			Instance = this;
		}
	}
}
