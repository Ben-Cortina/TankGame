#region using statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion


namespace TankGame.GameServices
{
    class CameraState
    {
        #region Fields

        //holds camera location angle, and up vector.
        //view = Matrix.CreateLookAt(cameraPosition, avatarPosition, new Vector3(0.0f, 1.0f, 0.0f));
        Matrix view; 

        //holds FOV aspect ratio and view distance
        // proj = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearClip, farClip);
        Matrix proj;

        //Camera distance from the tank
        Vector3 cameraOffSet;

        bool map;

        #endregion

        #region properties

        /// <summary>
        /// Returns the View
        /// </summary>
        public Matrix View
        {
            get{return view;}
        }

        /// <summary>
        /// Returns the projection
        /// </summary>
        public Matrix Projection
        {
            get { return proj; }
        }

        #endregion

        #region initialize

        /// <summary>
        /// Contrustor for CameraState, sets the projection matrix.
        /// </summary>
        /// <param name="viewAngle">FOV</param>
        /// <param name="aspectRatio">window aspect ratio</param>
        /// <param name="nearClip">sets the distance from camera before it draws</param>
        /// <param name="farClip">sets the distance from camera before it clips</param>
        public CameraState(float viewAngle, float aspectRatio, float nearClip, float farClip)
        {
            
            proj = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearClip, farClip);
            cameraOffSet = new Vector3(0, 1000, -3000);
            map = false;
        }

        #endregion

        #region methods

        /// <summary>
        /// Update the camera view
        /// </summary>
        /// <param name="position">player position</param>
        /// <param name="rotation">player rotation</param>
        public void Update(Vector3 position, float rotation)
        {
            if(!map)
            {
            Matrix rotationMatrix = Matrix.CreateRotationY(rotation);

            // Camera offset rotation based on player rotation
            Vector3 transformedReference = Vector3.Transform(cameraOffSet, rotationMatrix);

            //calculate offset
            Vector3 cameraPosition = position + transformedReference;

            // Set up the view matrix and projection matrix.
            view = Matrix.CreateLookAt(cameraPosition, position, new Vector3(0.0f, 1.0f, 0.0f));
            }
        }

        public void ToggleMap(Vector3 position, float rotation)
        {
            map = !map;
            if (map)
                view = Matrix.CreateLookAt(new Vector3(0.0f, 80000f, -1.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
            else
                Update(position, rotation);
        }

        #endregion
    }
}
