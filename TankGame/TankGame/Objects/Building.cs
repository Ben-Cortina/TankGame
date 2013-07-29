using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace TankGame.Objects
{
    /// <summary>
    /// immobile building
    /// </summary>
    public class Building : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields

        Game tankGame;

        //building properties
        Vector3 location;
        static Vector3 buildingSize = new Vector3(450, 900, 450);

        Model buildingModel;
        GameServices.CameraState camera;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the building Location
        /// </summary>
        public Vector3 Location
        {
            get { return location; }
        }

        /// <summary>
        /// Gets the building Size
        /// </summary>
        public Vector3 Size
        {
            get { return buildingSize; }
        }

        #endregion

        #region Initialize

        public Building(Game game, Vector3 p_location)
            : base(game)
        {
            tankGame = game;
            location = p_location;
            //world = Matrix.CreateTranslation(location);
            camera = (GameServices.CameraState)Game.Services.GetService(
                                typeof(GameServices.CameraState));
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            buildingModel = tankGame.Content.Load<Model>("models/shed");

            //leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            foreach (ModelMesh mesh in buildingModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
                    //effect.EmissiveColor = new Vector3(.5f, .5f, .5f);

                    effect.LightingEnabled = true; // turn on the lighting subsystem.

                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f); // White
                    effect.DirectionalLight0.Direction = new Vector3(-1 / (float)Math.Sqrt(2), -1 / (float)Math.Sqrt(2), 0);  // coming from +X, +Y
                    effect.DirectionalLight0.SpecularColor = new Vector3(1f, .5f, .5f); // red highlight

                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.DiffuseColor = new Vector3(1f, 1f, 1f); // White
                    effect.DirectionalLight1.Direction = new Vector3(0, -1 / (float)Math.Sqrt(2), -1 / (float)Math.Sqrt(2));  // coming from +Z, +Y
                    effect.DirectionalLight1.SpecularColor = new Vector3(.5f, .5f, 1); // blu highlight

                    effect.DirectionalLight2.Enabled = true;
                    effect.DirectionalLight2.DiffuseColor = new Vector3(1f, 1f, 1f); // White
                    effect.DirectionalLight2.Direction = new Vector3(0, -1, 0);  // coming from +Y
                    effect.DirectionalLight2.SpecularColor = new Vector3(.5f, 1f, .5f); // green highlight

                    effect.SpecularPower = 1000f;
                }
            }
        }
        #endregion

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            Matrix world = Matrix.CreateTranslation(location);// *Matrix.CreateRotationX(MathHelper.PiOver2);
            foreach (ModelMesh mesh in buildingModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = mesh.ParentBone.Transform * Matrix.CreateScale(8f)* world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
