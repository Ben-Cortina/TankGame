#region File introduction
// AudioLibrary.cs by Ben Cortina
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

#endregion

namespace TankGame.GameServices
{
    /// <summary>
    /// Used for passing loaded sounds to all members of the game
    /// </summary>
    public class AudioLibrary
    {
        #region Fields and Properties

        SoundEffect shoot;

        /// <summary>
        /// returns the sound for shooting the tank gun
        /// </summary>
        public SoundEffect Shoot
        {
            get { return shoot; }
        }

        SoundEffect hit;

        /// <summary>
        /// returns the sound to indicate a correct response
        /// </summary>
        public SoundEffect Hit
        {
            get { return hit; }
        }


        SoundEffectInstance engine;

        /// <summary>
        /// Returns the sound that tank makes
        /// </summary>
        public SoundEffectInstance Engine
        {
            get { return engine; }
        }

        AudioListener listener;

        /// <summary>
        /// Gets or sets the sound listener location
        /// </summary>
        public Vector3 ListenerPosition
        {
            get { return listener.Position; }
            set { listener.Position = value; }
        }

        #endregion

        #region LoadContent
        
        /// <summary>
        /// Allows the Audio Library to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void InitializeAudio()
        {
            listener = new AudioListener();
            listener.Position =Vector3.Zero;
        }


        /// <summary>
        /// Load the sounds
        /// </summary>
        public void LoadContent(ContentManager Content)
        {
            hit = Content.Load<SoundEffect>("Sounds/Hit");
            shoot = Content.Load<SoundEffect>("Sounds/CannonShot");
            SoundEffect sound;
            sound = Content.Load<SoundEffect>("Sounds/TankEngine");
            engine = sound.CreateInstance();
            engine.IsLooped = true;
            engine.Volume = .3f;
            engine.Play();
        }

        #endregion

        #region methods

        public void Update(Vector3 position, float p_rotation)
        {
            Vector3 normal = new Vector3((float)Math.Sin(p_rotation),0,(float)Math.Cos(p_rotation));
            listener.Position = position;
            listener.Forward = normal;
        }

        public void Play(SoundEffect sound, Vector3 position)
        {
            SoundEffect.DistanceScale = 5000f;
            SoundEffectInstance soundInstance = sound.CreateInstance();
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = position;
            soundInstance.Apply3D(listener, emitter);
            soundInstance.Play();

        }

        #endregion
    }
}
