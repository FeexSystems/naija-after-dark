using UnityEngine;
using NAAD.Core.Bootstrap;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// WS3 Player placeholder prefab.
    /// <para>
    /// Capsule root with existing <see cref="PlayerLocomotionController"/> +
    /// <see cref="PlayerAnimator"/> + <see cref="PlayerInputBridge"/> +
    /// <see cref="ThirdPersonCamera"/> wired. Ready to be replaced with the
    /// rigged player prefab (WS4) once licensed rig assets exist.
    /// </para>
    /// </summary>
    public sealed class PlaceholderPlayerPrefab : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private GameObject _playerRoot;

        [Header("Scene Integration")]
        [SerializeField] private PlayerSpawn _spawnAnchor;

        private void Awake()
        {
            if (_playerRoot == null)
            {
                _playerRoot = CreateCapsuleRoot();
            }

            EnsureComponents();
        }

        private void OnEnable()
        {
            // ScenePipeline calls OnSceneLoaded() to spawn the player at a registered
            // PlayerSpawn anchor when entering Play Mode.
            var hook = GetComponent<PlayerSceneHook>();
            hook?.OnSceneLoaded();
        }

        private GameObject CreateCapsuleRoot()
        {
            var root = new GameObject("Placeholder Player");
            root.SetActive(false);

            var col = root.AddComponent<CharacterController>();
            col.height = 1.7f;
            col.center = new Vector3(0f, 0.9f, 0f);
            col.radius = 0.35f;

            return root;
        }

        private void EnsureComponents()
        {
            if (_playerRoot == null) return;

            if (_playerRoot.GetComponent<PlayerLocomotionController>() == null)
                _playerRoot.AddComponent<PlayerLocomotionController>();

            if (_playerRoot.GetComponent<PlayerAnimator>() == null)
                _playerRoot.AddComponent<PlayerAnimator>();

            if (_playerRoot.GetComponent<PlayerInputBridge>() == null)
                _playerRoot.AddComponent<PlayerInputBridge>();

            if (_playerRoot.GetComponent<ThirdPersonCamera>() == null)
                _playerRoot.AddComponent<ThirdPersonCamera>();

            if (_playerRoot.GetComponent<PlaceholderPlayer>() == null)
                _playerRoot.AddComponent<PlaceholderPlayer>();
        }
    }
}