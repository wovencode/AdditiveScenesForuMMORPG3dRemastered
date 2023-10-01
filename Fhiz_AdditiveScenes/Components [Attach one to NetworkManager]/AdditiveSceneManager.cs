using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;
using Fhiz;

namespace Fhiz {

    /*
    * AdditiveSceneManager
    */
    [RequireComponent(typeof(NetworkManagerMMO))]
    [DisallowMultipleComponent]
    public partial class AdditiveSceneManager : MonoBehaviour
    {

        [Header("Components")]
        public NetworkManagerMMO manager;

        [Tooltip("Add all additive scenes in your project to this list")]
        public additiveScene[] additiveScenes;

        [Header("Debug")]
        public bool isActive = true;

        public static AdditiveSceneManager singleton;

        /* ******************************* CLASS EVENTS *********************************** */

        /*
        * Awake
        */
        void Awake()
        {
            // initialize singleton
            if (singleton == null) singleton = this;
        }

        /* ******************************* PUBLIC EVENTS *********************************** */

        /*
        * OnStopServer
        * @Server
        * Unload all additive scenes when the app quits
        */
        public void OnStopServer_AdditiveSceneManager()
        {
            StartCoroutine(UnloadAllAdditiveScenes());
        }

        /*
        * OnStopClient
        * @Client
        * Unload all additive scenes when the app quits
        */
        public void OnStopClient_AdditiveSceneManager()
        {
            StartCoroutine(UnloadAllAdditiveScenes());
        }

        /*
        * OnServerCharacterSelect_LoadAdditiveScenes (Event)
        */
        public void OnServerCharacterSelect_LoadAdditiveScenes(string account, GameObject go, NetworkConnection conn, CharacterSelectMsg message)
        {

            foreach (additiveScene adScene in additiveScenes)
            {
                // -- 1) self check: are we inside collider?
                // -- 2) other check: are there others in the scene?
                if (adScene.collider.bounds.Contains(go.transform.position) || adScene.level > 0)
                {

                    // Load scene on server
                    LoadAdditiveSceneAsync(adScene.name);

                    // Load on target client first (others are notified by onTriggerEnter)
                    SceneMessage msg = new SceneMessage { sceneName = adScene.name, sceneOperation = SceneOperation.LoadAdditive };
                    conn.Send(msg);

                }

            }

        }

        /* *************************** LOAD SCENE FUNCTIONS ******************************* */

        /*
        * LoadAdditiveSceneAsync
        * @Server / @Client
        * Checks & starts the scene loading process and adds the player to the scene
        * 
        */
        public void LoadAdditiveSceneAsync(string additiveSceneName, Player player = null)
        {

            // increase player count in scene if player was provided
            if (player)
                addPlayerToScene(additiveSceneName, player);
            
            if (!getSceneLoaded(additiveSceneName))
            {

                // Load on Server
                StartCoroutine(LoadAdditiveScene(additiveSceneName));
                setSceneLoaded(additiveSceneName, true);

                // Load on Clients if player was provided
                if (player)
                {
                    // Load on target Client first
                    SceneMessage message = new SceneMessage { sceneName = additiveSceneName, sceneOperation = SceneOperation.LoadAdditive };
                    player.netIdentity.connectionToClient.Send(message);

                    // Load on Clients around
                    player.GetComponent<PlayerAdditiveSceneController>().RpcClientLoadAdditiveScene(additiveSceneName);
                }

            }

        }

        /*
        * LoadAdditiveScene
        * @Server / @Client
        * The actual loading process of the scene (asynchronous)
        * 
        */
        IEnumerator LoadAdditiveScene(string additiveSceneName)
        {

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
                yield return null;

        }

        /* *************************** UNLOAD SCENE FUNCTIONS ***************************** */

