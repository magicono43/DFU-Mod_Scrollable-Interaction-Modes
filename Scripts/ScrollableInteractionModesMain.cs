// Project:         Scrollable Interaction Modes mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    9/19/2023, 9:30 PM
// Last Edit:		3/20/2024, 9:30 PM
// Version:			1.10
// Special Thanks:  
// Modifier:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System;

namespace ScrollableInteractionModes
{
    public partial class ScrollableInteractionModesMain : MonoBehaviour
    {
        public static ScrollableInteractionModesMain Instance;

        static Mod mod;

        // General Options
        public static bool ReverseCycleDirection { get; set; }
        public static bool ModeLooping { get; set; }

        // Ignore Modes Options
        public static bool IgnoreModes { get; set; }
        public static bool StealMode { get; set; }
        public static bool GrabMode { get; set; }
        public static bool InfoMode { get; set; }
        public static bool TalkMode { get; set; }

        // Misc Options
        public static bool AllowMouseWheelCycling { get; set; }
        public static bool AllowKeyPressCycling { get; set; }
        public static KeyCode CycleModeKey { get; set; }

        // Variables
        public static bool[] interactModes = { true, true, true, true }; // Steal, Grab, Info, Talk

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<ScrollableInteractionModesMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Scrollable Interaction Modes");

            Instance = this;

            mod.LoadSettings();

            Debug.Log("Finished mod init: Scrollable Interaction Modes");
        }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            ReverseCycleDirection = mod.GetSettings().GetValue<bool>("GeneralSettings", "ReverseCycleDirections");
            ModeLooping = mod.GetSettings().GetValue<bool>("GeneralSettings", "AllowModeLooping");

            IgnoreModes = mod.GetSettings().GetValue<bool>("IgnoreModesSettings", "EnableIgnoreModes");
            StealMode = mod.GetSettings().GetValue<bool>("IgnoreModesSettings", "IgnoreStealMode");
            GrabMode = mod.GetSettings().GetValue<bool>("IgnoreModesSettings", "IgnoreGrabMode");
            InfoMode = mod.GetSettings().GetValue<bool>("IgnoreModesSettings", "IgnoreInfoMode");
            TalkMode = mod.GetSettings().GetValue<bool>("IgnoreModesSettings", "IgnoreTalkMode");

            AllowMouseWheelCycling = mod.GetSettings().GetValue<bool>("MiscSettings", "EnableMouseWheelCycling");
            AllowKeyPressCycling = mod.GetSettings().GetValue<bool>("MiscSettings", "EnableKeyPressCycling");
            var cycleKeyText = mod.GetSettings().GetValue<string>("MiscSettings", "CycleModesKey");
            if (Enum.TryParse(cycleKeyText, out KeyCode result))
                CycleModeKey = result;
            else
            {
                CycleModeKey = KeyCode.R;
                Debug.Log("Scrollable Interaction Modes: Invalid cycle modes keybind detected. Setting default. 'R' Key");
                DaggerfallUI.AddHUDText("Scrollable Interaction Modes:", 6f);
                DaggerfallUI.AddHUDText("Invalid cycle modes keybind detected. Setting default. 'R' Key", 6f);
            }

            if (IgnoreModes)
            {
                interactModes[0] = StealMode ? false : true;
                interactModes[1] = GrabMode ? false : true;
                interactModes[2] = InfoMode ? false : true;
                interactModes[3] = TalkMode ? false : true;
            }
        }

        private void Update()
        {
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            // Handle mouse wheel
            if (AllowMouseWheelCycling)
            {
                float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
                if (mouseScroll != 0)
                {
                    if (ReverseCycleDirection)
                    {
                        if (mouseScroll > 0)
                            CycleInteractionMode(false);
                        else if (mouseScroll < 0)
                            CycleInteractionMode(true);
                    }
                    else
                    {
                        if (mouseScroll > 0)
                            CycleInteractionMode(true);
                        else if (mouseScroll < 0)
                            CycleInteractionMode(false);
                    }
                }
            }

            // Handle key presses
            if (AllowKeyPressCycling && InputManager.Instance.GetAnyKeyDown() == CycleModeKey)
            {
                if (ReverseCycleDirection)
                {
                    CycleInteractionMode(false);
                }
                else
                {
                    CycleInteractionMode(true);
                }
            }    
        }

        void CycleInteractionMode(bool forward)
        {
            PlayerActivateModes nextMode = GameManager.Instance.PlayerActivate.CurrentMode;
            PlayerActivateModes currentMode = nextMode;
            int currentModeIndex = (int)currentMode;

            // Cycle to next interaction mode. Order is Steal > Grab > Info > Talk then wraps back to Steal, if looping is enabled.
            if (forward)
            {
                for (int i = 0; i < interactModes.Length; i++)
                {
                    currentModeIndex = Mathf.Clamp(currentModeIndex + 1, -1, 3);
                    if (interactModes[currentModeIndex]) { nextMode = (PlayerActivateModes)currentModeIndex; break; }
                }

                if (ModeLooping && nextMode == currentMode)
                {
                    currentModeIndex = -1;
                    for (int i = 0; i < interactModes.Length; i++)
                    {
                        currentModeIndex = Mathf.Clamp(currentModeIndex + 1, -1, 3);
                        if (interactModes[currentModeIndex]) { nextMode = (PlayerActivateModes)currentModeIndex; break; }
                    }
                }
            }
            // Cycle to previous interaction mode. Order is Talk > Info > Grab > Steal then wraps back to Talk, if looping is enabled.
            else
            {
                for (int i = 0; i < interactModes.Length; i++)
                {
                    currentModeIndex = Mathf.Clamp(currentModeIndex - 1, 0, 4);
                    if (interactModes[currentModeIndex]) { nextMode = (PlayerActivateModes)currentModeIndex; break; }
                }

                if (ModeLooping && nextMode == currentMode)
                {
                    currentModeIndex = 4;
                    for (int i = 0; i < interactModes.Length; i++)
                    {
                        currentModeIndex = Mathf.Clamp(currentModeIndex - 1, 0, 4);
                        if (interactModes[currentModeIndex]) { nextMode = (PlayerActivateModes)currentModeIndex; break; }
                    }
                }
            }
            GameManager.Instance.PlayerActivate.ChangeInteractionMode(nextMode);
        }
    }
}
