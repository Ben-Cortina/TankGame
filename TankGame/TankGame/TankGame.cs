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

namespace TankGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TankGame : Microsoft.Xna.Framework.Game
    {
        #region fields

        SpriteBatch textInfo;
        GraphicsDeviceManager graphics;
        GameServices.InputState input;
        GameServices.AudioLibrary audio;
        GameServices.CameraState camera;
        Objects.Tank playerTank;
        VertexPositionNormalTexture[] groundVertices;

        Texture2D groundTex;
        Texture2D health;

        List<List<Objects.Building>> buildings;
        short[] groundIndexes;
        SpriteFont font;
        BasicEffect basicEffect;

        public static Vector2 worldsize = new Vector2(100000,100000);

        #endregion

        #region Initialize

        public TankGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1100;
            graphics.PreferredBackBufferHeight = 640;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            //Initialize Input
            input = new GameServices.InputState();
            input.Update();
            Services.AddService(typeof(GameServices.InputState), input);
            
            //create camera
            camera = new GameServices.CameraState(
                MathHelper.PiOver4,
                (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height,
                1.0f,
                100000.0f);
            Services.AddService(typeof(GameServices.CameraState), camera);

            //create audip
            audio = new GameServices.AudioLibrary();
            audio.InitializeAudio();
            Services.AddService(typeof(GameServices.AudioLibrary), audio);

            //create tanks
            playerTank = new Objects.Tank(this, new Vector3(0, 0, 0));
            playerTank.Health = 10;
            playerTank.AI = false;
            Components.Add(playerTank);

            //buildings
            buildings = new List<List<Objects.Building>>();
            List<Objects.Building> listception = new List<Objects.Building>();
            Objects.Building build;
            for (int i = 1; i <= worldsize.Y / 3000; i++)
            {
                for (int j = 1; j <= worldsize.X / 3000; j++)
                {
                    build = new Objects.Building(this, new Vector3(i * 3000 + 435 - worldsize.Y / 2, 0, j * 3000 + 435 - worldsize.X / 2));
                    build.Enabled = false;
                    Components.Add(build);
                }
            }
            
            Random x = new Random();
            int n = x.Next(5, 50);
            Objects.Tank tank;
            for (int i = 0; i < n; i++)
            {
                do
                tank = new Objects.Tank(this, new Vector3(x.Next(-15, 16) * 3000, 0, x.Next(-15, 16) * 3000));
                while (tank.CollisionDetection());
                Components.Add(tank);
            }
             

            camera.Update(playerTank.Location, playerTank.TurretRotation);

            GenerateGround(worldsize.X, worldsize.Y);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.Load<Model>("Models/tank");
            Content.Load<Model>("Models/Shell");
            Content.Load<Model>("Models/shed");
            font = Content.Load<SpriteFont>("Font");
            groundTex = Content.Load<Texture2D>("Images/RockyGround1");
            health = Content.Load<Texture2D>("Images/health");
            textInfo = new SpriteBatch(GraphicsDevice);
            audio.LoadContent(Content);
            InitializeEffect();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Handles keyboard input
        /// </summary>
        public void HandleInput()
        {
            //refresh input
            input.Update();

            //Check Tank Controls
            if (input.IsLeft())
                playerTank.TurnLeft();
            if (input.IsRight())
                playerTank.TurnRight();
            if (input.IsUp())
            {
                playerTank.MoveForward();
                camera.Update(playerTank.Location, playerTank.TurretRotation);
                audio.Update(playerTank.Location, playerTank.TurretRotation);
            }
            if (input.IsDown())
            {
                playerTank.MoveBackward();
                camera.Update(playerTank.Location, playerTank.TurretRotation);
                audio.Update(playerTank.Location, playerTank.TurretRotation);
            }
            if (input.IsLeftAlt())
            {
                playerTank.TurnTurretLeft();
                camera.Update(playerTank.Location, playerTank.TurretRotation);
                audio.Update(playerTank.Location, playerTank.TurretRotation);
            }
            if (input.IsRightAlt())
            {
                playerTank.TurnTurretRight();
                camera.Update(playerTank.Location, playerTank.TurretRotation);
                audio.Update(playerTank.Location, playerTank.TurretRotation);
            }
            if (input.IsUpAlt())
                playerTank.CannonUp();
            if (input.IsDownAlt())
                playerTank.CannonDown();
            if (input.IsFire())
                playerTank.FireCannon();
            if (input.IsExit())
                this.Exit();
            if (input.IsMap())
                camera.ToggleMap(playerTank.Location, playerTank.TurretRotation);

        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (playerTank.Health <= 0)
                this.Exit();
            HandleInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            textInfo.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default,RasterizerState.CullNone);

            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserIndexedPrimitives
                <VertexPositionNormalTexture>(
                PrimitiveType.TriangleList,
                groundVertices, 0, 4,
                groundIndexes, 0, 2);
            }
            int i=-1;
            foreach(GameComponent derp in Components)
                if (derp is Objects.Tank)
                    i++;

           textInfo.DrawString(font, "Enemy Tanks Remaining: " + i.ToString() , new Vector2(20, 40), Color.LightGreen,
                    0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
           textInfo.DrawString(font, "Health", new Vector2(20, 20), Color.LightGreen,
                   0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            for (i=0;i<10;i++)
            {
                if (i < playerTank.Health)
                    textInfo.Draw(health, new Rectangle(100 + (i) * 20, 20, 20, 20), Color.Green);
                else
                    textInfo.Draw(health, new Rectangle(100 + (i) * 20, 20, 20, 20), Color.Red);
            }
            base.Draw(gameTime);
            textInfo.End();
        }

        #endregion

        #region methods

        void GenerateGround(float width, float length)
        {
            groundVertices = new VertexPositionNormalTexture[4];
            groundIndexes = new short[6];

            Vector3 topLeft = new Vector3(width/2f, 0f, length/2f);
            Vector3 bottomLeft = new Vector3(width/2f, 0f, -length/2f);
            Vector3 topRight = new Vector3(-width/2f, 0f, length/2f);
            Vector3 bottomRight = new Vector3(-width/2f, 0f, -length/2f);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(100f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 100f);
            Vector2 textureBottomRight = new Vector2(100f, 100f);

            Vector3 frontNormal = Vector3.Up;

            // Front face.
            groundVertices[0] =
                new VertexPositionNormalTexture(
                topLeft, frontNormal, textureTopLeft);
            groundVertices[1] =
                new VertexPositionNormalTexture(
                topRight, frontNormal, textureTopRight);
            groundVertices[2] =
                new VertexPositionNormalTexture(
                bottomRight, frontNormal, textureBottomRight);
            groundVertices[3] =
                new VertexPositionNormalTexture(
                bottomLeft, frontNormal, textureBottomLeft);


            groundIndexes[0] = 0;
            groundIndexes[1] = 1;
            groundIndexes[2] = 2;
            groundIndexes[3] = 2;
            groundIndexes[4] = 3;
            groundIndexes[5] = 0;

        }

        /// <summary>
        /// Initializes the basic effect (parameter setting and technique selection)
        /// used for the 3D model.
        /// </summary>
        private void InitializeEffect()
        {
           
            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.World = Matrix.CreateTranslation(Vector3.Zero);
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
            basicEffect.Texture = groundTex;
            basicEffect.TextureEnabled = true;

            basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            //effect.EmissiveColor = new Vector3(.5f, .5f, .5f);

            basicEffect.LightingEnabled = true; // turn on the lighting subsystem.

            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f); // White
            basicEffect.DirectionalLight0.Direction = new Vector3(-1 / (float)Math.Sqrt(2), -1 / (float)Math.Sqrt(2), 0);  // coming from +X, +Y
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(1f, .5f, .5f); // red highlight

            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(1f, 1f, 1f); // White
            basicEffect.DirectionalLight1.Direction = new Vector3(0, -1 / (float)Math.Sqrt(2), -1 / (float)Math.Sqrt(2));  // coming from +Z, +Y
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(.5f, .5f, 1); // blu highlight

            basicEffect.DirectionalLight2.Enabled = true;
            basicEffect.DirectionalLight2.DiffuseColor = new Vector3(1f, 1f, 1f); // White
            basicEffect.DirectionalLight2.Direction = new Vector3(0, -1, 0);  // coming from +Y
            basicEffect.DirectionalLight2.SpecularColor = new Vector3(.5f, 1f, .5f); // green highlight
        }


        #endregion
    }
}