        /*
        * UnloadAdditiveSceneAsync
        * @Server / @Client
        * Checks & starts the scene unloading process and removes the player from the scene
        * 
        */
        public void UnloadAdditiveSceneAsync(string additiveadditiveSceneName, Player player = null)
        {

            // decrease player count in scene if a player was provided
            if (player)
                removePlayerFromScene(additiveadditiveSceneName, player);

            // unload scene ONLY if there are no players left inside
            if (getSceneLoaded(additiveadditiveSceneName) && getSceneLevel(additiveadditiveSceneName) <= 0)
            {

                // Load on Server
                setSceneLoaded(additiveadditiveSceneName, false);
                StartCoroutine(UnloadAdditiveScene(additiveadditiveSceneName));

                // Unload on target client and clients around if a player was provided
                if (player)
                {

                    // Unload on target client first
                    SceneMessage message = new SceneMessage { sceneName = additiveadditiveSceneName, sceneOperation = SceneOperation.UnloadAdditive };
                    player.netIdentity.connectionToClient.Send(message);

                    // Unload on clients around
                    player.GetComponent<PlayerAdditiveSceneController>().RpcClientUnloadAdditiveScene(additiveadditiveSceneName);
                }

            }


        }

        /*
        * UnloadAdditiveScene
        * @Server / @Client
        * The actual unloading process of the scene (asynchronous)
        * 
        */
        IEnumerator UnloadAdditiveScene(string additiveadditiveSceneName)
        {

            if (SceneManager.GetSceneByName(additiveadditiveSceneName).IsValid() || SceneManager.GetSceneByPath(additiveadditiveSceneName).IsValid())
                SceneManager.UnloadSceneAsync(additiveadditiveSceneName);

            Resources.UnloadUnusedAssets();

            yield return null;
        }

        /*
        * UnloadAllAdditiveScenes
        * @Server / @Client
        *
        * unloads all scenes (usually called when the app quits) works on clients and server
        *
        */
        IEnumerator UnloadAllAdditiveScenes()
        {

            foreach (additiveScene sub in additiveScenes)
                if (SceneManager.GetSceneByName(sub.name).IsValid() || SceneManager.GetSceneByPath(sub.name).IsValid())
                {
                    setSceneLoaded(sub.name, false);
                    yield return SceneManager.UnloadSceneAsync(sub.name);
                }

            yield return Resources.UnloadUnusedAssets();

        }

        /* ******************************** HELPER FUNCTIONS ****************************** */

        /*
        * getSceneLoaded
        */
        public bool getSceneLoaded(string additiveSceneName)
        {

            if (string.IsNullOrWhiteSpace(additiveSceneName))
                return false;

            for (int i = 0; i < additiveScenes.Count(); ++i)
            {
                if (additiveScenes[i].name == additiveSceneName)
                    return additiveScenes[i].loaded;
            }

            return false;

        }

        /*
        * setSceneLoaded
        */
        public void setSceneLoaded(string additiveSceneName, bool loadStatus)
        {

            if (string.IsNullOrWhiteSpace(additiveSceneName))
                return;

            for (int i = 0; i < additiveScenes.Count(); ++i)
            {
                if (additiveScenes[i].name == additiveSceneName)
                    additiveScenes[i].loaded = loadStatus;
            }

        }

        /*
         * getSceneLevel
        */
        public int getSceneLevel(string additiveSceneName)
        {

            if (string.IsNullOrWhiteSpace(additiveSceneName))
                return 0;

            for (int i = 0; i < additiveScenes.Count(); ++i)
            {
                if (additiveScenes[i].name == additiveSceneName)
                    return additiveScenes[i].level;
            }

            return 0;

        }

        /*
        * addPlayerToScene
        */
        public void addPlayerToScene(string additiveSceneName, Player player)
        {

            if (string.IsNullOrEmpty(additiveSceneName) || player == null)
                return;

            for (int i = 0; i < additiveScenes.Count(); ++i)
                if (additiveScenes[i].name == additiveSceneName)
                    additiveScenes[i].players.Add(player);

        }

        /*
        * removePlayerFromScene
        */
        public void removePlayerFromScene(string additiveSceneName, Player player)
        {

            if (string.IsNullOrEmpty(additiveSceneName) || player == null)
                return;

            for (int i = 0; i < additiveScenes.Count(); ++i)
                if (additiveScenes[i].name == additiveSceneName)
                    additiveScenes[i].players.Remove(player);

        }

    }

}