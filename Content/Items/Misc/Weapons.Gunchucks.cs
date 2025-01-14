using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Misc
{
	public class Gunchucks : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private int combo;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gunchucks");
		}

		public override void SetDefaults()
		{
			item.width = 16;
			item.height = 16;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 16;
			item.useAnimation = 16;
			item.shootSpeed = 1f;
			item.knockBack = 4f;
			item.UseSound = SoundID.Item116;
			item.shoot = ModContent.ProjectileType<GunchuckProj>();
			item.value = Item.sellPrice(gold: 4);
			item.noMelee = true;
			item.noUseGraphic = true;
			item.channel = true;
			item.autoReuse = true;
			item.ranged = true;
			item.damage = 16;
			item.useAmmo = AmmoID.Bullet;
			item.rare = ItemRarityID.Green;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			combo++;
			float distanceMult = Main.rand.NextFloat(0.8f, 1.2f);
			float curvatureMult = 0.7f;

			Vector2 direction = Vector2.Normalize(new Vector2(speedX, speedY).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f))) * 7;
			Projectile proj = Projectile.NewProjectileDirect(position, direction, ModContent.ProjectileType<GunchuckProj>(), damage, knockBack, player.whoAmI);
			if (proj.modProjectile is GunchuckProj modProj)
			{
				modProj.Flip = combo % 2 == 0;
				modProj.SwingTime = (int)(item.useTime * 2f);
				modProj.SwingDistance = 20 * distanceMult;
				modProj.Curvature = 0.33f * curvatureMult;
				modProj.Ammo = type;
				modProj.FireTime = Main.rand.NextFloat(0.55f, 0.65f);
			}

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
			return false;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Chain, 10);
			recipe.AddIngredient(ItemID.Boomstick, 2);
			recipe.AddTile(TileID.Anvils);

			recipe.SetResult(this);

			recipe.AddRecipe();
		}

		public override float UseTimeMultiplier(Player player) => base.UseTimeMultiplier(player) * player.meleeSpeed; //Scale with melee speed buffs, like whips
	}

	public class GunchuckProj : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

		public const float THROW_RANGE = 120; //Peak distance from player when thrown out, in pixels

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sanguine Flayer");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.Size = new Vector2(85, 85);
			projectile.tileCollide = false;
			projectile.ownerHitCheck = true;
			projectile.ignoreWater = true;
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
		}

		private Player Owner => Main.player[projectile.owner];

		public int SwingTime;
		public float SwingDistance;
		public float Curvature;
		public bool Flip;
		public int Ammo;
		public float FireTime;

		public Vector2 CurrentBase = Vector2.Zero;

		private bool shot = false;
		private float newRotation = 0f;

		public ref float Timer => ref projectile.ai[0];
		public ref float AiState => ref projectile.ai[1];

		public override void AI()
		{
			if (projectile.timeLeft > 2) //Initialize chain control points on first tick, in case of projectile hooking in on first tick
			{
				_chainMidA = projectile.Center;
				_chainMidB = projectile.Center;
			}

			projectile.timeLeft = 2;

			ThrowOutAI();

			float progress = Timer / SwingTime;
			progress = EaseFunction.EaseQuadOut.Ease(progress);
			if (progress > FireTime && !shot)
            {
				Texture2D projTexture = Main.projectileTexture[projectile.type];

				Vector2 projBottom = projectile.Center - new Vector2(projTexture.Width / 2, 0).RotatedBy(projectile.rotation + MathHelper.PiOver2) * 0.75f;

				float angleMaxDeviation = MathHelper.Pi * 0.85f;
				float angleOffset = Owner.direction * (Flip ? -1 : 1) * MathHelper.Lerp(angleMaxDeviation, -angleMaxDeviation / 4, progress);

				_chainMidA = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset) * Curvature;
				_chainMidB = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset / 2) * Curvature * 2.5f;

				BezierCurve curve = new BezierCurve(new Vector2[] { Owner.MountedCenter, _chainMidA, _chainMidB, projBottom });

				int numPoints = 10; //Should make dynamic based on curve length, but I'm not sure how to smoothly do that while using a bezier curve
				Vector2[] chainPositions = curve.GetPoints(numPoints).ToArray();

				newRotation = (projBottom - chainPositions[chainPositions.Length - 2]).ToRotation();

				shot = true;
				Vector2 direction = newRotation.ToRotationVector2();
				for (int i = 0; i < 5; i++)
                {
					Projectile.NewProjectile(projectile.Center + (direction * 25), direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(15, 20), Ammo, projectile.damage, projectile.knockBack, Owner.whoAmI);
                }
				
				Helper.PlayPitched("Guns/Scrapshot", 0.4f, 0, projectile.Center);

				float spread = 0.4f;
				for (int k = 0; k < 15; k++)
				{
					var dustDirection = direction.RotatedByRandom(spread);

					Dust.NewDustPerfect(projectile.Center + (direction * 25), ModContent.DustType<Dusts.Glow>(), dustDirection * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}
			}

			Owner.itemRotation = MathHelper.WrapAngle(Owner.AngleTo(Main.MouseWorld) - (Owner.direction < 0 ? MathHelper.Pi : 0));
		}

		private Vector2 GetSwingPosition(float progress)
		{
			//Starts at owner center, goes to peak range, then returns to owner center
			float distance = MathHelper.Clamp(SwingDistance, THROW_RANGE * 0.1f, THROW_RANGE) * MathHelper.Lerp((float)Math.Sin(progress * MathHelper.Pi), 1, 0.04f);

			float angleMaxDeviation = MathHelper.Pi / 1.2f;
			float angleOffset = Owner.direction * (Flip ? -1 : 1) * MathHelper.Lerp(-angleMaxDeviation, angleMaxDeviation, progress); //Moves clockwise if player is facing right, counterclockwise if facing left
			return projectile.velocity.RotatedBy(angleOffset) * distance;
		}

		private void ThrowOutAI()
		{
			projectile.rotation = projectile.AngleFrom(Owner.Center);
			Vector2 position = Owner.MountedCenter;
			float progress = ++Timer / SwingTime; //How far the projectile is through its swing
			progress = EaseFunction.EaseQuadOut.Ease(progress);


			projectile.Center = position + GetSwingPosition(progress);
			projectile.direction = projectile.spriteDirection = -Owner.direction * (Flip ? -1 : 1);

			if (Timer >= SwingTime)
				projectile.Kill();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (projectile.timeLeft > 2)
				return false;

			Texture2D projTexture = Main.projectileTexture[projectile.type];

			//End control point for the chain
			Vector2 projBottom = projectile.Center + new Vector2(projTexture.Width / 2, 0).RotatedBy(projectile.rotation + MathHelper.PiOver2) * 0.75f;
			DrawChainCurve(spriteBatch, projBottom, out Vector2[] chainPositions);

			//Adjust rotation to face from the last point in the bezier curve
			newRotation = (projBottom - chainPositions[chainPositions.Length - 2]).ToRotation();

			//Draw from bottom center of texture
			Vector2 origin = new Vector2(0, projTexture.Height / 2);
			SpriteEffects flip = (projectile.spriteDirection < 0) ? SpriteEffects.FlipVertically : SpriteEffects.None;

			lightColor = Lighting.GetColor((int)(projectile.Center.X / 16f), (int)(projectile.Center.Y / 16f));

			spriteBatch.Draw(projTexture, projBottom - Main.screenPosition, null, lightColor, newRotation, origin, projectile.scale, flip, 0);


			CurrentBase = projBottom + (newRotation - 1.57f).ToRotationVector2() * (projTexture.Height / 2);

			return false;
		}

		//Control points for drawing chain bezier, update slowly when hooked in
		private Vector2 _chainMidA;
		private Vector2 _chainMidB;
		private void DrawChainCurve(SpriteBatch spriteBatch, Vector2 projBottom, out Vector2[] chainPositions)
		{
			Texture2D chainTex = ModContent.GetTexture(Texture + "_Chain");

			float progress = Timer / SwingTime;

				progress = EaseFunction.EaseQuadOut.Ease(progress);

			float angleMaxDeviation = MathHelper.Pi * 0.85f;
			float angleOffset = Owner.direction * (Flip ? -1 : 1) * MathHelper.Lerp(angleMaxDeviation, -angleMaxDeviation / 4, progress);

			_chainMidA = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset) * Curvature;
			_chainMidB = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset / 2) * Curvature * 2.5f;

			BezierCurve curve = new BezierCurve(new Vector2[] { Owner.MountedCenter, _chainMidA, _chainMidB, projBottom });

			int numPoints = 10; //Should make dynamic based on curve length, but I'm not sure how to smoothly do that while using a bezier curve
			chainPositions = curve.GetPoints(numPoints).ToArray();

			//Draw each chain segment, skipping the very first one, as it draws partially behind the player
			for (int i = 1; i < numPoints; i++)
			{
				Vector2 position = chainPositions[i];

				float rotation = (chainPositions[i] - chainPositions[i - 1]).ToRotation() - MathHelper.PiOver2; //Calculate rotation based on direction from last point
				float yScale = Vector2.Distance(chainPositions[i], chainPositions[i - 1]) / chainTex.Height; //Calculate how much to squash/stretch for smooth chain based on distance between points

				Vector2 scale = new Vector2(1, yScale); // Stretch/Squash chain segment
				Color chainLightColor = Lighting.GetColor((int)position.X / 16, (int)position.Y / 16); //Lighting of the position of the chain segment
				Vector2 origin = new Vector2(chainTex.Width / 2, chainTex.Height); //Draw from center bottom of texture
				spriteBatch.Draw(chainTex, position - Main.screenPosition, null, chainLightColor, rotation, origin, scale, SpriteEffects.None, 0);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			BezierCurve curve = new BezierCurve(new Vector2[] { Owner.MountedCenter, _chainMidA, _chainMidB, projectile.Center });

			int numPoints = 32;
			Vector2[] chainPositions = curve.GetPoints(numPoints).ToArray();
			float collisionPoint = 0;
			for (int i = 1; i < numPoints; i++)
			{
				Vector2 position = chainPositions[i];
				Vector2 previousPosition = chainPositions[i - 1];
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, previousPosition, 6, ref collisionPoint))
					return true;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
	}
}