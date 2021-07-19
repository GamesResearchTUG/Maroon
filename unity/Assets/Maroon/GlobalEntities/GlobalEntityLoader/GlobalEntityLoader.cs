using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maroon
{
    /// <summary>
    ///     Handles the creation and instantiation of global entities.
    ///
    ///     TODO: The input should just be an array of globalEntities, and they should be instantiated in a loop. But
    ///           this works for now.
    /// </summary>
    public class GlobalEntityLoader : MonoBehaviour
    {
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Fields

        // Singleton instance
        private static GlobalEntityLoader _instance = null;

        // Prefabs
        
        [SerializeField] private GameObject _platformManagerPrefab = null;
        [SerializeField] private GameObject _sceneManagerPrefab = null;
        [SerializeField] private GameObject _bootstrappingManagerPrefab = null;
        [SerializeField] private GameObject _globalInputManagerPrefab = null;
        [SerializeField] private GameObject _gameManagerPrefab = null;
        [SerializeField] private GameObject _soundManagerPrefab = null;
        [SerializeField] private GameObject _networkManagerPrefab = null;



        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Getters and Properties

        public static GlobalEntityLoader Instance
        {
            get { return GlobalEntityLoader._instance; }
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Methods

        private void Awake()
        {
            // ---------------------------------------------------------------------------------------------------------
            // Singleton
            if(GlobalEntityLoader._instance == null)
            {
                GlobalEntityLoader._instance = this;
            }
            else if(GlobalEntityLoader._instance != this)
            {                
                DestroyImmediate(this.gameObject);
                return;
            }

            // ---------------------------------------------------------------------------------------------------------
            // Keep alive
            DontDestroyOnLoad(this.gameObject);

            // ---------------------------------------------------------------------------------------------------------
            // Create instances

            // PlatformManager first, SceneManager and BootstrappingManager need to know on which platform we are
            if(Maroon.PlatformManager.Instance == null)
            {
                Instantiate(this._platformManagerPrefab).transform.SetParent(this.transform);
            }

            // SceneManager second, Bootstrapping manager needs to know on which scene we started to decide what to do
            if(Maroon.SceneManager.Instance == null)
            {
                Instantiate(this._sceneManagerPrefab).transform.SetParent(this.transform);
            }

            // BootstrappingManager third, we need to setup and redirect at this point
            if(Maroon.BootstrappingManager.Instance == null)
            {
                Instantiate(this._bootstrappingManagerPrefab).transform.SetParent(this.transform);
            }

            // Order not thought through yet
            if(Maroon.GlobalInputManager.Instance == null)
            {
                Instantiate(this._globalInputManagerPrefab).transform.SetParent(this.transform);
            }

            // Order not thought through yet
            if(Maroon.GameManager.Instance == null)
            {
                Instantiate(this._gameManagerPrefab).transform.SetParent(this.transform);
            }

            // Order not thought through yet
            if(Maroon.SoundManager.Instance == null)
            {
                Instantiate(this._soundManagerPrefab).transform.SetParent(this.transform);
            }

            // Order not thought through yet
            if(Maroon.NetworkManager.Instance == null)
            {
                Instantiate(this._networkManagerPrefab).transform.SetParent(this.transform);
            }
        }
    }
}