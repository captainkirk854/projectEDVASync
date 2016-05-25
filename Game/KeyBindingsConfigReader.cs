﻿namespace Game
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using System.Data;
    using Helpers;

    /// <summary>
    /// Reads and process Key Binding Configuration Files
    /// </summary>
    public static class KeyBindingsConfigReader
    {
        // Preset Key Map Enumeration to use ..
        static Enums.KeyType KeyType = Enums.KeyType.WindowsForms;

        // Initialise ..
        static readonly KeyMapper KeyMap = new KeyMapper(KeyType);
        static DataTable KeyBindingsTable = new DataTable();
        const string D = "+";

        /// <summary>
        /// Constructor initialises DataTable structure ..
        /// </summary>
        static KeyBindingsConfigReader()
        {
            DefineKeyBindingsTableStructure(KeyBindingsTable);
        }

        /// <summary>
        /// Parse Elite Dangerous Key Bindings into DataTable
        /// </summary>
        /// <param name="cfgFilePath"></param>
        /// <returns></returns>
        public static DataTable EliteDangerous(string cfgFilePath)
        {
            // Load configuration file as xml object ..
            var EDCfg = Xml.ReadXDoc(cfgFilePath);

            // Read configuration xml and convert to DataTable ..
            DataTable primary = ExtractKeyBindings_EliteDangerous(EDCfg, Enums.EliteDangerousDevicePriority.Primary);
            DataTable secondary = ExtractKeyBindings_EliteDangerous(EDCfg, Enums.EliteDangerousDevicePriority.Secondary);

            // Merge ..
            primary.Merge(secondary);

            // Return merged DataTable contents ..
            return primary;
        }

        /// <summary>
        /// Parse Voice Attack Key Bindings into DataTable
        /// </summary>
        /// <param name="cfgFilePath"></param>
        /// <returns></returns>
        public static DataTable VoiceAttack(string cfgFilePath)
        {
            // Load configuration file as xml object .. 
            var VACfg = Xml.ReadXDoc(cfgFilePath);

            // Return as DataTable  ..
            return ExtractKeyBindings_VoiceAttack(VACfg);
        }

        /// <summary>
        /// Define Key Bindings DataTable Structure
        /// </summary>
        /// <param name="KeyBindings"></param>
        private static void DefineKeyBindingsTableStructure(DataTable KeyBindings)
        {
            KeyBindings.TableName = "KeyBindings";

            // Define table structure ..
            KeyBindings.Columns.Add("Context", typeof(string));
            KeyBindings.Columns.Add("KeyEnumerationType", typeof(string));
            KeyBindings.Columns.Add("KeyFunction", typeof(string));
            KeyBindings.Columns.Add("Priority", typeof(string));
            KeyBindings.Columns.Add("KeyValue", typeof(string));
            KeyBindings.Columns.Add("KeyCode", typeof(string));
            KeyBindings.Columns.Add("KeyId", typeof(string));
            KeyBindings.Columns.Add("ModifierKeyValue", typeof(string));
            KeyBindings.Columns.Add("ModifierKeyCode", typeof(string));
            KeyBindings.Columns.Add("ModifierKeyId", typeof(string));
        }

        /// <summary>
        /// Process Voice Attack Config File
        /// Format: XML
        ///             o <Profile/>
        ///               |_ <Commands/>
        ///                  |_<ActionSequences/>
        ///                     !_[some] <CommandActions/>
        ///                              |_<ActionType/>
        ///                             
        /// Selected Keys: Use actual Key code (as opposed to key value)
        ///                e.g. 
        ///                   ((Shield Cell)) : 222 (= Oem7 Numpad?7)
        ///                   ((Power To Weapons)) : 39  (= Right arrow)
        ///                   ((Select Target Ahead)) : 84 (= T)
        ///                   ((Flight Assist)) : 90 (= Z)
        /// </summary>
        /// <param name="xdoc"></param>
        private static DataTable ExtractKeyBindings_VoiceAttack(XDocument xdoc)
        {
            // Initialise ..
            const string VAKeyBoardInteraction = "PressKey";
            const string KeyBindingContext = "VoiceAttack";

            // traverse config XML and gather pertinent element data arranged in row(s) of anonymous types ..
            var keyBindings = from item in xdoc.Descendants("Command")
                             where item.Element("ActionSequence").Element("CommandAction") != null &&
                                   item.Element("ActionSequence").Element("CommandAction").Element("ActionType").Value == VAKeyBoardInteraction
                            select 
                               new // create anonymous type for every key code ..
                                 {
                                    Commandstring = item.Element("CommandString").SafeElementValue(),
                                    Id = item.Element("ActionSequence").Element("CommandAction").Element("Id").SafeElementValue(),
                                    KeyCode = item.Element("ActionSequence").Element("CommandAction").Element("KeyCodes").Element("unsignedShort").SafeElementValue()
                                 };

            // insert anonymous type row data (with some additional values) into DataTable ..
            foreach (var keyBinding in keyBindings)
            {
                KeyBindingsTable.LoadDataRow(new object[] 
                                                {
                                                 KeyBindingContext, //Context
                                                 KeyMap.KeyType.ToString(), //KeyMappingType
                                                 keyBinding.Commandstring, //KeyFunction
                                                 "N/A", //Priority
                                                 KeyMap.GetValue(Int32.Parse(keyBinding.KeyCode)), //KeyValue
                                                 keyBinding.KeyCode, //KeyCode
                                                 keyBinding.Id, //KeyId
                                                 "N/A", //ModifierKeyValue
                                                 "N/A", //ModifierKeyCode
                                                 "N/A" //ModifierId
                                                }
                                             , false);
            }

            // return Datatable ..
            return KeyBindingsTable;
        }

        /// <summary>
        /// Process Elite Dangerous Config File
        /// Keys can be in assigned with Primary or Secondary priorities
        /// Format: XML
        ///             o <Root/>
        ///               |_ <KeyboardLayout/>
        ///               |_ <things></things>.[Value] attribute
        ///               |_ <things/>
        ///                  |_<Binding/>
        ///                  |_<Inverted/>
        ///                  |_<Deadzone/>
        ///               |_ <things/>
        ///                  |_<Primary/>
        ///                  |_<Secondary/>
        ///               |_ <things/>
        ///                  |_<Primary/>
        ///                     |_<Modifier/>
        ///                  |_<Secondary/>
        ///                     |_<Modifier/>
        ///               |_ <things/>
        ///                  |_<Primary/>
        ///                  |_<Secondary/>
        ///                  |_<ToggleOn/>
        ///                  
        /// Selected Keys: Use actual Key value (as opposed to key code)
        ///                e.g. 
        ///                     SystemMapOpen : Key_B
        ///                     GalaxyMapOpen : Key_M
        ///                   FocusRightPanel : Key_4
        ///                       SetSpeedZero: Key_0 + Key_RightShift (modifier)
        ///                
        /// </summary>
        /// <param name="xdoc"></param>
        private static DataTable ExtractKeyBindings_EliteDangerous(XDocument xdoc, Enums.EliteDangerousDevicePriority devicepriority)
        {
            // Initialise ..
            const string KeyBindingContext = "EliteDangerous";
            const string EDKeyBoardInteraction = "Keyboard";
            const string XMLKey = "Key";
            const string XMLDevice = "Device";
            const string XMLModifier = "Modifier";
            string DevicePriority = devicepriority.ToString();

            // traverse config XML and gather pertinent element data arranged in row(s) of anonymous types ..
            // Scan all child nodes from top-level node ..
            foreach (var childNode in xdoc.Element("Root").Elements())
            {
                // can only process if child node itself has children ..
                if (childNode.DescendantNodes().Any())
                {
                    var keyBindings = from item in xdoc.Descendants(childNode.Name)
                                     where
                                           item.Element(DevicePriority).SafeAttributeValue(XMLDevice) == EDKeyBoardInteraction &&
                                           item.Element(DevicePriority).Attribute(XMLKey).Value.Contains("Key_") == true
                                     select
                                        new // create anonymous type for every key code ..
                                          {
                                              //---------------------------------------------------------------------------------
                                              // Priority ..
                                              //---------------------------------------------------------------------------------
                                              xmlNode_DevicePriority = item.Element(DevicePriority).Attribute(XMLKey).Parent.Name,
                                            
                                              //---------------------------------------------------------------------------------
                                              // Main Key Binding ..
                                              //---------------------------------------------------------------------------------
                                              xmlNode_Device = item.Element(DevicePriority).SafeAttributeName(XMLDevice),
                                              xmlNode_Key = item.Element(DevicePriority).SafeAttributeName(XMLKey),
                                              DeviceType = item.Element(DevicePriority).SafeAttributeValue(XMLDevice),
                                              KeyValueFull = item.Element(DevicePriority).SafeAttributeValue(XMLKey),
                                              KeyValue = item.Element(DevicePriority).SafeAttributeValue(XMLKey) != string.Empty ? item.Element(DevicePriority).SafeAttributeValue(XMLKey).Substring(4) : string.Empty,

                                              //---------------------------------------------------------------------------------
                                              // Modifier Key Binding (should it exist) ..
                                              //---------------------------------------------------------------------------------
                                              xmlNode_Modifier = item.Element(DevicePriority).Element(XMLModifier).SafeElementName(),
                                              xmlNode_ModifierDevice = item.Element(DevicePriority).Element(XMLModifier).SafeAttributeName(XMLDevice),
                                              xmlNode_ModifierKey = item.Element(DevicePriority).Element(XMLModifier).SafeAttributeName(XMLKey),
                                              ModifierDeviceType = item.Element(DevicePriority).Element(XMLModifier).SafeAttributeValue(XMLDevice),
                                              ModifierKeyValueFull = item.Element(DevicePriority).Element(XMLModifier).SafeAttributeValue(XMLKey),
                                              ModifierKeyValue = item.Element(DevicePriority).Element(XMLModifier).SafeAttributeValue(XMLKey) != string.Empty ? item.Element(DevicePriority).Element(XMLModifier).SafeAttributeValue(XMLKey).Substring(4) : string.Empty
                                          };

                    foreach (var keyBinding in keyBindings)
                    {
                        string CustomKeyId = childNode.Name + D +
                                             keyBinding.xmlNode_DevicePriority + D +
                                             keyBinding.xmlNode_Device + D +
                                             keyBinding.DeviceType + D +
                                             keyBinding.xmlNode_Key + D +
                                             keyBinding.KeyValueFull;

                        string CustomModifierKeyId = string.Empty;
                        if (keyBinding.xmlNode_Modifier != string.Empty)
                        {
                            CustomModifierKeyId = childNode.Name + D +
                                                  keyBinding.xmlNode_DevicePriority + D +
                                                  keyBinding.xmlNode_Modifier + D +
                                                  keyBinding.xmlNode_ModifierDevice + D +
                                                  keyBinding.ModifierDeviceType + D +
                                                  keyBinding.xmlNode_ModifierKey + D +
                                                  keyBinding.ModifierKeyValueFull;
                        }

                        KeyBindingsTable.LoadDataRow(new object[] 
                                                        {
                                                         KeyBindingContext, //Context
                                                         KeyMap.KeyType.ToString(), //KeyMappingType
                                                         childNode.Name, //KeyFunction
                                                         keyBinding.xmlNode_DevicePriority, //Priority 
                                                         keyBinding.KeyValue, //KeyValue
                                                         KeyMap.GetCode(keyBinding.KeyValue), //KeyCode
                                                         CustomKeyId, //KeyId
                                                         keyBinding.ModifierKeyValue, //ModifierKeyValue
                                                         KeyMap.GetCode(keyBinding.ModifierKeyValue),//ModifierKeyCode
                                                         CustomModifierKeyId //ModifierId
                                                        }
                                                     , false);
                    }
                }
            }

            // return Datatable ..
            return KeyBindingsTable;
        }     
    }
}