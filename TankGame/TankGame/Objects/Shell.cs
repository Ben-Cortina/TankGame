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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Shell : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields

        Game tankGame;

        //Shell properties
        public Vector3 velocity;
        Vector3 cannonEnd;
        public Vector3 location;
        Tank owner;

        //calculation variables
        float gravity;
        float time;
        float elaspedTime;
        float radius;

        Model shellModel;

        GameServices.CameraState camera;
        GameServices.AudioLibrary audio;

        #endregion




        #region Initialize

        public Shell(Game game, Vector3 p_velocity, Vector3 p_cannonEnd, Tank p_owner)
            : base(game)
        {
            tankGame = game;
            velocity = p_velocity;
            location = p_cannonEnd;
            cannonEnd = p_cannonEnd;
            time = 0;
            gravity = 980.0665f;
            radius = 15;
            owner = p_owner;
            camera = (GameServices.CameraState)Game.Services.GetService(
                                typeof(GameServices.CameraState));
            audio = (GameServices.AudioLibrary)Game.Services.GetService(
                                typeof(GameServices.AudioLibrary));
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
            shellModel = tankGame.Content.Load<Model>("Models/Shell");
            foreach (ModelMesh mesh in shellModel.Meshes)
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

                    effect.SpecularPower = 1f;

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

            elaspedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Collision Detection
            if (location.Y < 1015)
                if (CollisionDetection())
                {
                    RemoveThis();
                }

            location.Y = (cannonEnd.Y + (velocity.Y * time) - (.5f) * gravity * time * time);
            location.X = cannonEnd.X + (velocity.X * time);
            location.Z = cannonEnd.Z + (velocity.Z * time);

            time += elaspedTime;

            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            Matrix world = Matrix.CreateTranslation(location);
            foreach (ModelMesh mesh in shellModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;

                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        #region Collision Detection

        /// <summary>
        /// Checks all tanks and buildings to see if a collision has occurred
        /// Called during each update if shell is low enough to hit something
        /// </summary>
        bool CollisionDetection()
        {
            if (location.Y < 15)
                return true;
            foreach (GameComponent component in tankGame.Components)
            {
                if (component is Tank && location.Y < ((Tank)component).Size.Y+15)
                {
                    if (CheckTankCollision((Tank)component))
                    {
                        audio.Play(audio.Hit, location);
                        return true;  
                    }
                }
                else if (component is Building)
                {
                    if (CheckBuildingCollision((Building)component))
                    {
                        audio.Play(audio.Hit, location);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check shell on tank collision it is assumed the shell is low enough to hit the tank.
        /// This only gets checked if the shells edge is below the height of the Tank, thus all calculations are in 2D
        /// </summary>
        /// <param name="tank">Tank</param>
        bool CheckTankCollision(Tank tank)
        {
            // First checks to see if this shell is within 596 units from the tank (595.4 units is the furthest collision possible)
            if (Vector3.Distance(tank.Location, location) < 596 && tank != owner)
            {
                //relative location and rotation
                float rot = tank.Rotation;

                //Calculate shells location relative to the tank
                Vector3 loc = new Vector3( (location.X - tank.Location.X) * (float)Math.Cos(rot) - (location.Z - tank.Location.Z) * (float)Math.Sin(rot), 
                                            location.Y,
                                           (location.X - tank.Location.X) * (float)Math.Sin(rot) + (location.Z - tank.Location.Z) * (float)Math.Cos(rot));

                Vector3 vel = new Vector3(velocity.X * (float)Math.Cos(rot) - velocity.Z * (float)Math.Sin(rot),
                                                velocity.Y,
                                                velocity.X * (float)Math.Sin(rot) + velocity.Z * (float)Math.Cos(rot));

                //Create an enlarged hitbox based on the shell's radius
                Vector3 hitBox = new Vector3(tank.Size.X + radius, tank.Size.Y + radius, tank.Size.Z + radius);

                //Check to see if the shell passes through the tanks hitbox
                if (CheckHitBoxCollision(hitBox, vel, loc))
                {
                    tank.Health -= 1;
                    if (tank.Health <= 0)
                        tankGame.Components.Remove(tank);
                    return true;
                }
            }
            return false;
        }

        bool CheckBuildingCollision(Building building)
        {
            if (Vector3.Distance(building.Location, location) < 1400)
            {
                //Calculate shells location relative to the building
                Vector3 loc = new Vector3(location.X - building.Location.X,
                                           location.Y,
                                           location.Z - building.Location.Z);
                //Create an enlarged hitbox based on the shell's radius
                Vector3 hitBox = new Vector3(building.Size.X + radius, building.Size.Y + radius, building.Size.Z + radius);
                //Check to see if the shell passes through the buildings hitbox
                if (CheckHitBoxCollision(hitBox, velocity, loc))
                {
                    tankGame.Components.Remove(building);
                    return true;
                }
            }
            return false;
            
        }

        /// <summary>
        /// Checks if a ray collides with a hitbox
        /// The hitbox is assumed to be unrotated.
        /// </summary>
        /// <param name="hitBox">X, Y, and Z, are the width/2, length/2, and height of the hit box
        ///  With the bottom of the box assumed to be at Y=0 and the middle at X=0 and Z=0</param>
        bool CheckHitBoxCollision(Vector3 hitBox, Vector3 vel, Vector3 loc)
        {
            Vector3 intersection = Vector3.Zero;

            //Is the shell in the hitbox?
            if (loc.Y <= hitBox.Y &&
                loc.X >= -hitBox.X && loc.X <= hitBox.X &&
                loc.Z >= -hitBox.Z && loc.Z <= hitBox.Z)
                return true;

            // Will the Shell pass through the hitbox this cycle?
            if (loc.Y > hitBox.Y)
                intersection = CheckPlaneIntersect(new Vector4(0, -1, 0, hitBox.Y), vel, loc);
            if (intersection != Vector3.Zero && InsideRectangle(new Vector2(intersection.X, intersection.Z), new Vector2(hitBox.X, hitBox.Z)))
                return true;
            else
            {
                intersection = Vector3.Zero;
                if (loc.X < -hitBox.X)
                    intersection = CheckPlaneIntersect(new Vector4(-1, 0, 0, -hitBox.X), vel, loc);
                if (intersection != Vector3.Zero && InsideRectangle(new Vector2(intersection.Y, intersection.Z), new Vector2(hitBox.Y, hitBox.Z)))
                    return true;
                else
                {
                    intersection = Vector3.Zero;
                    if (loc.X > hitBox.X)
                        intersection = CheckPlaneIntersect(new Vector4(-1, 0, 0, hitBox.X), vel, loc);
                    if (intersection != Vector3.Zero && InsideRectangle(new Vector2(intersection.Y, intersection.Z), new Vector2(hitBox.Y, hitBox.Z)))
                        return true;
                    else
                    {
                        intersection = Vector3.Zero;
                        if (loc.Z > hitBox.Z)
                            intersection = CheckPlaneIntersect(new Vector4(0, 0, -1, hitBox.Z), vel, loc);
                        if (intersection != Vector3.Zero && InsideRectangle(new Vector2(intersection.X, intersection.Y), new Vector2(hitBox.X, hitBox.Y)))
                            return true;
                        else
                        {
                            intersection = Vector3.Zero;
                            if (loc.Z < -hitBox.Z)
                                intersection = CheckPlaneIntersect(new Vector4(0, 0, -1, -hitBox.Z), vel, loc);
                            if (intersection != Vector3.Zero && InsideRectangle(new Vector2(intersection.X, intersection.Y), new Vector2(hitBox.X, hitBox.Y)))
                            return true;
                            else 
                                return false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the point of intersection of a ray and a plane
        /// Returns Zero if there is no intersect
        /// </summary>
        /// <param name="plane">X, Y and Z are the components of the 
        /// vector normal to the Plane.  The value of W is
        /// the distance of the plane to the origin</param>
        Vector3 CheckPlaneIntersect(Vector4 plane, Vector3 vel, Vector3 loc)
        {
            float timeOfIntersect = -(plane.X * loc.X + plane.Y * loc.Y + plane.Z * loc.Z + plane.W) /
                                    (plane.X * vel.X + plane.Y * vel.Y + plane.Z * vel.Z);
            if (timeOfIntersect < 0 || timeOfIntersect > elaspedTime)
                return Vector3.Zero;
            else
            {
                //Calculate Intersection
                return new Vector3(loc.X + vel.X * timeOfIntersect,
                                   loc.Y + vel.Y * timeOfIntersect,
                                   loc.Z + vel.Z * timeOfIntersect);
            }
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

        void RemoveThis()
        {
            tankGame.Components.Remove(this);
        }

        #endregion
    }
}
