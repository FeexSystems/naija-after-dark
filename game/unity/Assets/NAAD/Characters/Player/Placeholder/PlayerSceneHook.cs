using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Core.State;
using UnityEngine;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// WS3 seamless scene loop hook. When a scene is loaded (Bootstrap → Apartment
    /// → CityStreet → SuyaSpot → Nightclub), this component runs only once per scene
    /// and instantiates the player capsule at the registered spawn anchor.
    /// <para>
    /// On scene load, SceneLoader calls <see cref="OnSceneLoaded"/>.
    /// The player capsule spawns at the anchor's position and the camera targets it.
    /// This component is the WS3 placeholder that is later replaced by the rigged
    /// player prefab (WS4) once licensed rig assets exist.
    /// </para>
    /// </summary>
    public sealed class PlayerSceneHook : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private bool _autoSpawn = true;
        private Transform _playerInstance;

        private void Awake()
        {
            // Only own the rig — save this component, do not duplicate.
            if (transform.childCount > 0)
            {
                // We may be an empty marker — keep the first child as the player root.
            }
        }

        private void OnEnable()
        {
            // SceneLoader sets _playerPrefab via a non-serialized property before
            // OnEnable. The instance is created only once when the scene loads.
            if (_autoSpawn && _playerPrefab != null)
            {
                var anchor = FindObjectOfType<PlayerSpawn>();
                if (anchor != null)
                {
                    var go = Instantiate(_playerPrefab, anchor.Position, Quaternion.Euler(0f, anchor.FacingYaw, 0f));
                    _playerInstance = go.transform;
                }
            }
        }

        /// <summary>
        /// Called by the editor-only ScenePipeline after scene validation and
        /// before the Unity Editor enters Play Mode. Enables the player rig,
        /// attaches the camera target, and hands off to the scene loader.
        /// </summary>
        public void OnSceneLoaded()
        {
            if (_playerPrefab == null) return;
            if (_playerInstance == null)
            {
                var anchor = FindObjectOfType<PlayerSpawn>();
                if (anchor != null)
                {
                    _playerInstance = Instantiate(_playerPrefab, anchor.Position, Quaternion.Euler(0f, anchor.FacingYaw, 0f)).transform;
                }
            }
            _playerInstance?.gameObject.SetActive(true);
        }
    }
}