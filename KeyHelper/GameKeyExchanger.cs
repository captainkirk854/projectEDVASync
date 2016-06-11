﻿namespace KeyHelper
{
    using Helper;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Dictionary of Key Bindings
    /// </summary>
    public class GameKeyExchanger
    {
        private Dictionary<string, string> relationship = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GameKeyExchanger"/> class
        /// </summary>
        public GameKeyExchanger()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameKeyExchanger"/> class
        /// </summary>
        /// <param name="game"></param>
        public GameKeyExchanger(Helper.Enums.Game game)
        {
            this.Initialise(game);
        }

        /// <summary>
        /// Initialise Mapping Dictionary for certain Game Keys: [Game -> WindowsForm]
        /// </summary>
        /// <param name="game"></param>
        public void Initialise(Helper.Enums.Game game)
        {
            if (game == Helper.Enums.Game.EliteDangerous)
            {
                this.relationship.Add("0", "D0");
                this.relationship.Add("1", "D1");
                this.relationship.Add("2", "D2");
                this.relationship.Add("3", "D3");
                this.relationship.Add("4", "D4");
                this.relationship.Add("5", "D5");
                this.relationship.Add("6", "D6");
                this.relationship.Add("7", "D7");
                this.relationship.Add("8", "D8");
                this.relationship.Add("9", "D9");
                this.relationship.Add("Numpad_0", "NumPad0");
                this.relationship.Add("Numpad_1", "NumPad1");
                this.relationship.Add("Numpad_2", "NumPad2");
                this.relationship.Add("Numpad_3", "NumPad3");
                this.relationship.Add("Numpad_4", "NumPad4");
                this.relationship.Add("Numpad_5", "NumPad5");
                this.relationship.Add("Numpad_6", "NumPad6");
                this.relationship.Add("Numpad_7", "NumPad7");
                this.relationship.Add("Numpad_8", "NumPad8");
                this.relationship.Add("Numpad_9", "NumPad9");
                this.relationship.Add("Numpad_Enter", "Return");
                this.relationship.Add("Numpad_Multiply", "Multiply");
                this.relationship.Add("Numpad_Add", "Add");
                this.relationship.Add("Numpad_Minus", "Subtract");
                this.relationship.Add("Numpad_Subtract", "Subtract");
                this.relationship.Add("Numpad_Divide", "Divide");
                this.relationship.Add("Numpad_Decimal", "Decimal");
                this.relationship.Add("PageDown", "Next");
                this.relationship.Add("LeftArrow", "Left");
                this.relationship.Add("UpArrow", "Up");
                this.relationship.Add("RightArrow", "Right");
                this.relationship.Add("DownArrow", "Down");
                this.relationship.Add("Enter", "Return");
                this.relationship.Add("LeftShift", "LShiftKey");
                this.relationship.Add("RightShift", "RShiftKey");
                this.relationship.Add("LeftControl", "LControlKey");
                this.relationship.Add("RightControl", "RControlKey");
                this.relationship.Add("LeftBracket", "OemOpenBrackets");
                this.relationship.Add("RightBracket", "Oem6");
                this.relationship.Add("Grave", "Oem8");
                this.relationship.Add("ScrollLock", "Scroll");
                this.relationship.Add("Dash", "Separator");
                this.relationship.Add("Minus", "OemMinus");
                this.relationship.Add("Backspace", "Back");
                this.relationship.Add("Period", "OemPeriod");
                this.relationship.Add("Comma", "Oemcomma");
                this.relationship.Add("Equals", "Oemplus");
                this.relationship.Add("Slash", "Oem5");
                this.relationship.Add("Semicolon", "Oem1");
                this.relationship.Add("Hash", "Oem7");
                this.relationship.Add("ForwardSlash", "OemQuestion");
                this.relationship.Add("Tilde", "OemTilde");
                this.relationship.Add("BackSlash", "OemBackSlash");
            }
        }

        /// <summary>
        /// Get Windows Key Name
        /// </summary>
        /// {Dictionary Value}
        /// <param name="keyName"></param>
        /// <returns></returns>
        public string GetValue(string keyName)
        {
            try
            {
                return this.relationship[keyName];
            }
            catch
            {
                return keyName;
            }
        }

        /// <summary>
        /// Get Game Key Name
        /// </summary>
        /// {Dictionary Key}
        /// <param name="keyName"></param>
        /// <returns></returns>
        public string GetKey(string keyName)
        {
            return this.relationship.FirstOrDefault(x => x.Value == keyName).Key;
        }
    }
}