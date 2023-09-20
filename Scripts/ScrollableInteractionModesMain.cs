// Project:         Scrollable Interaction Modes mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    9/19/2023, 9:30 PM
// Last Edit:		9/20/2023, 6:20 AM
// Version:			1.00
// Special Thanks:  
// Modifier:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop;

namespace ScrollableInteractionModes
{
    public partial class ScrollableInteractionModesMain : MonoBehaviour
    {
        public static ScrollableInteractionModesMain Instance;

        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<ScrollableInteractionModesMain>(); // Add script to the scene.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Scrollable Interaction Modes");

            Instance = this;

            Debug.Log("Finished mod init: Scrollable Interaction Modes");
        }

        private void Update()
        {
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            // Handle mouse wheel
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScroll != 0)
            {
                if (mouseScroll > 0)
                    NextInteractionMode();
                else if (mouseScroll < 0)
                    PreviousInteractionMode();
            }
        }

        /// <summary>
        /// Cycle to next interaction mode.
        /// Order is Steal > Grab > Info > Talk then wraps back to Steal.
        /// </summary>
        void NextInteractionMode()
        {
            PlayerActivateModes nextMode;
            switch (GameManager.Instance.PlayerActivate.CurrentMode)
            {
                case PlayerActivateModes.Steal:
                    nextMode = PlayerActivateModes.Grab;
                    break;
                case PlayerActivateModes.Grab:
                    nextMode = PlayerActivateModes.Info;
                    break;
                case PlayerActivateModes.Info:
                    nextMode = PlayerActivateModes.Talk;
                    break;
                case PlayerActivateModes.Talk:
                    nextMode = PlayerActivateModes.Steal;
                    break;
                default:
                    nextMode = GameManager.Instance.PlayerActivate.CurrentMode;
                    break;
            }
            GameManager.Instance.PlayerActivate.ChangeInteractionMode(nextMode);
        }

        /// <summary>
        /// Cycle to previous interaction mode.
        /// Order is Talk > Info > Grab > Steal then wraps back to Talk.
        /// </summary>
        void PreviousInteractionMode()
        {
            PlayerActivateModes nextMode;
            switch (GameManager.Instance.PlayerActivate.CurrentMode)
            {
                case PlayerActivateModes.Talk:
                    nextMode = PlayerActivateModes.Info;
                    break;
                case PlayerActivateModes.Info:
                    nextMode = PlayerActivateModes.Grab;
                    break;
                case PlayerActivateModes.Grab:
                    nextMode = PlayerActivateModes.Steal;
                    break;
                case PlayerActivateModes.Steal:
                    nextMode = PlayerActivateModes.Talk;
                    break;
                default:
                    nextMode = GameManager.Instance.PlayerActivate.CurrentMode;
                    break;
            }
            GameManager.Instance.PlayerActivate.ChangeInteractionMode(nextMode);
        }
    }
}
