﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace Maroon
{
    public class GlobalInputManager : MonoBehaviour
    {
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Fields

        // Singleton instance
        private static GlobalInputManager _instance = null;

        // Action maps, actions, and bindings generated by the Unity input system package
        private MaroonInputActions _maroonInputActions = null;

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Properties, Getters and Setters

        // -------------------------------------------------------------------------------------------------------------
        // Singleton

        /// <summary>
        ///     The GlobalInputManager instance
        /// </summary>
        public static GlobalInputManager Instance
        {
            get
            {
                return GlobalInputManager._instance;
            }
        }

        /// <summary>
        ///     Action maps, actions, and bindings generated by the Unity input system package
        /// </summary>
        public MaroonInputActions MaroonInputActions
        {
            get
            {
                return this._maroonInputActions;
            }
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Methods

        // -------------------------------------------------------------------------------------------------------------
        // Initialization

        /// <summary>
        ///     Called by Unity. Initializes singleton instance and DontDestroyOnLoad (stays active on new scene load).
        ///     Initializes the input actions.
        /// </summary>
        private void Awake()
        {
            // Singleton
            if(GlobalInputManager._instance == null)
            {
                GlobalInputManager._instance = this;
            }
            else if(GlobalInputManager._instance != this)
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            // Keep alive
            DontDestroyOnLoad(this.gameObject);

            // Initialize input actions
            this._maroonInputActions = new MaroonInputActions();

            Debug.Log("SET INPUT ACTIONS");
        }

        /// <summary>
        ///     Called by Unity. Enables Unity input system package.
        /// </summary>
        private void OnEnable()
        {
            // Enable the input system
            _maroonInputActions.Enable();
        }

        /// <summary>
        ///     Called by Unity. Disables Unity input system package.
        /// </summary>
        private void OnDisable()
        {
            // Disable the input system
            _maroonInputActions.Disable();
        }
    }
}