﻿using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Misc
{
    public class ImpulseThruster : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        private bool releaseJump = false;

        public ImpulseThruster() : base("Impulse Thruster", "Converts all wingtime into a burst of energy") { }

        public override void SafeSetDefaults()
        {
            item.value = Item.sellPrice(0, 2, 0, 0);
            item.rare = ItemRarityID.Pink;
        }

        public override void SafeUpdateEquip(Player player)
        {
            if (player.controlJump && player.wingTime > 0)
            {
                Vector2 dir = new Vector2(0, -1);
                if (player.controlDown)
                    dir.Y = 1;
                if (player.controlUp)
                    dir.Y = -1;
                if (player.controlLeft)
                    dir.X = -1;
                if (player.controlRight)
                    dir.X = 1;

                if (!player.releaseJump)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(player.Center - new Vector2(8, 8), 0, 0, ModContent.DustType<ImpulseThrusterDustOne>());
                        dust.velocity = -dir.RotatedByRandom(0.4f) * Main.rand.NextFloat(20);
                        dust.scale = Main.rand.NextFloat(1.2f, 1.9f);
                        dust.alpha = Main.rand.Next(70);
                        dust.rotation = Main.rand.NextFloat(6.28f);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 projDir = dir.RotatedByRandom(1.4f);
                        Projectile.NewProjectileDirect(player.Center - (projDir * 20), projDir * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<ThrusterEmber>(), 0, 0, player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
                    }
                }

                dir.Normalize();

                float mult = MathHelper.Clamp((float)Math.Pow(player.wingTime, 0.7f) * 3, 1, 70);
                dir *= mult;
                player.velocity = dir;
                player.wingTime = 0;
                releaseJump = true;
            }

            if ((player.velocity.Y == 0 || player.grappling[0] > -1) && !player.controlJump)
            {
                releaseJump = false;
            }
        }
    }

    public class ThrusterEmber : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thruster");
        }

        public override void SetDefaults()
        {
            projectile.penetrate = 1;
            projectile.tileCollide = true;
            projectile.hostile = false;
            projectile.friendly = false;
            projectile.aiStyle = 1;
            projectile.width = projectile.height = 12;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            projectile.extraUpdates = 1;
            projectile.alpha = 255;
        }

        public override void AI()
        {
            projectile.scale *= 0.98f;
            if (Main.rand.Next(2) == 0)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ImpulseThrusterDustTwo>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
                dust.scale = 0.6f * projectile.scale;
                dust.rotation = Main.rand.NextFloatDirection();
            }
        }
    }

    public class ImpulseThrusterDustOne : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 60)
            {
                ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 60f);
            }
            else if (dust.alpha < 140)
            {
                ret = Color.Lerp(Color.Orange, Color.OrangeRed, (dust.alpha - 60) / 80f);
            }
            else if (dust.alpha < 200)
            {
                ret = Color.Lerp(Color.OrangeRed, gray, (dust.alpha - 140) / 80f);
            }
            else
                ret = gray;
            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.velocity.Length() > 6)
                dust.velocity *= 0.92f;
            else
                dust.velocity *= 0.96f;
            if (dust.alpha > 100)
            {
                dust.scale += 0.01f;
                dust.alpha += 2;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 4;
            }
            dust.position += dust.velocity;
            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }

    public class ImpulseThrusterDustTwo : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 40)
                ret = Color.Lerp(Color.Yellow, Color.OrangeRed, dust.alpha / 40f);
            else if (dust.alpha < 80)
                ret = Color.Lerp(Color.OrangeRed, gray, (dust.alpha - 40) / 40f);
            else
                ret = gray;

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;

            if (dust.alpha > 60)
            {
                dust.scale += 0.01f;
                dust.alpha += 6;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 4;
            }

            dust.position += dust.velocity;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }
}