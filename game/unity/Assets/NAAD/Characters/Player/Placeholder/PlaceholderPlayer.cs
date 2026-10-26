using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Characters.Player;
using NAAD.Core.Logging;
using NAAD.Gameplay.Input;
using NAAD.Networking;
using NAAD.Core.State;
using UnityEngine;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// WS3 placeholder player — used while rig/gameplay assets are being
    /// acquired (Gate 4). Uses a capsule + CharacterController + the existing
    /// PlayerLocomotionController so the runtime scene loop (WS3) is proven
    /// immediately, before licensed rig assets exist.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerInputBridge))]
    [DisallowMultipleComponent]
    public sealed class PlaceholderPlayer : MonoBehaviour
    {
        /// <summary>Spawn anchor lookup; assigned by SceneLoader/ScenePipeline.</summary>
        [SerializeField]
        private PlayerSpawn _spawnAnchor;

        private CharacterController _characterController;
        private PlayerAnimator _animator;
        private PlayerInputBridge _inputBridge;
        private ThirdPersonCamera _camera;
        private NAADApplicationRoot _root;
        private IAuthService _auth;
        private IPlayerService _players;
        private ICommandService _commands;
        private GameStateManager _gameState;

        private Vector2 _moveInput;
        private bool _runHeld;

        /// <summary>
        /// Set after Awake() from the owning bootstrap. Do not reference before
        /// this fires.
        /// </summary>
        public NAADApplicationRoot Root => _root;

        public GameStateManager GameState => _gameState;

        private void Awake()
        {
            _root = NAADApplicationRoot.Instance;
            if (_root == null)
            {
                Debug.LogWarning("[PlaceholderPlayer] No NAADApplicationRoot; stub services only.");
                return;
            }

            _gameState = _root.GameState;
            _auth = _root.Auth;
            _players = _root.Players;
            _commands = _root.Commands;

            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<PlayerAnimator>();
            _inputBridge = GetComponent<PlayerInputBridge>();
            _camera = GetComponent<ThirdPersonCamera>();

            if (_characterController == null)
            {
                Debug.LogError("[PlaceholderPlayer] CharacterController missing. A capsule will not move.");
            }
            if (_animator == null)
            {
                Debug.LogError("[PlaceholderPlayer] PlayerAnimator missing.");
            }
            if (_inputBridge == null)
            {
                Debug.LogError("[PlaceholderPlayer] PlayerInputBridge missing.");
            }

            // Wire the animator (locomotion + one-shot overlay triggers)
            _inputBridge.OnMoveInput += OnMoveInput;
        }

        private void OnMoveInput(Vector2 input, bool runHeld)
        {
            _moveInput = input;
            _runHeld = runHeld;

            if (_animator == null) return;

            var move = _moveInput.magnitude;
            _animator.SetLocomotion(move, move, _runHeld && move > 0.2f, move > 0.01f);
        }

        private void Update()
        {
            if (_characterController == null) return;

            // Server-authoritative location remains in the service layer.
            // This component owns only local traversal/presentation.
            _characterController.Move(_moveInput * 2.0f * Time.deltaTime);

            // Keep camera locked to the player
            if (_camera != null)
            {
                _camera.SetTarget(transform);
            }
        }

        /// <summary>
        /// Entry point used by SceneLoader after scene load. Sets the camera target
        /// and initializes the jump-start from the spawn anchor.
        /// </summary>
        public void OnSceneEntered()
        {
            _camera?.SetTarget(transform);

            // If we have a spawn anchor, face its yaw and snap position.
            if (_spawnAnchor != null)
            {
                transform.position = _spawnAnchor.Position;
                transform.rotation = Quaternion.Euler(0f, _spawnAnchor.FacingYaw, 0f);
            }
        }
    }
}