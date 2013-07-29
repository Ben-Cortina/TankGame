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
    /// Commands for AI Movement
    /// </summary>
    enum AIMove
    {
        None,
        Forward,
        Backward,
        TurnLeft,
        TurnRight,
    }



    /// <summary>
    /// This is the class that handles tank behaviors.
    /// </summary>
    public class Tank : Microsoft.Xna.Framework.DrawableGameComponent
    {

        #region Fields

        Game tankGame;
        GameServices.CameraState camera;
        GameServices.AudioLibrary audio;

        //AI elements
        AIMove currentCommand;
        float targetRotation;
        Vector3 enemyLocation;
        float targetLocation;
        bool aiming;

        // The XNA framework Model object that we are going to display.
        Model tankModel;


        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;
        ModelBone leftSteerBone;
        ModelBone rightSteerBone;
        ModelBone turretBone;
        ModelBone cannonBone;
        ModelBone hatchBone;


        // Store the original transform matrix for each animating bone.
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;
        Matrix leftSteerTransform;
        Matrix rightSteerTransform;
        Matrix turretTransform;
        Matrix cannonTransform;
        Matrix hatchTransform;


        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] boneTransforms;


        // Current animation positions.
        float frontWheelRotation;
        float backWheelRotation;
        float steerRotation;
        float turretRotation;
        float cannonRotation;
        float hatchRotation;
        float totalRotation;
        Vector3 tankLocation;

        //Bone rotation speeds
        static float steerRotationSpeed = (float)Math.PI / 200f;
        static float turretRotationSpeed = (float)Math.PI / 250f;
        static float cannonRotationSpeed = (float)Math.PI / 500f;
        static float movementSpeed = 10f;
        static float frontWheelRotationSpeed = movementSpeed / 75f;
        static float backWheelRotationSpeed = movementSpeed / 100f;

        //Tank dimensions
        Vector3 tankSize;
        float barrelLength;

        //Stuff
        static float shellSpeed = 3000f;
        int health;
        bool isAI;
        double cannonCooldown = 3;
        double cannonFired;
        bool updateCannonTimer;
        bool cannonReloading;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Location of the tank.
        /// </summary>
        public Vector3 Location
        {
            get { return tankLocation; }
            set { tankLocation = value; }
        }

        /// <summary>
        /// Gets or sets the Rotation of the tank.
        /// </summary>
        public float Rotation
        {
            get { return totalRotation; }
            set { totalRotation = value; }
        }

        /// <summary>
        /// Gets the tanks turret
        /// </summary>
        public float TurretRotation
        {
            get { return totalRotation + turretRotation; }
        }

        /// <summary>
        /// Gets and sets the tanks health
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        /// <summary>
        /// Gets and sets the tanks Size
        /// </summary>
        public Vector3 Size
        {
            get { return tankSize; }
            set { tankSize = value; }
        }

        /// <summary>
        /// Gets and sets the tanks controller
        /// </summary>
        public bool AI
        {
            get { return isAI; }
            set { isAI = value;
                if (!isAI) cannonCooldown = 1.5f;
                else cannonCooldown = 3f;
            }
        }

        #endregion

        #region Initialization

        public Tank(Game game, Vector3 p_tankLocation)
            : base(game)
        {
            tankGame = game;
            isAI = true;
            camera = (GameServices.CameraState)Game.Services.GetService(
                                typeof(GameServices.CameraState));
            audio = (GameServices.AudioLibrary)Game.Services.GetService(
                                typeof(GameServices.AudioLibrary));

            //set the original tank propeties
            frontWheelRotation = 0;
            backWheelRotation = 0;
            steerRotation = 0;
            turretRotation = 0;
            cannonRotation = 0;
            hatchRotation = 0;
            totalRotation = 0;
            tankLocation = p_tankLocation;
            movementSpeed = 10;
            barrelLength = 300;
            health = 1;
            cannonFired = -1.5;
            cannonReloading = false;
            currentCommand = AIMove.None;

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
            // Load the tank model from the ContentManager.
            tankModel = tankGame.Content.Load<Model>("Models/tank");

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            tankSize = new Vector3(310, 350, 340);

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[tankModel.Bones.Count];
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
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

                    effect.SpecularPower = 50f;
                }
            }

        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            double timeSeconds = gameTime.TotalGameTime.TotalSeconds;
            if (updateCannonTimer)
            {
                cannonReloading = true;
                updateCannonTimer = false;
                cannonFired = timeSeconds;
            }

            if (isAI)
                HandleAI(gameTime);

            if (cannonReloading &&  timeSeconds - cannonFired  > cannonCooldown)
                cannonReloading = false;
                base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game componenet to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            // Draw the model.
            tankModel.Root.Transform = Matrix.CreateRotationY(totalRotation) * Matrix.CreateTranslation(tankLocation);

            // Calculate matrices based on the current animation position.
            Matrix frontWheelRotationMatrix = Matrix.CreateRotationX(frontWheelRotation);
            Matrix backWheelRotationMatrix = Matrix.CreateRotationX(backWheelRotation);
            Matrix steerRotationMatrix = Matrix.CreateRotationY(steerRotation);
            Matrix turretRotationMatrix = Matrix.CreateRotationY(turretRotation);
            Matrix cannonRotationMatrix = Matrix.CreateRotationX(cannonRotation);
            Matrix hatchRotationMatrix = Matrix.CreateRotationX(hatchRotation);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = backWheelRotationMatrix * leftBackWheelTransform;
            rightBackWheelBone.Transform = backWheelRotationMatrix * rightBackWheelTransform;
            leftFrontWheelBone.Transform = frontWheelRotationMatrix * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = frontWheelRotationMatrix * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotationMatrix * leftSteerTransform;
            rightSteerBone.Transform = steerRotationMatrix * rightSteerTransform;
            turretBone.Transform = turretRotationMatrix * turretTransform;
            cannonBone.Transform = cannonRotationMatrix * cannonTransform;
            hatchBone.Transform = hatchRotationMatrix * hatchTransform;

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }

                mesh.Draw();
            }
        }

        #endregion

        #region collision Detection

        /// <summary>
        /// Checks all tanks and buildings to see if a collision has occurred
        /// Called after each movement
        /// </summary>
        public bool CollisionDetection()
        {
            if (CheckBorder())
                return true;
            foreach (GameComponent component in tankGame.Components)
            {
                if(component is Tank)
                {
                    if (CheckTankCollision((Tank)component))
                        return true;
                }
                else if (component is Building)
                {
                    if(CheckBuildingCollision((Building)component))
                        return true;
                }
            }
            return false;
        }

        bool CheckBorder()
        {
            float rot = totalRotation;

            //Calculate other tanks corners relative to this building
            Vector2 topLeft = new Vector2(tankLocation.X + tankSize.X * (float)Math.Cos(rot) + tankSize.Z * (float)Math.Sin(rot),
                                          tankLocation.Y + tankSize.Z * (float)Math.Cos(rot) - tankSize.X * (float)Math.Sin(rot));
            Vector2 topRight = new Vector2(tankLocation.X - tankSize.X * (float)Math.Cos(rot) + tankSize.Z * (float)Math.Sin(rot),
                                           tankLocation.Y + tankSize.Z * (float)Math.Cos(rot) + tankSize.X * (float)Math.Sin(rot));
            Vector2 botLeft = new Vector2(tankLocation.X + tankSize.X * (float)Math.Cos(rot) - tankSize.Z * (float)Math.Sin(rot),
                                          tankLocation.Y - tankSize.Z * (float)Math.Cos(rot) - tankSize.X * (float)Math.Sin(rot));
            Vector2 botRight = new Vector2(tankLocation.X - tankSize.X * (float)Math.Cos(rot) - tankSize.Z * (float)Math.Sin(rot),
                                            tankLocation.Y - tankSize.Z * (float)Math.Cos(rot) + tankSize.X * (float)Math.Sin(rot));
            if (Math.Abs(topLeft.X) >= TankGame.worldsize.X  ||
                Math.Abs(topRight.X) >= TankGame.worldsize.X ||
                Math.Abs(botLeft.X) >= TankGame.worldsize.X  ||
                Math.Abs(botRight.X) >= TankGame.worldsize.X ||
                Math.Abs(topLeft.Y) >= TankGame.worldsize.Y  ||
                Math.Abs(topRight.Y) >= TankGame.worldsize.Y ||
                Math.Abs(botLeft.Y) >= TankGame.worldsize.Y  ||
                Math.Abs(topLeft.Y) >= TankGame.worldsize.Y  )
                return true;
            return false;


        }

        /// <summary>
        /// Checks if this tank is colliding with a building
        /// </summary>
        bool CheckBuildingCollision(Building building)
        {
            if (Vector3.Distance(building.Location, tankLocation) < 1300)
            {
                //relative location and rotation
                float rot = totalRotation;

                //Calculate tanks location relative to building
                Vector2 loc = new Vector2((tankLocation.X - building.Location.X),
                    (tankLocation.Z - building.Location.Z));

                //Calculate other tanks corners relative to this building
                Vector2 topLeft = new Vector2(loc.X + tankSize.X * (float)Math.Cos(rot) + tankSize.Z * (float)Math.Sin(rot),
                                              loc.Y + tankSize.Z * (float)Math.Cos(rot) - tankSize.X * (float)Math.Sin(rot));
                Vector2 topRight = new Vector2(loc.X - tankSize.X * (float)Math.Cos(rot) + tankSize.Z * (float)Math.Sin(rot),
                                               loc.Y + tankSize.Z * (float)Math.Cos(rot) + tankSize.X * (float)Math.Sin(rot));
                Vector2 botLeft = new Vector2(loc.X + tankSize.X * (float)Math.Cos(rot) - tankSize.Z * (float)Math.Sin(rot),
                                              loc.Y - tankSize.Z * (float)Math.Cos(rot) - tankSize.X * (float)Math.Sin(rot));
                Vector2 botRight = new Vector2(loc.X - tankSize.X * (float)Math.Cos(rot) - tankSize.Z * (float)Math.Sin(rot),
                                                loc.Y - tankSize.Z * (float)Math.Cos(rot) + tankSize.X * (float)Math.Sin(rot));
                Vector2 hitBox = new Vector2(building.Size.X, building.Size.Z);

                if (InsideRectangle(topLeft, hitBox) ||
                    InsideRectangle(topRight, hitBox) ||
                    InsideRectangle(botLeft, hitBox) ||
                    InsideRectangle(botRight, hitBox))
                    return true;
                //Checks to see if this buildings corners are in the tank
                else if (InsideTriangle(topLeft, topRight, botLeft, hitBox) ||
                        InsideTriangle(topLeft, topRight, botLeft, new Vector2(-hitBox.X, hitBox.Y)) ||
                        InsideTriangle(topLeft, topRight, botLeft, new Vector2(hitBox.X, -hitBox.Y)) ||
                        InsideTriangle(topLeft, topRight, botLeft, new Vector2(-hitBox.X, -hitBox.Y)) ||
                        InsideTriangle(botRight, topRight, botLeft, hitBox) ||
                        InsideTriangle(botRight, topRight, botLeft, new Vector2(-hitBox.X, hitBox.Y)) ||
                        InsideTriangle(botRight, topRight, botLeft, new Vector2(hitBox.X, -hitBox.Y)) ||
                        InsideTriangle(botRight, topRight, botLeft, new Vector2(-hitBox.X, -hitBox.Y)))
                    return true;


            }

            return false;
        }

        /// <summary>
        /// Check tank on tank collision
        /// Seeing as the tanks cannot travel vertically, all these calculations are in 2D
        /// </summary>
        /// <param name="tank">other tank</param>
        bool CheckTankCollision(Tank tank)
        {
            // First checks to see if this tank is within 921 units from the other tank (920.2 units is the furthest collision possible)
            if (Vector3.Distance(tank.Location,tankLocation) < 921 && tank != this)
            {
                //relative location and rotation
                float rot = tank.Rotation-totalRotation;

                //Calculate other tanks location relative to this tank
                Vector2 loc = new Vector2((tank.Location.X - tankLocation.X) * (float)Math.Cos(totalRotation) - (tank.Location.Z - tankLocation.Z) * (float)Math.Sin(totalRotation), 
                    (tank.Location.X - tankLocation.X) * (float)Math.Sin(totalRotation) + (tank.Location.Z - tankLocation.Z) * (float)Math.Cos(totalRotation));

                //Calculate other tanks corners relative to this tank
                Vector2 topLeft = new Vector2(loc.X + tankSize.X  * (float)Math.Cos(rot) + tankSize.Z *(float)Math.Sin(rot),
                                              loc.Y + tankSize.Z * (float)Math.Cos(rot) - tankSize.X *(float)Math.Sin(rot));
                Vector2 topRight = new Vector2(loc.X - tankSize.X * (float)Math.Cos(rot) + tankSize.Z * (float)Math.Sin(rot),
                                               loc.Y + tankSize.Z * (float)Math.Cos(rot) + tankSize.X *(float)Math.Sin(rot));
                Vector2 botLeft = new Vector2(loc.X + tankSize.X * (float)Math.Cos(rot) - tankSize.Z * (float)Math.Sin(rot),
                                              loc.Y - tankSize.Z * (float)Math.Cos(rot) - tankSize.X *(float)Math.Sin(rot));
                Vector2 botRight = new Vector2(loc.X - tankSize.X * (float)Math.Cos(rot) - tankSize.Z * (float)Math.Sin(rot),
                                                loc.Y - tankSize.Z * (float)Math.Cos(rot) + tankSize.X *(float)Math.Sin(rot));
                Vector2 hitBox = new Vector2(tankSize.X, tankSize.Z);

                //Check to see if other tanks corners are in this tank
                if(InsideRectangle(topLeft,hitBox)||
                    InsideRectangle(topRight,hitBox)||
                    InsideRectangle(botLeft,hitBox) ||
                    InsideRectangle(botRight,hitBox))
                    return true;

                //Checks to see if this tanks corners are in the other tank
                else if(InsideTriangle(topLeft, topRight, botLeft, hitBox)||
                        InsideTriangle(topLeft, topRight, botLeft, new Vector2(-hitBox.X, hitBox.Y))||
                        InsideTriangle(topLeft, topRight, botLeft, new Vector2(hitBox.X, -hitBox.Y))||
                        InsideTriangle(topLeft, topRight, botLeft, new Vector2(-hitBox.X, -hitBox.Y))||
                        InsideTriangle(botRight, topRight, botLeft, hitBox)||
                        InsideTriangle(botRight, topRight, botLeft, new Vector2(-hitBox.X, hitBox.Y))||
                        InsideTriangle(botRight, topRight, botLeft, new Vector2(hitBox.X, -hitBox.Y))||
                        InsideTriangle(botRight, topRight, botLeft, new Vector2(-hitBox.X, -hitBox.Y)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// SImple function to calculate if a point is in a rectangle
        /// </summary>
        /// <param name="point">Point coords</param>
        /// <param name="box">The width and height of the rectangle, the rectangle is assumed to be centered at the origin</param>
        bool InsideRectangle(Vector2 point, Vector2 box)
        {
            if (point.X >= -box.X &&
                point.X <= box.X &&
                point.Y >= -box.Y &&
                point.Y <= box.Y)
                return true;
            else 
                return false;
        }

        /// <summary>
        /// Checks to see if point P is in Triangle A-B-C
        /// Uses barycentric coordinates
        /// </summary>
        /// <param name="A">Triangle Point A</param>
        bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            // Compute vectors        
            Vector2 v0 = C - A;
            Vector2 v1 = B - A;
            Vector2 v2 = P - A;

            // Compute dot products
            float dot00 = dot(v0, v0);
            float dot01 = dot(v0, v1);
            float dot02 = dot(v0, v2);
            float dot11 = dot(v1, v1);
            float dot12 = dot(v1, v2);

            // Compute barycentric coordinates
            float det = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * det;
            float v = (dot00 * dot12 - dot01 * dot02) * det;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        float dot(Vector2 A, Vector2 B)
        {
            return A.X*B.X+A.Y*B.Y;
        }

        #endregion

        #region Movement Methods

        /// <summary>
        /// Rotates the wheels and moves and rotates the tank acording to the SteerRotation
        /// </summary>
        public void MoveForward()
        {
            //previous state
            float oldRotation=totalRotation;
            Vector3 oldLocation = tankLocation;

            frontWheelRotation += frontWheelRotationSpeed;
            backWheelRotation += backWheelRotationSpeed;

            Matrix movement = Matrix.CreateRotationY(steerRotation + totalRotation);
            Vector3 v = new Vector3(0, 0, movementSpeed);
            v = Vector3.Transform(v, movement);
            tankLocation.Z += v.Z;
            tankLocation.X += v.X;

            movement = Matrix.CreateRotationY(steerRotation);
            v = new Vector3(0, 0, movementSpeed);
            v = Vector3.Transform(v, movement);
            totalRotation += (float)(Math.Atan(v.X / (tankSize.Z-v.Z)));

            if (totalRotation > 2 * Math.PI)
                totalRotation -= (float)(2 * Math.PI);
            else if (totalRotation < 0)
                totalRotation += (float)(2 * Math.PI);

            if (frontWheelRotation > 2 * Math.PI)
                frontWheelRotation -= (float)(2 * Math.PI);
            if (backWheelRotation > 2 * Math.PI)
                backWheelRotation -= (float)(2 * Math.PI);
            
            // Revert state if collision
            if (CollisionDetection())
            {
                tankLocation = oldLocation;
                totalRotation = oldRotation;
            }
            //audio.Play(audio.Drive, tankLocation);
        }

        /// <summary>
        /// Rotates the wheels and moves and rotates the tank acording to the SteerRotation
        /// </summary>
        public void MoveBackward()
        {
            //previous state
            float oldRotation = totalRotation;
            Vector3 oldLocation = tankLocation;

            //Rotate the wheels
            frontWheelRotation -= frontWheelRotationSpeed;
            backWheelRotation -= backWheelRotationSpeed;

            //move the tank
            Matrix movement = Matrix.CreateRotationY(steerRotation + totalRotation);
            Vector3 v = new Vector3(0, 0, -movementSpeed);
            v = Vector3.Transform(v, movement);
            tankLocation.Z += v.Z;
            tankLocation.X += v.X;

            movement = Matrix.CreateRotationY(steerRotation);
            v = new Vector3(0, 0, movementSpeed);
            v = Vector3.Transform(v, movement);
            totalRotation -= (float)( Math.Atan(v.X/(tankSize.Z+v.Z)));

            if (totalRotation > 2 * Math.PI)
                totalRotation -= (float)(2 * Math.PI);
            else if (totalRotation < 0)
                totalRotation += (float)(2 * Math.PI);

            if (frontWheelRotation < 0)
                frontWheelRotation += (float)(2 * Math.PI);
            if (backWheelRotation < 0)
                backWheelRotation += (float)(2 * Math.PI);

            if (CollisionDetection())
            {
                tankLocation = oldLocation;
                totalRotation = oldRotation;
            }
        }

        /// <summary>
        /// Turns the tank wheel Left
        /// </summary>
        public void TurnLeft()
        {
            if (steerRotation < (float)((Math.PI) / 6))
                steerRotation += steerRotationSpeed;


        }

        /// <summary>
        /// Turns the tank wheel right
        /// </summary>
        public void TurnRight()
        {
            if (steerRotation > -(float)(Math.PI / 6))
                steerRotation -= steerRotationSpeed;

        }

        /// <summary>
        /// Turns the tank Turret Left
        /// </summary>
        public void TurnTurretLeft()
        {
            if (turretRotation < (float)MathHelper.PiOver2)
                turretRotation += turretRotationSpeed;
        }

        /// <summary>
        /// Turns the tank Turret Right
        /// </summary>
        public void TurnTurretRight()
        {
            if (turretRotation > -(float)MathHelper.PiOver2)
                turretRotation -= turretRotationSpeed;
        }

        /// <summary>
        /// Tilts the cannon up
        /// </summary>
        public void CannonUp()
        {
            if (cannonRotation > -MathHelper.PiOver4)
                cannonRotation -= cannonRotationSpeed;
        }

        /// <summary>
        /// Tilts the cannon down
        /// </summary>
        public void CannonDown()
        {
            if (cannonRotation < 0)
                cannonRotation += cannonRotationSpeed;
        }

        /// <summary>
        /// Creates a Shell and fires the cannon
        /// </summary>
        public void FireCannon()
        {
            if (!cannonReloading)
            {
                float totalTurretRotation = totalRotation + turretRotation;
                Vector3 velocity = new Vector3(
                    (float)(shellSpeed * Math.Cos(cannonRotation) * Math.Sin(totalTurretRotation)),
                    (float)(shellSpeed * Math.Sin(-cannonRotation)),
                    (float)(shellSpeed * Math.Cos(cannonRotation) * Math.Cos(totalTurretRotation)));

                Vector2 offset = new Vector2((float)((-30f) * Math.Sin(totalRotation)), (float)((-30f) * Math.Cos(totalRotation)));

                Vector3 cannonEnd = new Vector3(
                    (float)(Location.X + offset.X + barrelLength * Math.Cos(-cannonRotation) * Math.Sin(totalTurretRotation)),
                    tankSize.Y + (float)(Location.Y + barrelLength * Math.Sin(-cannonRotation)),
                    (float)(Location.Z + offset.Y + barrelLength * Math.Cos(-cannonRotation) * Math.Cos(totalTurretRotation)));

                tankGame.Components.Add(new Shell(tankGame, velocity, cannonEnd, this));
                updateCannonTimer = true;
                audio.Play(audio.Shoot, tankLocation);
            }
        }

        #endregion

        #region Aritificial Intelligence

        void HandleAI(GameTime gameTime)
        {
            foreach(GameComponent component in tankGame.Components)
                if (component is Tank)
                    if (!((Tank)component).AI)
                    {
                        enemyLocation = ((Tank)component).Location;
                        break;
                    }

            switch (currentCommand)
            {
                case AIMove.None:
                    CalculateNextCommand();
                    break;
                case AIMove.Forward:
                    AIForward();
                    break;
                case AIMove.Backward:
                    AIBackward();
                    break;
                case AIMove.TurnLeft:
                    AITurnLeft();
                    break;
                case AIMove.TurnRight:
                    AITurnRight();
                    break;
                default:
                    break;
            }
            AIAim();
        }

        void CalculateNextCommand()
        {
            //calculate angle to target based on world blocks (1000 unit blocks)
            Vector2 tankLoc = new Vector2((int)(tankLocation.X/1000) ,
                (int)(tankLocation.Z/ 1000 ));
            Vector2 enemyLoc = new Vector2((int)(enemyLocation.X/1000),
                (int)(enemyLocation.Z/ 1000 ));
            //Im exhausted and not thinking clearly so this is most likely unnecessary
            tankLoc.X = tankLoc.X + Math.Sign(tankLocation.X);
            tankLoc.Y = tankLoc.Y + Math.Sign(tankLocation.Z);
            enemyLoc.X = enemyLoc.X + Math.Sign(tankLocation.X);
            enemyLoc.Y = enemyLoc.Y + Math.Sign(tankLocation.Z);
            Vector2 loc = enemyLoc - tankLoc;
            float angle;
            if (loc.Length() != 0)
            {
                angle = (float)Math.Acos(Math.Abs(loc.Y) / (loc.Length()));
                if (loc.X < 0 && loc.Y < 0)
                    angle -= (float)MathHelper.Pi;
                else if (loc.Y < 0)
                    angle = (float)MathHelper.Pi - angle;
                else if (loc.X < 0)
                    angle = - angle;
            }
            else
                angle = 0;

            //calculate rotation relative to tank
            float relAngle = angle- totalRotation;

            if (relAngle > Math.PI)
                relAngle -= (float)Math.PI * 2;
            if (relAngle < -Math.PI)
                relAngle += (float)Math.PI*2;

            //if rotation is right
            if (Math.Abs(relAngle) < MathHelper.PiOver2 )
            {
                //if not too close
                if (loc.Length() > 6)
                {
                    //AI move forward
                    currentCommand = AIMove.Forward;

                    targetLocation = (tankLoc.X * 1000 - 2500 * Math.Sign(tankLocation.X)) * (float)Math.Sin(totalRotation) +
                                     (tankLoc.Y * 1000 - 2500 * Math.Sign(tankLocation.Z)) * (float)Math.Cos(totalRotation);
                }
                else if (loc.Length() < 4)
                {
                    //move back
                    currentCommand = AIMove.Backward;
                    targetLocation = (tankLoc.X * 1000 + 1500 * Math.Sign(tankLocation.X)) * (float)Math.Sin(totalRotation) +
                                     (tankLoc.Y * 1000 + 1500 * Math.Sign(tankLocation.Z)) * (float)Math.Cos(totalRotation);
                    
                }
            }
            //else If tank is not in safe location ( 800 > tank location - i*1500*2 > -800)
            else if ((Math.Abs(tankLoc.X) - 2) % 3 == 0 || (Math.Abs(tankLoc.Y) - 2) % 3 == 0)
            {
                //Move forward
                currentCommand = AIMove.Forward;
            }
            else if (relAngle > 0)
                {
                    currentCommand = AIMove.TurnLeft;
                    targetRotation = totalRotation + MathHelper.PiOver2;
                    if (targetRotation > (float)Math.PI * 2)
                        targetRotation -= (float)Math.PI * 2;
                }
            else if (relAngle < 0)
            {
                //turn right
                currentCommand = AIMove.TurnRight;
                targetRotation = totalRotation - MathHelper.PiOver2;
                if (targetRotation < 0)
                    targetRotation += (float)Math.PI * 2;
            }
        }

        /// <summary>
        /// Turns Tank 90 degrees
        /// </summary>
        void AITurnLeft()
        {
            Vector3 loc = tankLocation;
            float oldRotation = totalRotation;
            if (totalRotation >= targetRotation && targetRotation != 2 * Math.PI)
            {
                loc = tankLocation;
                if (steerRotation != 0)
                {
                    TurnRight();
                    if (Math.Abs(steerRotation) < 1f / 300f)
                        steerRotation = 0;
                }
                else
                {
                    totalRotation = targetRotation;
                    currentCommand = AIMove.Forward;
                    Vector2 tankLoc = new Vector2((int)(tankLocation.X / 1000),
                        (int)(tankLocation.Z / 1000));
                    tankLoc.X = tankLoc.X + Math.Sign(tankLocation.X);
                    tankLoc.Y = tankLoc.Y + Math.Sign(tankLocation.Z);

                    targetLocation = (tankLoc.X * 1000 - 2500 * Math.Sign(tankLocation.X)) * (float)Math.Sin(totalRotation) +
                                     (tankLoc.Y * 1000 - 2500 * Math.Sign(tankLocation.Z)) * (float)Math.Cos(totalRotation);
                }
                oldRotation = totalRotation;
            }
            else if (totalRotation < targetRotation)
            {
                TurnLeft();
                MoveForward();
                if (loc == tankLocation)
                    TurnRight();
            }

            if (oldRotation > totalRotation && targetRotation == Math.PI)
                totalRotation = 2 * (float)Math.PI;
        }

        /// <summary>
        /// Turns Tank 90 degrees
        /// </summary>
        void AITurnRight()
        {
            Vector3 loc = tankLocation;
            float oldRotation = totalRotation;
            if (totalRotation <= targetRotation && targetRotation != 0)
            {
                loc = tankLocation;
                if (steerRotation != 0)
                {
                    TurnLeft();
                    if (Math.Abs(steerRotation) < 1f / 300f)
                        steerRotation = 0;
                }
                else
                {
                    totalRotation = targetRotation;
                    currentCommand = AIMove.Forward;
                    Vector2 tankLoc = new Vector2((int)(tankLocation.X / 1000),
                        (int)(tankLocation.Z / 1000));
                    tankLoc.X = tankLoc.X + Math.Sign(tankLocation.X);
                    tankLoc.Y = tankLoc.Y + Math.Sign(tankLocation.Z);
                    
                    targetLocation = (tankLoc.X*1000 - 2500 * Math.Sign(tankLocation.X))* (float)Math.Sin(totalRotation) + 
                                     (tankLoc.Y*1000 - 2500 * Math.Sign(tankLocation.Z))* (float)Math.Cos(totalRotation);
                }
                oldRotation = totalRotation;
            }
            else if (totalRotation > targetRotation)
            {
                TurnRight();
                MoveForward();
                if (loc == tankLocation)
                    TurnLeft();
            }

            if (oldRotation < totalRotation && targetRotation == 0)
                totalRotation = 0;
        }

        /// <summary>
        /// Moves the tank 3 blocks
        /// </summary>
        void AIForward()
        {
            //1D tank location
            float tankLoc = tankLocation.X * (float)Math.Sin(totalRotation) + 
                            tankLocation.Z * (float)Math.Cos(totalRotation);
            if (tankLoc < targetLocation)
                MoveForward();
            else
                currentCommand = AIMove.None;

        }

        /// <summary>
        /// Moves the tank 3 blocks
        /// </summary>
        void AIBackward()
        {
            //1D tank location
            float tankLoc = tankLocation.X * (float)Math.Sin(totalRotation) +
                            tankLocation.Z * (float)Math.Cos(totalRotation);
            if (tankLoc > targetLocation)
                MoveBackward();
            else
                currentCommand = AIMove.None;

        }

        /// <summary>
        /// Aims the turret
        /// </summary>
        void AIAim()
        {
            //calculate angle to target based on world blocks (1000 unit blocks)
            Vector2 tankLoc = new Vector2(tankLocation.X,
                tankLocation.Z);
            Vector2 enemyLoc = new Vector2(enemyLocation.X,
                enemyLocation.Z) ;
            //Im exhausted and not thinking clearly so this is most likely unnecessary
            Vector3 loc = enemyLocation - tankLocation;
            float distance = Vector2.Distance(tankLoc, enemyLoc);
            //distance the shell would travel at the current angle
            float yVel = (float)(shellSpeed * Math.Sin(-cannonRotation));
            float grav = 980.0665f;
            float time = (float)Math.Max((-yVel+Math.Sqrt(yVel*yVel+ grav*2*400))/-grav,(-yVel-Math.Sqrt(yVel*yVel+ grav*2*400))/-grav);
            
            float aimDistance = 350+ (float)(shellSpeed * Math.Cos(cannonRotation) * time);
            float angle;
            if (loc.Length() != 0)
            {
                angle = (float)Math.Acos(Math.Abs(loc.Z) / (loc.Length()));
                if (loc.X < 0 && loc.Z < 0)
                    angle -= (float)MathHelper.Pi;
                else if (loc.Z < 0)
                    angle = (float)MathHelper.Pi - angle;
                else if (loc.X < 0)
                    angle = -angle;
            }
            else
                angle = 0;

            //calculate rotation relative to tank
            float relAngle = angle - totalRotation;

            if (relAngle > Math.PI)
                relAngle -= (float)Math.PI * 2;
            if (relAngle < -Math.PI)
                relAngle += (float)Math.PI * 2;
            //randomize aim
            Random derp = new Random();

            if ((turretRotation < relAngle + Math.PI / 10 && turretRotation > relAngle - Math.PI / 10 && aimDistance < distance + 600 && aimDistance > distance - 600))
            {
                //randomize aim
                if (derp.Next(4) == 1)
                    aiming = false;
                if (aiming == false && distance < 8000)
                    FireCannon();
            }
            else
            {
                aiming = true;
                if (turretRotation > relAngle + Math.PI / 10)
                    TurnTurretRight();
                else if (turretRotation < relAngle - Math.PI / 10)
                    TurnTurretLeft();
                else
                {
                    if (derp.Next(2) < 1)
                        TurnTurretLeft();
                    else
                        TurnTurretRight();
                }
                if (aimDistance > distance + 300)
                    CannonDown();
                else if (aimDistance < distance - 300)
                    CannonUp();
                else
                {
                    if (derp.Next(2) < 1)
                        CannonUp();
                    else
                        CannonDown();
                }
                
            }
        }
        
        #endregion
    }
}
