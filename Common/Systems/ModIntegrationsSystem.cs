using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AncientRealms.Core.Systems
{
    public class ModIntegrationsSystem : ModSystem
    {
        public override void PostSetupContent() {
			DoBossChecklistIntegration();
		}

        private void DoBossChecklistIntegration()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod)) {
				return;
			}

            if (bossChecklistMod.Version < new Version(1, 6)) {
				return;
			}

            bossChecklistMod.Call(
				"LogBoss",
				Mod,
				"GateKeeper",
				2.7f,
				() => BossDownedSystem.downedGateKeeper,
				ModContent.NPCType<Content.Bosses.GateKeeper.GateKeeper>());

            bossChecklistMod.Call(
				"LogBoss",
				Mod,
				"EldritchVoid",
				7.8f,
				() => BossDownedSystem.downedEldritchVoid,
				ModContent.NPCType<Content.Bosses.EldritchVoid.EldritchVoid>());
        }
    }
}