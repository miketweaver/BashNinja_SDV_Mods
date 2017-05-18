using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using System.Collections.Generic;
using StardewValley.Locations;
using System.Linq;

namespace Stardewponics
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
		** Properties
		*********/
        /// <summary>The tractor garage's building type.</summary>
        private readonly string GarageBuildingType = "Aquaponics";

        /// <summary>The tractor's NPC name.</summary>
        //private readonly string TractorName = "Tractor";

        /// <summary>The current player's farm.</summary>
        private Farm Farm;

        private bool IsNewDay;

        /*********
		** Public methods
		*********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            MenuEvents.MenuChanged += this.MenuAddInBuilding;

            // spawn tractor & remove it before save
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            //LocationEvents.CurrentLocationChanged += this.LocationEvents_CurrentLocationChanged;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;

        }


        /*********
		** Private methods
		*********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>

        //		private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        private void LocationEvents_CurrentLocationChanged(object sender, EventArgs eventArgs)
        {

            // spawn tractor house & tractor
            //			if (this.IsNewDay && e.NewLocation == this.Farm)
            if (this.IsNewDay)

            {
                this.RestoreCustomData();
                this.IsNewDay = false;
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs eventArgs)
        {
            this.StashCustomData();
        }


        /// <summary>Stash all tractor and garage data to a separate file to avoid breaking the save file.</summary>
        private void StashCustomData()
        {
            // back up garages
            Building[] garages = this.GetGreenhouses(this.Farm).ToArray();
            CustomSaveData saveData = new CustomSaveData(garages);
            this.Helper.WriteJsonFile($"data/{Constants.SaveFolderName}.json", saveData);

            // remove tractors + buildings
            foreach (Building garage in garages)
                this.Farm.destroyStructure(garage);
            //foreach (GameLocation location in Game1.locations)
            //	this.RemoveEveryCharactersOfType<Tractor>(location);
        }


        /// <summary>Restore tractor and garage data removed by <see cref="StashCustomData"/>.</summary>
        private void RestoreCustomData()
        {
            Monitor.Log("RestoreCustomData");
            // get save data
            CustomSaveData saveData = this.Helper.ReadJsonFile<CustomSaveData>($"data/{Constants.SaveFolderName}.json");

            if (saveData?.Buildings == null)
                return;

            Monitor.Log("Begin Adding");
            // add garages
            BluePrint blueprint = this.CreateGreenhouse();
            foreach (CustomSaveBuilding building in saveData.Buildings)
            {
                Building newGarage = new Greenhouse(blueprint, building.Tile)
                {
                    buildingType = this.GarageBuildingType,
                    daysOfConstructionLeft = building.DaysOfConstructionLeft
                }; // rebuild to avoid data issues
                Monitor.Log("building " + newGarage.tileX.ToString());
                this.Farm.buildings.Add(newGarage);

                if (newGarage.daysOfConstructionLeft > 0)
                {

                    NPC robin = Game1.getCharacterFromName("Robin");
                    robin.ignoreMultiplayerUpdates = true;
                    robin.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                    new FarmerSprite.AnimationFrame(24, 75),
                    new FarmerSprite.AnimationFrame(25, 75),
                    new FarmerSprite.AnimationFrame(26, 300, false, false, new AnimatedSprite.endOfAnimationBehavior(farmer => this.Helper.Reflection.GetPrivateMethod(robin,"robinHammerSound").Invoke(farmer)), false),
                    new FarmerSprite.AnimationFrame(27, 1000, false, false, new AnimatedSprite.endOfAnimationBehavior(farmer => this.Helper.Reflection.GetPrivateMethod(robin,"robinVariablePause").Invoke(farmer)), false)
                   });
                    robin.ignoreScheduleToday = true;

                    Building underConstruction = Game1.getFarm().getBuildingUnderConstruction();

                    Game1.warpCharacter(robin, "Farm", new Vector2((float)(underConstruction.tileX + underConstruction.tilesWide / 2), (float)(underConstruction.tileY + underConstruction.tilesHigh / 2)), false, false);
                    robin.position.X += (float)(Game1.tileSize / 4);
                    robin.position.Y -= (float)(Game1.tileSize / 2);


                    this.Farm.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(399, 262, underConstruction.daysOfConstructionLeft == 1 ? 29 : 9, 43), new Vector2((float)(underConstruction.tileX + underConstruction.tilesWide / 2), (float)(underConstruction.tileY + underConstruction.tilesHigh / 2)) * (float)Game1.tileSize + new Vector2((float)(-Game1.tileSize / 4), (float)(-Game1.tileSize * 2 - Game1.tileSize / 4)), false, 0.0f, Color.White)
                    {
                        scale = (float)Game1.pixelZoom,
                        interval = 999999f,
                        animationLength = 1,
                        totalNumberOfLoops = 99999,
                        layerDepth = (float)((double)((underConstruction.tileY + underConstruction.tilesHigh / 2) * Game1.tileSize + Game1.tileSize / 2) / 10000.0)
                    });

                }
                else
                    this.Farm.buildStructure(newGarage, building.Tile, false, Game1.player);
                //if (this.IsNewTractor)
                //	this.SpawnTractor();
            }
        }



        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed == Keys.OemCloseBrackets)
            {

                //int start = 30;
                //int farmX = 25;
                //int farmY = 40;
                //this.Monitor.Log("Build Greenhouse key pressed. ]");
                //this.Farm.buildStructure(new Greenhouse(this.Helper.Content).SetDaysOfConstructionLeft(0), new Vector2(farmX, farmY), false, Game1.player);



                //GameLocation farmLocation = Game1.getLocationFromName("Farm");

                //farmLocation.warps.Add(new Warp(farmX + 4, farmY + 4, "Greenhouse", start + 10, start + 23, false));
                //farmLocation.warps.Add(new Warp(farmX + 5, farmY + 4, "Greenhouse", start + 10, start + 23, false));

                ////Game1.locations[1].Name

                //GameLocation greenhouseLocation = Game1.getLocationFromName("Greenhouse");
                //var tilesheet = greenhouseLocation.map.GetTileSheet("untitled tile sheet");


                //var aquaponics = this.Helper.Content.Load<Map>(@"assets\greenhousemap.xnb", ContentSource.ModFolder);
                //var layers = new[] { "Back", "Buildings", "Front" };
                //foreach (string lay in layers)
                //{
                //var aqualayer = aquaponics.GetLayer(lay);
                //var layer = greenhouseLocation.map.GetLayer(lay);
                //layer.LayerSize = new xTile.Dimensions.Size(230, 230);

                //	for (int x = 0; x<aqualayer.LayerSize.Width; x++)
                //	{
                //		for (int y = 0; y<aqualayer.LayerSize.Height; y++)
                //		{
                //			var aquaTile = aqualayer.Tiles[x, y];
                //			if (aquaTile != null)
                //				layer.Tiles[start + x, start + y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, aquaTile.TileIndex);
                //		}
                //	}
                //}

                //greenhouseLocation.warps.Add(new Warp(start + 10, start + 24, "Farm", farmX + 4, farmY + 7, false));
            }

        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs eventArgs)
        {
            this.IsNewDay = true;
            this.Farm = Game1.getFarm();
            this.RestoreCustomData();
        }

        private void MenuForceOurBuildingRendering(object sender, EventArgs eventArgs)
        {
            var carpenter = Game1.activeClickableMenu as CarpenterMenu;
            if (carpenter == null)
            {
                GameEvents.UpdateTick -= MenuForceOurBuildingRendering;
                Building[] greenhouses = this.GetGreenhouses(this.Farm).ToArray();

                foreach (Building greenhouse in greenhouses)
                    if (greenhouse.daysOfConstructionLeft == 2)
                        greenhouse.daysOfConstructionLeft = 3;
                return;
            }

            var currBuildingField = Helper.Reflection.GetPrivateField<Building>(carpenter, "currentBuilding");
            var currBuilding = currBuildingField.GetValue();
            if (currBuilding is Building && currBuilding.buildingType == this.GarageBuildingType)
            {
                currBuildingField.SetValue(new Greenhouse(CreateGreenhouse(), new Vector2()));
            }
        }

        private void MenuAddInBuilding(object sender, EventArgsClickableMenuChanged e)
        {

            if (e.NewMenu is CarpenterMenu)
            {
                GameEvents.UpdateTick += MenuForceOurBuildingRendering;

                List<BluePrint> blueprints = this.Helper.Reflection.GetPrivateValue<List<BluePrint>>(Game1.activeClickableMenu, "blueprints");
                blueprints.Add(CreateGreenhouse());
            }
        }

        private BluePrint CreateGreenhouse()
        {
            BluePrint AquaBP = new BluePrint(GarageBuildingType);
            AquaBP.itemsRequired.Clear();

            string[] strArray2 = "390 200".Split(' ');
            int index = 0;
            while (index < strArray2.Length)
            {
                if (!strArray2[index].Equals(""))
                    AquaBP.itemsRequired.Add(Convert.ToInt32(strArray2[index]), Convert.ToInt32(strArray2[index + 1]));
                index += 2;
            }
            AquaBP.texture = this.Helper.Content.Load<Texture2D>(@"assets\greenhouse.xnb", ContentSource.ModFolder);
            AquaBP.humanDoor = new Point(-1, -1);
            AquaBP.animalDoor = new Point(-2, -1);
            AquaBP.mapToWarpTo = "null";
            AquaBP.displayName = "Aquaponics";
            AquaBP.description = "A place to grow plants using fertilized water from your Fish!";
            AquaBP.blueprintType = "Buildings";
            AquaBP.nameOfBuildingToUpgrade = "";
            AquaBP.actionBehavior = "null";
            AquaBP.maxOccupants = -1;
            AquaBP.moneyRequired = 100; //ModConfig.TractorHousePrice;
            AquaBP.tilesWidth = 14;
            AquaBP.tilesHeight = 7;
            AquaBP.getTileSheetIndexForStructurePlacementTile(0, 0);
            AquaBP.sourceRectForMenuView = new Microsoft.Xna.Framework.Rectangle(0, 0, 96, 96);
            AquaBP.namesOfOkayBuildingLocations.Clear();
            AquaBP.namesOfOkayBuildingLocations.Add("Farm");
            AquaBP.magical = false;

            return AquaBP;
        }

        /// <summary>Get all garages in the given location.</summary>
        /// <param name="location">The location to search.</param>
        private IEnumerable<Building> GetGreenhouses(BuildableGameLocation location)
        {
            return location.buildings.Where(building => building.buildingType == this.GarageBuildingType);
        }
    }
}