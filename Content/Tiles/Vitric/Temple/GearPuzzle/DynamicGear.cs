﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
    class DynamicGear : GearTile
    {
        public override int DummyType => ModContent.ProjectileType<DynamicGearDummy>();

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.showItemIcon2 = ModContent.ItemType<GearTilePlacer>();
			player.noThrow = 2;
			player.showItemIcon = true;
		}

		public override bool NewRightClick(int i, int j)
		{
			var dummy = (Dummy(i, j).modProjectile as GearTileDummy);

			var entity = TileEntity.ByPosition[new Point16(i, j)] as GearTileEntity;

			if (entity is null)
				return false;

			if (dummy is null || dummy.gearAnimation > 0)
				return false;

			entity.Disengage();

			dummy.oldSize = dummy.Size;
			dummy.Size++;
			dummy.gearAnimation = 40;

			GearPuzzleHandler.PuzzleOriginEntity?.Engage(2);

			return true;
		}
	}

    class DynamicGearDummy : GearTileDummy
    {
        public DynamicGearDummy() : base(ModContent.TileType<DynamicGear>()) { }

		public override void Update()
		{
			base.Update();

			Lighting.AddLight(projectile.Center, new Vector3(0.1f, 0.2f, 0.3f) * Size);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D pegTex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearPeg");
			spriteBatch.Draw(pegTex, projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			Texture2D tex;

			switch (Size)
			{
				case 0: tex = ModContent.GetTexture(AssetDirectory.Invisible); break;
				case 1: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
				case 2: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearMid"); break;
				case 3: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearLarge"); break;
				default: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
			}

			if (gearAnimation > 0) //switching between sizes animation
			{
				Texture2D texOld;

				switch (oldSize)
				{
					case 0: texOld = ModContent.GetTexture(AssetDirectory.Invisible); break;
					case 1: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
					case 2: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearMid"); break;
					case 3: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearLarge"); break;
					default: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
				}

				if (gearAnimation > 20)
				{
					float progress = Helpers.Helper.BezierEase((gearAnimation - 20) / 20f);
					spriteBatch.Draw(texOld, projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, texOld.Size() / 2, progress, 0, 0);
				}
				else
				{
					float progress = Helpers.Helper.SwoopEase(1 - gearAnimation / 20f);
					spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, tex.Size() / 2, progress, 0, 0);
				}

				return;
			}

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class GearTilePlacer : QuickTileItem
	{
		public GearTilePlacer() : base("Gear puzzle", "Debug item", ModContent.TileType<DynamicGear>(), 8, AssetDirectory.VitricTile) { }
	}
}
