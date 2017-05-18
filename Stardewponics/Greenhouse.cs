using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace Stardewponics
{
    public class Greenhouse : Building
    {
		/*********
		** Public methods
		*********/

        public Greenhouse(BluePrint blueprint, Vector2 tileLocation)
        {
            //buildingType = (int)
            //humanDoor = new Point(-1, -1);
            //animalDoor = new Point(-2, -1);
            //indoors = null;
            //nameOfIndoors = "";
            //baseNameOfIndoors = "";
            //nameOfIndoorsWithoutUnique = "";
            //magical = false;
            //tileX = 0;
            //tileY = 0;
            //maxOccupants = 0;
            //tilesWide = 14;
            //tilesHigh = 7;
            //this.texture = content.Load<Texture2D>(@"assets\greenhouse.xnb", ContentSource.ModFolder);
            //daysOfConstructionLeft = 1;
            //leftShadow = new Rectangle(656, 394, 16, 16); //656, 394, 16, 16

			this.tileX = (int)tileLocation.X;
			this.tileY = (int)tileLocation.Y;
			this.tilesWide = blueprint.tilesWidth;
			this.tilesHigh = blueprint.tilesHeight;
			this.buildingType = blueprint.name;
			this.texture = blueprint.texture;
			this.humanDoor = blueprint.humanDoor;
			this.animalDoor = blueprint.animalDoor;
			this.nameOfIndoors = blueprint.mapToWarpTo;
			this.baseNameOfIndoors = this.nameOfIndoors;
			this.nameOfIndoorsWithoutUnique = this.baseNameOfIndoors;
			this.indoors = this.getIndoors();
			this.nameOfIndoors = this.nameOfIndoors + (object)(this.tileX * 2000 + this.tileY);
			this.maxOccupants = blueprint.maxOccupants;
			this.daysOfConstructionLeft = 4;
			this.magical = blueprint.magical;
        }

        public Greenhouse SetDaysOfConstructionLeft(int input)
        {
            daysOfConstructionLeft = input;
            return this;
        }

        public override bool intersects(Rectangle boundingBox)
        {
            if (!base.intersects(boundingBox))
                return false;
            if (boundingBox.X >= (this.tileX + 4) * Game1.tileSize && boundingBox.Right < (this.tileX + 7) * Game1.tileSize)
                return boundingBox.Y <= (this.tileY + 1) * Game1.tileSize;
            return true;
        }


        public override void draw(SpriteBatch b)
        {
            if (this.daysOfConstructionLeft > 0)
            {
                this.drawInConstruction(b);
            }
            else
            {
                this.drawShadow(b, -1, -1);
                b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, (float)this.texture.Bounds.Height), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 2) * Game1.tileSize) / 10000f);
            }
        }

        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            CarpenterMenu menu = Game1.activeClickableMenu as CarpenterMenu;
            float texScale = 2;
            int num1 = (menu.maxWidthOfBuildingViewer - (int)(texture.Width * texScale)) / 2;
            num1 -= (int)(texture.Width / 3.5);
            int num2 = (menu.maxHeightOfBuildingViewer - (int)(texture.Height * texScale)) / 2;
            this.drawShadow(b, num1, num2);
            b.Draw(this.texture, new Rectangle(Game1.activeClickableMenu.xPositionOnScreen + num1, Game1.activeClickableMenu.yPositionOnScreen + num2, (int)(texture.Width * texScale), (int)(texture.Height * texScale)), Color.White);
        }

		public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
		{
			Vector2 position = localX == -1 ? Game1.GlobalToLocal(new Vector2((float)(this.tileX * Game1.tileSize), (float)((this.tileY + this.tilesHigh) * Game1.tileSize))) : new Vector2((float)localX, (float)(localY + this.getSourceRectForMenu().Height * Game1.pixelZoom));
			b.Draw(Game1.mouseCursors, position, new Rectangle?(Building.leftShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
			for (int index = 1; index < this.tilesWide - 1; ++index)
				b.Draw(Game1.mouseCursors, position + new Vector2((float)(index * Game1.tileSize), 0.0f), new Rectangle?(Building.middleShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
			b.Draw(Game1.mouseCursors, position + new Vector2((float)((this.tilesWide - 1) * Game1.tileSize), 0.0f), new Rectangle?(Building.rightShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
		}

        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);
            if (this.daysOfConstructionLeft > 0)
                return;
            //this.grabHorse();
            //do special action for this building here
            /*
			 */
        }

        public override Rectangle getSourceRectForMenu()
        {
            return new Rectangle(0, 0, this.texture.Bounds.Width - 1, this.texture.Bounds.Height);
        }
    }
}