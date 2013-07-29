#region File Description
//-----------------------------------------------------------------------------
// Modified from Microsoft Corporation's InputState.cs
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace TankGame.GameServices
{
    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and Mouse. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move through the menu"
    /// </summary>
    public class InputState
    {
        #region Fields

        KeyboardState CurrentKeyboardState;
        
        /// <summary>
        /// keyboard state
        /// </summary>
        public KeyboardState KeyboardState
        {
            get { return CurrentKeyboardState; }
        }

        GamePadState CurrentGamePadState;

        /// <summary>
        /// GamePad state
        /// </summary>
        public GamePadState GamePadState
        {
            get { return CurrentGamePadState; }
        }

        MouseState CurrentMouseState;

        /// <summary>
        /// Mouse state
        /// </summary>
        public MouseState MouseState
        {
            get { return CurrentMouseState; }
        }

        KeyboardState LastKeyboardState;
        GamePadState LastGamePadState;
        MouseState LastMouseState;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardState = new KeyboardState();
            CurrentGamePadState = new GamePadState();
            CurrentMouseState = new MouseState();

            LastKeyboardState = new KeyboardState();
            LastGamePadState = new GamePadState();
            LastMouseState = new MouseState();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
                LastKeyboardState = CurrentKeyboardState;
                LastGamePadState = CurrentGamePadState;
                LastMouseState = CurrentMouseState;

                CurrentKeyboardState = Keyboard.GetState();
                CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
                CurrentMouseState = Mouse.GetState();
        }

        #region input checks

        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        /// <param name="key">key to check</param>
        public bool IsNewKeyPress(Keys key)
        {

                return (CurrentKeyboardState.IsKeyDown(key) && 
                        LastKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        /// Checks if a key is being pressed
        /// </summary>
        public bool IsKeyPress(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the button has recently been pressed.
        /// </summary>
        /// <param name="button">button to check</param>
        public bool IsNewButtonPress(Buttons button)
        {

                return (CurrentGamePadState.IsButtonDown(button) &&
                        LastGamePadState.IsButtonUp(button));

        }

        /// <summary>
        /// Checks if a button is being pressed.
        /// </summary>
        /// <param name="button">button to check</param>
        public bool IsButtonPress(Buttons button)
        {

            return CurrentGamePadState.IsButtonDown(button);
        }

        /// <summary>
        /// Checks if the mouse button has been released
        /// </summary>
        public bool IsNewMouseReleased()
        {

            return (CurrentMouseState.LeftButton == ButtonState.Released &&
                LastMouseState.LeftButton == ButtonState.Pressed);

        }

        /// <summary>
        /// Checks if the mouse button has been Pressed
        /// </summary>
        public bool IsNewMousePressed()
        {

            return (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                LastMouseState.LeftButton == ButtonState.Released);

        }

        /// <summary>
        /// Checks if the mouse button is released
        /// </summary>
        public bool IsMouseReleased()
        {

            return CurrentMouseState.LeftButton == ButtonState.Released;

        }

        /// <summary>
        /// Checks if the mouse button is pressed
        /// </summary>
        public bool IsMousePressed()
        {

            return CurrentMouseState.LeftButton == ButtonState.Pressed;

        }

        /// <summary>
        /// Checks if the Mouse has recently received input
        /// </summary>
        public bool IsMouseChanged()
        {
            return CurrentMouseState != LastMouseState || IsMousePressed();
        }

        /// <summary>
        /// Checks if the Keyboard has recently recieved input
        /// </summary>
        public bool IsKeyboardChanged()
        {
            return CurrentKeyboardState != LastKeyboardState;
        }

        /// <summary>
        /// Checks if the GamePad has recently recieved input
        /// </summary>
        public bool IsGamePadChanged()
        {
            return CurrentGamePadState != LastGamePadState;
        }

        #endregion

        #region controls implementation

        /// <summary>
        /// Checks for a "menu select" input action.
        /// </summary>
        public bool IsFire()
        {
            return IsNewKeyPress(Keys.Space) ||
                   IsNewKeyPress(Keys.Enter) ||
                   IsNewButtonPress(Buttons.A);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// </summary>
        public bool IsExit()
        {
            return IsNewKeyPress(Keys.Escape) ||
                   IsNewKeyPress(Keys.Q) ||
                   IsNewButtonPress(Buttons.B) ||
                   IsNewButtonPress(Buttons.Back);
        }

        /// <summary>
        /// Checks for a "up" input action.
        /// </summary>
        public bool IsUp()
        {

            return IsKeyPress(Keys.Up) ||
                   IsButtonPress(Buttons.DPadUp) ||
                   IsButtonPress(Buttons.LeftThumbstickUp);
        }


        /// <summary>
        /// Checks for a "down" input action.
        /// </summary>
        public bool IsDown()
        {

            return IsKeyPress(Keys.Down) ||
                   IsButtonPress(Buttons.DPadDown) ||
                   IsButtonPress(Buttons.LeftThumbstickDown);
        }

        /// <summary>
        /// Checks for a "right" input action.
        /// </summary>
        public bool IsRight()
        {

            return IsKeyPress(Keys.Right) ||
                   IsButtonPress(Buttons.DPadRight) ||
                   IsButtonPress(Buttons.LeftThumbstickRight);
        }

        /// <summary>
        /// Checks for a "left" input action.
        /// </summary>
        public bool IsLeft()
        {

            return IsKeyPress(Keys.Left) ||
                   IsButtonPress(Buttons.DPadLeft) ||
                   IsButtonPress(Buttons.LeftThumbstickLeft);
        }

        /// <summary>
        /// Checks for a "menu up" input action.
        /// </summary>
        public bool IsUpAlt()
        {

            return IsKeyPress(Keys.W) ||
                   IsButtonPress(Buttons.RightThumbstickUp);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// </summary>
        public bool IsDownAlt()
        {

            return IsKeyPress(Keys.S) ||
                   IsButtonPress(Buttons.RightThumbstickDown);
        }

        /// <summary>
        /// Checks for a "menu right" input action.
        /// </summary>
        public bool IsRightAlt()
        {

            return IsKeyPress(Keys.D) ||
                   IsButtonPress(Buttons.RightThumbstickRight);
        }

        /// <summary>
        /// Checks for a "menu left" input action.
        /// </summary>
        public bool IsLeftAlt()
        {

            return IsKeyPress(Keys.A) ||
                   IsButtonPress(Buttons.RightThumbstickLeft);
        }

        /// <summary>
        /// Checks for a "Map" input action.
        /// </summary>
        public bool IsMap()
        {

            return IsNewKeyPress(Keys.M) ||
                   IsNewButtonPress(Buttons.Y);
        }

        #endregion

        #endregion
    }
}
