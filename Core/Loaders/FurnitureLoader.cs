﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
	public abstract class FurnitureLoader : ILoadable
    {
        private readonly string name = "Nameless";
        private readonly string path = "StarlightRiver/Tiles/Placeholders/";

        private readonly Color color = Color.White;
        private readonly Color glowColor = Color.Blue;
        private readonly int dust = DustID.Dirt;
        private readonly int material = ItemID.DirtBlock;

        public FurnitureLoader(string name, string path, Color color, Color glowColor, int dust, int material = ItemID.None)
        {
            this.name = name;
            this.path = path;
            this.color = color;
            this.glowColor = glowColor;
            this.dust = dust;
            this.material = material;
        }

        public float Priority { get => 1f; }

        public void Load()
        {
            var mod = StarlightRiver.Instance;

            Add("Bathtub", new GenericBathtub(color, dust, name + "Bathtub"), mod, 14);
            Add("Bed", new GenericBed(color, dust, name + "Bed"), mod, 15);
            Add("Bookcase", new GenericBookcase(color, dust, name + "Bookcase"), mod, 20);
            Add("Candelabra", new GenericCandelabra(glowColor, dust, name + "Candelabra"), mod, 5);
            Add("Candle", new GenericCandle(glowColor, dust, name + "Candle"), mod, 4);
            Add("Chair", new GenericChair(color, dust, name + "Chair"), mod, 4);
            Add("Chandelier", new GenericChandelier(glowColor, dust, name + "Chandelier"), mod, 4);
            Add("Clock", new GenericClock(color, dust, name + "Clock"), mod, 10);
            Add("Dresser", new GenericDresser(color, dust, name + "Dresser"), mod, 16);
            Add("Lamp", new GenericLamp(glowColor, dust, name + "Lamp"), mod, 3);
            Add("Lantern", new GenericLantern(glowColor, dust, name + "Lantern"), mod, 6);
            Add("Piano", new Generic3x2(color, dust, name + "Piano"), mod, 15);
            Add("Sink", new GenericSink(color, dust, name + "Sink"), mod, 6);
            Add("Sofa", new Generic3x2(color, dust, name + "Sofa"), mod, 5);
            Add("Table", new GenericSolidWithTop(color, dust, name + "Table"), mod, 8);
            Add("Workbench", new GenericWorkbench(color, dust, name + "Workbench"), mod, 10);

            //special stuff for the door
            mod.AddTile(name + "DoorClosed", new GenericDoorClosed(color, dust, name + "DoorClosed"), path + name + "DoorClosed");
            mod.AddTile(name + "DoorOpen", new GenericDoorOpen(color, dust, name + "DoorOpen"), path + name + "DoorOpen");
            mod.AddItem(name + "Door", new GenericFurnitureItem(name + " " + "DoorClosed", path + name + "DoorItem", 6, material));
        }

        public void Unload() { }

        private void Add(string typename, ModTile tile, Mod mod, int craftingQuantity)
        {
            mod.AddTile(name + typename, tile, path + name + typename);
            mod.AddItem(name + typename, new GenericFurnitureItem(name + " " + typename, path + name + typename + "Item", craftingQuantity, material));
        }
    }

    class GenericFurnitureItem : QuickTileItem
    {
        private readonly string name;
        private readonly int craftingQuantity;
        private readonly int craftingMaterial;
        private readonly string texture;

        public GenericFurnitureItem(string name, string texture, int craftingQuantity, int craftingMaterial) : base(name.Replace("Closed", ""), "", StarlightRiver.Instance.TileType(name.Replace(" ", "")), 0)
        {
            this.name = name;
            this.craftingQuantity = craftingQuantity;
            this.craftingMaterial = craftingMaterial;
            this.texture = texture;
        }

        public override bool CloneNewInstances => true;

        public override string Texture => texture;

        public override void SafeSetDefaults()
        {
            item.maxStack = 99;
            item.value = 30;
        }

        public override void AddRecipes()
        {
            if (craftingMaterial != ItemID.None)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(craftingMaterial, craftingQuantity);

                if (name.Contains("Candle") || name.Contains("Lamp") || name.Contains("Lantern") || name.Contains("Candelabra"))
                    recipe.AddIngredient(ItemID.Torch);

                if (name.Contains("Candelabra"))
                    recipe.AddIngredient(ItemID.Torch, 3);

                if (name.Contains("Chandelier"))
                {
                    recipe.AddIngredient(ItemID.Torch, 4);
                    recipe.AddIngredient(ItemID.Chain);
                }

                if (name.Contains("Bed"))
                    recipe.AddIngredient(ItemID.Silk, 5);

                if (name.Contains("Bookcase"))
                    recipe.AddIngredient(ItemID.Book, 10);

                if (name.Contains("Clock"))
                {
                    recipe.AddRecipeGroup(RecipeGroupID.IronBar, 3);
                    recipe.AddIngredient(ItemID.Glass, 6);
                }

                if (name.Contains("Piano"))
                {
                    recipe.AddIngredient(ItemID.Bone, 4);
                    recipe.AddIngredient(ItemID.Book);
                }

                if (name.Contains("Sink"))
                    recipe.AddIngredient(ItemID.WaterBucket);

                if (name.Contains("Sofa"))
                    recipe.AddIngredient(ItemID.Silk, 2);


                recipe.AddTile(TileID.WorkBenches);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }
    }

    abstract class Furniture : ModTile
    {
        protected readonly Color color;
        protected readonly int dust;
        protected readonly string name;

        public Furniture(Color color, int dust, string name)
        {
            this.color = color;
            this.dust = dust;
            this.name = name;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, mod.ItemType(name));
    }

    //Bathtub
    class GenericBathtub : Furniture
    {
        public GenericBathtub(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 4, 0);
            QuickBlock.QuickSetFurniture(this, 4, 2, dust, SoundID.Dig, false, color);
            adjTiles = new int[] { TileID.Bathtubs };
        }
    }

    //bed
    class GenericBed : Furniture
    {
        public GenericBed(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 4, 0);
            QuickBlock.QuickSetFurniture(this, 4, 2, dust, SoundID.Dig, false, color);
            TileID.Sets.HasOutlines[Type] = true;
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Beds };
            bed = true;
        }

        public override bool HasSmartInteract() => true;

        public override bool NewRightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int spawnX = i - tile.frameX / 18;
            int spawnY = j + 2;
            spawnX += tile.frameX >= 72 ? 5 : 2;
            if (tile.frameY % 38 != 0)
            {
                spawnY--;
            }
            player.FindSpawn();
            if (player.SpawnX == spawnX && player.SpawnY == spawnY)
            {
                player.RemoveSpawn();
                Main.NewText("Spawn point removed!", 255, 240, 20, false);
            }
            else if (Player.CheckSpawn(spawnX, spawnY))
            {
                player.ChangeSpawn(spawnX, spawnY);
                Main.NewText("Spawn point set!", 255, 240, 20, false);
            }
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType(this.name);
        }
    }

    //bookcase
    class GenericBookcase : Furniture
    {
        public GenericBookcase(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            TileObjectData.newTile.Origin = new Point16(0, 4);
            QuickBlock.QuickSetFurniture(this, 3, 4, dust, SoundID.Dig, false, color, true);
            adjTiles = new int[] { TileID.Bookcases };
        }
    }

    //Candelabra
    class GenericCandelabra : Furniture
    {
        public GenericCandelabra(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 2, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Candelabras };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            int newX = i;
            int newY = j;
            if (tile.frameX % 36 == 18) 
                newX = i - 1;
            if (tile.frameY % 36 == 18) 
                newY = j - 1;

            for (int k = 0; k < 2; k++)
                for (int l = 0; l < 2; ++l)
                {
                    Main.tile[newX + k, newY + l].frameX += (short)(Main.tile[newX + k, newY + l].frameX >= 36 ? -36 : 36);
                    Wiring.SkipWire(newX + k, newY + l);
                }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.frameX < 36) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Candle
    class GenericCandle : Furniture
    {
        public GenericCandle(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 1, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Candles };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            tile.frameX += (short)(tile.frameX >= 18 ? -18 : 18);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.frameX < 18) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Chair
    class GenericChair : Furniture
    {
        public GenericChair(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 2, dust, SoundID.Dig, false, color);

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
            adjTiles = new int[] { TileID.Chairs };
        }
    }

    //Chandelier
    class GenericChandelier : Furniture
    {
        public GenericChandelier(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 3, dust, SoundID.Dig, false, color, Origin: new Point16(1, 1));
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Chandeliers };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j); //Initial tile

            int newX = i - tile.frameX % 54 / 18; //Adjustments
            int newY = j - tile.frameY % 54 / 18;

            tile = Framing.GetTileSafely(newX, newY); //Top-left tile

            for (int k = 0; k < 3; k++) //Changes frames properly
            {
                for (int l = 0; l < 3; ++l)
                {
                    Main.tile[newX + k, newY + l].frameX += (short)(Main.tile[newX + k, newY + l].frameX >= 54 ? -54 : 54);
                    Wiring.SkipWire(newX + k, newY + l);
                }
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.frameX < 54) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Clock
    class GenericClock : Furniture
    {
        public GenericClock(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            TileObjectData.newTile.Origin = new Point16(0, 5);
            QuickBlock.QuickSetFurniture(this, 2, 5, dust, SoundID.Dig, false, color);
            TileID.Sets.HasOutlines[Type] = true;
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.GrandfatherClocks };
        }

        public override bool HasSmartInteract() => true;

        public override bool NewRightClick(int x, int y)
        {
            string text = "AM";
            //Get current weird time
            double time = Main.time;
            if (!Main.dayTime)
            {
                //if it's night add this number
                time += 54000.0;
            }
            //Divide by seconds in a day * 24
            time = time / 86400.0 * 24.0;
            //Dunno why we're taking 19.5. Something about hour formatting
            time = time - 7.5 - 12.0;
            //Format in readable time
            if (time < 0.0)
            {
                time += 24.0;
            }
            if (time >= 12.0)
            {
                text = "PM";
            }
            int intTime = (int)time;
            //Get the decimal points of time.
            double deltaTime = time - intTime;
            //multiply them by 60. Minutes, probably
            deltaTime = (int)(deltaTime * 60.0);
            //This could easily be replaced by deltaTime.ToString()
            string text2 = string.Concat(deltaTime);
            if (deltaTime < 10.0)
            {
                //if deltaTime is eg "1" (which would cause time to display as HH:M instead of HH:MM)
                text2 = "0" + text2;
            }
            if (intTime > 12)
            {
                //This is for AM/PM time rather than 24hour time
                intTime -= 12;
            }
            if (intTime == 0)
            {
                //0AM = 12AM
                intTime = 12;
            }
            //Whack it all together to get a HH:MM format
            var newText = string.Concat("Time: ", intTime, ":", text2, " ", text);
            Main.NewText(newText, 255, 240, 20);
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
                Main.clock = true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType(this.name);
        }
    }

    //Door
    class GenericDoorClosed : Furniture
    {
        public GenericDoorClosed(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 1);
            TileObjectData.addAlternate(0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 2);
            TileObjectData.addAlternate(0);

            QuickBlock.QuickSetFurniture(this, 1, 3, dust, SoundID.Dig, false, color, Origin: new Point16(0, 0));

            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.NotReallySolid[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

            disableSmartCursor = true;
            adjTiles = new int[] { TileID.ClosedDoor };
            openDoorID = mod.TileType(name.Replace("Closed", "Open"));
        }

        public override bool HasSmartInteract() => true;

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType(name.Replace("Closed", ""));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, mod.ItemType(name.Replace("Closed", "")));
    }

    //Open Door
    class GenericDoorOpen : Furniture
    {
        public GenericDoorOpen(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);

            TileObjectData.newTile.LavaDeath = true;

            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;

            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 1);
            TileObjectData.addAlternate(0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 2);
            TileObjectData.addAlternate(0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 0);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 1);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 2);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);

            QuickBlock.QuickSetFurniture(this, 2, 3, dust, SoundID.Dig, false, color, Origin: new Point16(0, 0));

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
            TileID.Sets.HousingWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            disableSmartCursor = true;
            adjTiles = new int[] { TileID.OpenDoor };
            closeDoorID = mod.TileType(name.Replace("Open", "Closed"));
        }

        public override bool HasSmartInteract() => true;

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType(name.Replace("Open", ""));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, mod.ItemType(name.Replace("Open", "")));
    }

    //Piano, Sofa
    class Generic3x2 : Furniture
    {
        public Generic3x2(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 2, dust, SoundID.Dig, false, color);

            if(name.Contains("Piano"))
                adjTiles = new int[] { TileID.Pianos };
        }
    }    

    //dresser
    class GenericDresser : Furniture
    {
        public GenericDresser(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileContainer[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            TileObjectData.newTile.HookCheck = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
            QuickBlock.QuickSetFurniture(this, 3, 2, dust, SoundID.Dig, false, color);
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Dressers };
            dresser = name.Substring(0, name.Length - 7) + " Dresser";
            dresserDrop = mod.ItemType(name);
        }

        public override bool HasSmartInteract() => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 48, 32, dresserDrop);
            Chest.DestroyChest(i, j);
        }

        public override bool NewRightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].frameY == 0)
            {
                Main.CancelClothesWindow(true);
                Main.mouseRightRelease = false;
                int left = (int)(Main.tile[Player.tileTargetX, Player.tileTargetY].frameX / 18);
                left %= 3;
                left = Player.tileTargetX - left;
                int top = Player.tileTargetY - (int)(Main.tile[Player.tileTargetX, Player.tileTargetY].frameY / 18);
                if (player.sign > -1)
                {
                    Main.PlaySound(SoundID.MenuClose);
                    player.sign = -1;
                    Main.editSign = false;
                    Main.npcChatText = string.Empty;
                }
                if (Main.editChest)
                {
                    Main.PlaySound(SoundID.MenuTick);
                    Main.editChest = false;
                    Main.npcChatText = string.Empty;
                }
                if (player.editedChestName)
                {
                    NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
                    player.editedChestName = false;
                }
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    if (left == player.chestX && top == player.chestY && player.chest != -1)
                    {
                        player.chest = -1;
                        Recipe.FindRecipes();
                        Main.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
                        Main.stackSplit = 600;
                    }
                }
                else
                {
                    player.flyingPigChest = -1;
                    int num213 = Chest.FindChest(left, top);
                    if (num213 != -1)
                    {
                        Main.stackSplit = 600;
                        if (num213 == player.chest)
                        {
                            player.chest = -1;
                            Recipe.FindRecipes();
                            Main.PlaySound(SoundID.MenuClose);
                        }
                        else if (num213 != player.chest && player.chest == -1)
                        {
                            player.chest = num213;
                            Main.playerInventory = true;
                            Main.recBigList = false;
                            Main.PlaySound(SoundID.MenuOpen);
                            player.chestX = left;
                            player.chestY = top;
                        }
                        else
                        {
                            player.chest = num213;
                            Main.playerInventory = true;
                            Main.recBigList = false;
                            Main.PlaySound(SoundID.MenuTick);
                            player.chestX = left;
                            player.chestY = top;
                        }
                        Recipe.FindRecipes();
                    }
                }
            }
            else
            {
                Main.playerInventory = false;
                player.chest = -1;
                Recipe.FindRecipes();
                Main.dresserX = Player.tileTargetX;
                Main.dresserY = Player.tileTargetY;
                Main.OpenClothesWindow();
            }
            return true;
        }

        public override void MouseOverFar(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            int left = Player.tileTargetX;
            int top = Player.tileTargetY;
            left -= (int)(tile.frameX % 54 / 18);
            if (tile.frameY % 36 != 0)
            {
                top--;
            }
            int chestIndex = Chest.FindChest(left, top);
            player.showItemIcon2 = -1;
            if (chestIndex < 0)
            {
                player.showItemIconText = Language.GetTextValue("LegacyDresserType.0");
            }
            else
            {
                if (Main.chest[chestIndex].name != "")
                {
                    player.showItemIconText = Main.chest[chestIndex].name;
                }
                else
                {
                    player.showItemIconText = chest;
                }
                if (player.showItemIconText == chest)
                {
                    player.showItemIcon2 = mod.ItemType(this.name);
                    player.showItemIconText = "";
                }
            }
            player.noThrow = 2;
            player.showItemIcon = true;
            if (player.showItemIconText == "")
            {
                player.showItemIcon = false;
                player.showItemIcon2 = 0;
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            int left = Player.tileTargetX;
            int top = Player.tileTargetY;
            left -= (int)(tile.frameX % 54 / 18);
            if (tile.frameY % 36 != 0)
            {
                top--;
            }
            int num138 = Chest.FindChest(left, top);
            player.showItemIcon2 = -1;
            if (num138 < 0)
            {
                player.showItemIconText = Language.GetTextValue("LegacyDresserType.0");
            }
            else
            {
                if (Main.chest[num138].name != "")
                {
                    player.showItemIconText = Main.chest[num138].name;
                }
                else
                {
                    player.showItemIconText = chest;
                }
                if (player.showItemIconText == chest)
                {
                    player.showItemIcon2 = mod.ItemType(this.name);
                    player.showItemIconText = "";
                }
            }
            player.noThrow = 2;
            player.showItemIcon = true;
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].frameY > 0)
            {
                player.showItemIcon2 = ItemID.FamiliarShirt;
            }
        }
    }

    //Lamp
    class GenericLamp : Furniture
    {
        public GenericLamp(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 3, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Lamps };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            int newY = j - tile.frameY % 54 / 18;

            tile = Framing.GetTileSafely(i, newY);

            for (int l = 0; l < 3; ++l)
            {
                Main.tile[i, newY + l].frameX += (short)(Main.tile[i, newY + l].frameX >= 18 ? -18 : 18);
                Wiring.SkipWire(i, newY + l);
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.frameX < 18 && tile.frameY == 0) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Lantern
    class GenericLantern : Furniture
    {
        public GenericLantern(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 2, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.HangingLanterns };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            int newY = j - tile.frameY % 36 / 18;
            //Main.NewText("O: " + tile.frameY % 36 / 18);

            tile = Framing.GetTileSafely(i, newY);

            //Main.NewText("G: " + tile.frameX);

            for (int l = 0; l < 2; l++)
            {
                Main.tile[i, newY + l].frameX += (short)(Main.tile[i, newY + l].frameX >= 18 ? -18 : 18);
                //Wiring.SkipWire(i, newY + l);
            }

            //Main.NewText("G: " + tile.frameX);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.frameX < 18 && tile.frameY == 18) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Sink
    class GenericSink : Furniture
    {
        public GenericSink(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 2, dust, SoundID.Dig, false, color);
            adjTiles = new int[] { TileID.Sinks };
        }
    }

    //Table
    class GenericSolidWithTop : Furniture
    {
        public GenericSolidWithTop(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 2, dust, SoundID.Dig, false, color, true);
            adjTiles = new int[] { TileID.Tables };
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        }
    }

    //Workbench
    class GenericWorkbench : Furniture
    {
        public GenericWorkbench(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 1, dust, SoundID.Dig, false, color, true);
            adjTiles = new int[] { TileID.WorkBenches };
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        }
    }
}