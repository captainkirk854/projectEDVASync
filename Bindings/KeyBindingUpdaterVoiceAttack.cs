﻿namespace Bindings
{
    using Helpers;
    using System.Data;
    using System.Xml.XPath;
    using System.Xml.Linq;
    using System.Linq;

    /// <summary>
    /// Update Voice Attack Profile Command(s) with new Key Codes
    /// </summary>
    public class KeyBindingUpdaterVoiceAttack : IKeyBindingUpdater
    {
        // Initialise ..
        private const string XMLCommand = "Command";
        private const string XMLActionSequence = "ActionSequence";
        private const string XMLCommandAction = "CommandAction";
        private const string XMLActionId = "Id";
        private const string XMLKeyCodes = "KeyCodes";
        private const string XMLunsignedShort = "unsignedShort";
        
        /// <summary>
        /// Update Voice Attack Profile with adjusted KeyCode(s) from Elite Dangerous Key Bindings
        /// </summary>
        /// <param name="consolidatedkeybindings"></param>
        /// <returns></returns>
        public bool Write(DataTable consolidatedkeybindings)
        {
            bool profileUpdated = false;

            // Find VoiceAttack commands which require remapping ..
            var consolidatedBindings = from cb in consolidatedkeybindings.AsEnumerable()
                                      where cb.Field<string>(Enums.Column.KeyUpdateRequired.ToString()) == Enums.KeyUpdateRequired.YES.ToString()
                                     select
                                        new
                                            {
                                                VAAction = cb.Field<string>(Enums.Column.VoiceAttackAction.ToString()),
                                                VAKeyCode = cb.Field<string>(Enums.Column.VoiceAttackKeyCode.ToString()),
                                                VAKeyId = cb.Field<string>(Enums.Column.VoiceAttackKeyId.ToString()),
                                                VAP = cb.Field<string>(Enums.Column.VoiceAttackProfile.ToString()),
                                                EDKeyCode = cb.Field<string>(Enums.Column.EliteDangerousKeyCode.ToString()),
                                                EDModifierKeyCode = cb.Field<string>(Enums.Column.EliteDangerousModifierKeyCode.ToString())
                                            };

            // Perform Update(s) for those commands that require it ..
            foreach (var consolidatedBinding in consolidatedBindings)
            {
                // Align key code with that used in Elite Dangerous ..
                this.UpdateVoiceAttackKeyCode(consolidatedBinding.VAP, consolidatedBinding.VAKeyId.Trim(), consolidatedBinding.EDKeyCode);

                // Align modifier key code by creating additional element for modifier ..
                if (int.Parse(consolidatedBinding.EDModifierKeyCode) > 0)
                {
                    this.InsertVoiceAttackModifierKeyCode(consolidatedBinding.VAP, consolidatedBinding.VAKeyId.Trim(), consolidatedBinding.EDModifierKeyCode);
                }
                profileUpdated = true;
            }

            return profileUpdated;
        }

        /// <summary>
        /// Add KeyCode as <unsignedShort> for modifier associated to specific [Id] in Voice Attack
        /// </summary>
        /// <param name="vaprofile"></param>
        /// <param name="vakeyId"></param>
        /// <param name="keyCode"></param>
        private void InsertVoiceAttackModifierKeyCode(string vaprofile, string vakeyId, string keyCode)
        {
            var vap = Xml.ReadXDoc(vaprofile);

            // Search through all <unsignedShort> elements whose grandparent (Parent.Parent) <id> element equals vakeyId
            // and add a new <unsignedShort> element with keyCode value physically before existing one ..
            vap.Descendants(XMLunsignedShort)
               .Where(item => item.Parent.Parent.Element(XMLActionId).Value == vakeyId).FirstOrDefault()
               .AddBeforeSelf(new XElement(XMLunsignedShort, keyCode));

            vap.Save(vaprofile);
        }

        /// <summary>
        /// Update Key Code associated to specific [Id] in Voice Attack
        /// </summary>
        /// <remarks>
        ///   Format: XML
        ///             o <Profile/>
        ///               |_ <Commands/>
        ///                  |_ <Command/>
        ///                      |_<Id/>
        ///                      !_<CommandString/>
        ///                      |_<ActionSequence/>
        ///                        !_[some] <CommandAction/>
        ///                                 !_<Id/>
        ///                                 |_<ActionType/> = PressKey
        ///                                 |_<KeyCodes/>
        ///                                   (|_<unsignedShort/> = when modifier present)
        ///                                    |_<unsignedShort/>
        /// </remarks>
        /// <param name="vaprofile"></param>
        /// <param name="vakeyId"></param>
        /// <param name="keyCode"></param>
        private void UpdateVoiceAttackKeyCode(string vaprofile, string vakeyId, string keyCode)
        {
            // Read Voice Attack Profile ...
            var vap = Xml.ReadXDoc(vaprofile);

            // Construct XPathSelect to locate element (XMLunsignedShort) for update using XMLActionId ..
            string xPathSelect = "//" + XMLCommand +
                                  "/" + XMLActionSequence +
                                  "/" + XMLCommandAction +
                                  "/" + XMLActionId + "[text() = '" + vakeyId + "']" +
                                  "/.." +
                                  "/" + XMLKeyCodes +
                                  "/" + XMLunsignedShort;

            // Update located element with new keyCode value ..
            vap.XPathSelectElement(xPathSelect).Value = keyCode;

            // Save file ..
            vap.Save(vaprofile);
        }
    }
}
