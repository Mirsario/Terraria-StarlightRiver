﻿using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Breacher
{
    [AutoloadEquip(EquipType.Head)]
    public class BreacherHead : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherHead";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Visor");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class BreacherChest : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherChest";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Chestplate");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
        }


        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<BreacherHead>() && legs.type == ModContent.ItemType<BreacherLegs>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "A breacher drone follows you \nDouble tap down to call an airstrike on a nearby enemy";
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SpotterDrone>()] < 1)
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<SpotterDrone>(), (int)(30 * player.rangedDamage), 1.5f, player.whoAmI);
            }
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class BreacherLegs : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherLegs";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Leggings");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
        }
    }

    public class SpotterDrone : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public int ScanTimer = 0;

        public const int ScanTime = 230;

        public bool CanScan => ScanTimer <= 0;

        public int Charges = 0;

        float timer = 0;

        const float attackRange = 800;

        private NPC target;

        private int attackDelay;

        private int targetHeight
        {
            get
            {
                if (target == null || !target.active)
                    return 0;
                else
                    return (int)(target.height * 2.5f);
            }
        }

        private Vector2 targetPos => Vector2.Lerp(target.Bottom, target.Top, 0.5f + ((float)Math.Cos((ScanTimer - 100) * 6.28f / (float)(ScanTime - 100)) / 2f));
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Granitech Drone");
            Main.projPet[projectile.type] = true;
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.netImportant = true;
            projectile.width = 20;
            projectile.height = 20;
            projectile.friendly = false;
            projectile.minion = true;
            projectile.minionSlots = 0;
            projectile.penetrate = -1;
            projectile.timeLeft = 216000;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            timer += 0.1f;
            if (ScanTimer <= 0)
            {
                IdleMovement(player);
                Vector2 direction = player.Center - projectile.Center;
                projectile.rotation = direction.ToRotation() + 3.14f;
            }
            else
                AttackMovement(player);
        }

        private void IdleMovement(Entity entity)
        {
            Vector2 toEntity = (entity.Center - new Vector2((entity.width + 50) * entity.direction, entity.height + 50)) - projectile.Center;
            toEntity.Normalize();
            toEntity *= 10;
            projectile.velocity = Vector2.Lerp(projectile.velocity, toEntity, 0.05f);
        }

        private void AttackMovement(Player player)
        {
            if (ScanTimer == ScanTime)
            {
                NPC testtarget = Main.npc.Where(n => n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < attackRange).OrderBy(n => Vector2.Distance(n.Center, Main.MouseWorld)).FirstOrDefault();

                if (testtarget == default)
                    IdleMovement(player);
                else
                {
                    target = testtarget;
                    ScanTimer--;
                }
            }
            else 
            {
                if (!target.active || target == null)
                {
                    ScanTimer = ScanTime;
                    return;
                }
                if (ScanTimer > Charges)
                {
                    if (ScanTimer < 150)
                        player.GetModPlayer<StarlightPlayer>().Shake = (int)MathHelper.Lerp(0, 2, 1 - ((float)ScanTimer / 150f));
                    if (ScanTimer == 125)
                        Helper.PlayPitched("AirstrikeIncoming", 0.6f, 0);
                    ScanTimer--;
                }
                else
                {
                    if (attackDelay == 0)
                        SummonStrike();
                    attackDelay--;
                }
                IdleMovement(target);
                Vector2 direction = targetPos - projectile.Center;
                projectile.rotation = direction.ToRotation() + 3.14f;
            }
        }

        private void SummonStrike()
        {
            attackDelay = 6;
            Vector2 direction = new Vector2(0, -1);
            direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            Projectile.NewProjectile(target.Center + (direction * 800), direction * -10, ModContent.ProjectileType<OrbitalStrike>(), projectile.damage, projectile.knockBack, projectile.owner);
            Charges--;
        }
        public void DrawPrimitives()
        {
            if (ScanTimer == ScanTime || ScanTimer <= 100)
                return;

            Vector2[] target = new Vector2[3];
            Vector2[] source = new Vector2[3];
            if (projectile.Center.X < targetPos.X)
            {
                target[0] = projectile.Center - Main.screenPosition;
                source[0] = Vector2.Zero * 32;

                target[1] = targetPos - Main.screenPosition;
                source[1] = new Vector2(1, 1) * 32;

                target[2] = new Vector2(Vector2.Lerp(projectile.Center, targetPos, 0.5f).X, targetPos.Y) - Main.screenPosition;
                source[2] = new Vector2(0.5f, 1) * 32;
            }
            else
            {
                target[0] = projectile.Center - Main.screenPosition;
                source[0] = new Vector2(1, 0) * 32;

                target[2] = targetPos - Main.screenPosition;
                source[2] = new Vector2(0, 1) * 32;

                target[1] = new Vector2(Vector2.Lerp(projectile.Center, targetPos, 0.5f).X, targetPos.Y) - Main.screenPosition;
                source[1] = new Vector2(0.5f, 1) * 32;
            }

            Texture2D tex = ModContent.GetTexture("StarlightRiver/Assets/Items/Breacher/BreacherLaserPixel");

            DrawHelper.DrawTriangle(tex, target, source);
        }
    }
    internal class OrbitalStrike : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        private bool hit = false;

        private float Alpha => hit ? (projectile.timeLeft / 50f) : 1;
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 20;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = 1;
            projectile.timeLeft = 300;
            projectile.extraUpdates = 4;
            projectile.scale = 0.6f;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbital Strike");
            Main.projFrames[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }
        public override void AI()
        {
            if (!hit)
                ManageCaches();
            ManageTrail();
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 100; i++)
                {
                    cache.Add(projectile.Center);
                }
            }
            cache.Add(projectile.oldPos[0] + new Vector2(projectile.width / 2, projectile.height / 2));

            while (cache.Count > 100)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(11, 22, factor), factor =>
            {
                return Color.Cyan;
            });
            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(6, 12, factor), factor =>
            {
                return Color.White;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = projectile.Center;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["alpha"].SetValue(Alpha);

            trail?.Render(effect);

            trail2?.Render(effect);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            Color color = Color.Cyan;
            color.A = 0;
            Color color2 = Color.White;
            color2.A = 0;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null,
                             color * Alpha * 0.33f, projectile.rotation, tex.Size() / 2, projectile.scale * 2, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null,
                             color * Alpha, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null,
                             color2 * Alpha, projectile.rotation, tex.Size() / 2, projectile.scale * 0.75f, SpriteEffects.None, 0);
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake += 9;
            projectile.friendly = false;
            projectile.penetrate++;
            hit = true;
            projectile.timeLeft = 50;
            projectile.extraUpdates = 3;
            projectile.velocity = Vector2.Zero;

            Explode();
        }
        private void Explode()
        {
            Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(projectile.Center + new Vector2(20, 70), ModContent.DustType<BreacherDustThree>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(12, 26), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.7f, 0.9f));
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<BreacherDustTwo>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(8), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.1f, 0.2f));
            }
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<OrbitalStrikeRing>(), projectile.damage, projectile.knockBack, projectile.owner);
        }
    }
    internal class OrbitalStrikeRing : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;
        public override string Texture => AssetDirectory.BreacherItem + "OrbitalStrike";

        private float Progress => 1 - (projectile.timeLeft / 10f);

        private float Radius => 66 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbital Strike");
        }

        public override void AI()
        {

            ManageCaches();
            ManageTrail();
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;
            for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
            {
                return Color.Cyan;
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 33f / 32f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = projectile.Center + offset;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
    public class BreacherPlayer : ModPlayer
    {
        public const int CHARGETIME = 150;

        public int ticks;
        public int Charges => ticks / CHARGETIME;

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && ticks < CHARGETIME * 5)
                ticks += CHARGETIME / 3;
            base.OnHitNPCWithProj(proj, target, damage, knockback, crit);
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && ticks < CHARGETIME * 5)
                ticks += CHARGETIME / 3;
            base.OnHitNPC(item, target, damage, knockback, crit);
        }
    }
}