﻿using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    public abstract class GeoProj : ModProjectile
    {
        protected const float bigScale = 1.4f;
        protected const int STARTOFFSET = 45;
        protected float offsetLerper = 1;

        protected float glowCounter;

        protected float whiteCounter;

        protected float fade = 1;

        protected bool released = false;
        protected float releaseCounter = 0;
        protected float extraSpin = 0f;
        public override void SetDefaults()
        {
            projectile.friendly = false;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(16, 16);
            projectile.penetrate = -1;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
        public override void AI()
        {
            if (projectile.scale == bigScale)
                glowCounter += 0.02f;

            SafeAI();
            projectile.rotation = 0f;
            projectile.Center = Main.player[projectile.owner].Center + ((projectile.ai[0] + (Main.GlobalTime * 0.5f) + extraSpin).ToRotationVector2() * MathHelper.Lerp(0, STARTOFFSET, EaseFunction.EaseCubicOut.Ease(offsetLerper)));

            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            projectile.timeLeft = 2;
            if ((modPlayer.DiamondStored && modPlayer.RubyStored && modPlayer.EmeraldStored && modPlayer.SapphireStored && modPlayer.TopazStored && modPlayer.AmethystStored) || released)
            {
                if (whiteCounter < 1)
                    whiteCounter += 0.007f;

                releaseCounter += 0.01f;
                extraSpin += Math.Min(releaseCounter, 0.15f);
                released = true;
                if (releaseCounter > 0.5f)
                {
                    offsetLerper -= 0.015f;
                }
                if (offsetLerper <= 0)
                {
                    Destroy();
                    projectile.active = false;
                    modPlayer.AmethystStored = false;
                    modPlayer.TopazStored = false;
                    modPlayer.EmeraldStored = false;
                    modPlayer.SapphireStored = false;
                    modPlayer.RubyStored = false;
                    modPlayer.DiamondStored = false;

                    modPlayer.storedGem = StoredGem.All;
                    modPlayer.allTimer = 400;

                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat(6.28f);
                        Dust dust = Dust.NewDustPerfect(Main.player[projectile.owner].Center + (angle.ToRotationVector2() * 20), ModContent.DustType<GeoRainbowDust>());
                        dust.scale = 1f;
                        dust.velocity = angle.ToRotationVector2() * Main.rand.NextFloat() * 4;
                    }

                }
            }
            if (!modPlayer.SetBonusActive)
                projectile.active = false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            if (projectile.scale == bigScale)
            {
                float progress = glowCounter % 1;
                float transparency = (float)Math.Pow(1 - progress, 2);
                float scale = 0.95f + progress;

                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null, Color.White * transparency, projectile.rotation, tex.Size() / 2, scale * projectile.scale, SpriteEffects.None, 0f);
            }

            float progress2 = 1 - fade;
            float transparency2 = (float)Math.Pow(1 - progress2, 2);
            float scale2 = 0.95f + progress2;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null, Color.White * transparency2, projectile.rotation, tex.Size() / 2, projectile.scale * scale2, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (!released)
                return;
            Texture2D tex = ModContent.GetTexture(Texture + "_White");

            float progress2 = 1 - fade;
            float transparency2 = (float)Math.Pow(1 - progress2, 2);
            float scale2 = 0.95f + progress2;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null, Color.White * whiteCounter * transparency2, projectile.rotation, tex.Size() / 2, projectile.scale * scale2, SpriteEffects.None, 0f);
        }

        protected virtual void SafeAI() { }

        protected virtual void Destroy() { }
    }

    public class GeoAmethystProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoAmethyst";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();

            if (modPlayer.storedGem == StoredGem.Amethyst && !released)
            {
                fade = Math.Min(1, modPlayer.timer / 40f);
                if (modPlayer.timer == 1)
                {
                    modPlayer.AmethystStored = false;
                    projectile.active = false;
                    GeomancerPlayer.PickOldGem(Main.player[projectile.owner]);
                    modPlayer.timer = 1200;
                }
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 0.75f;
            }
        }
    }

    public class GeoRubyProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Ruby && !released)
            {
                fade = Math.Min(1, modPlayer.timer / 40f);
                if (modPlayer.timer == 1)
                {
                    modPlayer.RubyStored = false;
                    projectile.active = false;
                    GeomancerPlayer.PickOldGem(Main.player[projectile.owner]);
                    modPlayer.timer = 1200;
                }
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 0.75f;
            }
        }
    }
    public class GeoSapphireProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoSapphire";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Sapphire && !released)
            {
                fade = Math.Min(1, modPlayer.timer / 40f);
                if (modPlayer.timer == 1)
                {
                    modPlayer.SapphireStored = false;
                    projectile.active = false;
                    GeomancerPlayer.PickOldGem(Main.player[projectile.owner]);
                    modPlayer.timer = 1200;
                }
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 0.75f;
            }
        }
    }
    public class GeoEmeraldProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoEmerald";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Emerald && !released)
            {
                fade = Math.Min(1, modPlayer.timer / 40f);
                if (modPlayer.timer == 1)
                {
                    modPlayer.EmeraldStored = false;
                    projectile.active = false;
                    GeomancerPlayer.PickOldGem(Main.player[projectile.owner]);
                    modPlayer.timer = 1200;
                }
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 0.75f;
            }
        }
    }

    public class GeoTopazProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoTopaz";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Topaz && !released)
            {
                fade = Math.Min(1, modPlayer.timer / 40f);
                if (modPlayer.timer == 1)
                {
                    modPlayer.TopazStored = false;
                    projectile.active = false;
                    GeomancerPlayer.PickOldGem(Main.player[projectile.owner]);
                    modPlayer.timer = 1200;
                }
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 0.88f;
            }
        }

        protected override void Destroy()
        {
            Player player = Main.player[projectile.owner];
        }
    }

    public class GeoDiamondProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoDiamond";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Diamond && !released)
            {
                fade = Math.Min(1, modPlayer.timer / 40f);
                if (modPlayer.timer == 1)
                {
                    modPlayer.DiamondStored = false;
                    projectile.active = false;
                    GeomancerPlayer.PickOldGem(Main.player[projectile.owner]);
                    modPlayer.timer = 1200;
                }
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 0.75f;
            }
        }
    }
}