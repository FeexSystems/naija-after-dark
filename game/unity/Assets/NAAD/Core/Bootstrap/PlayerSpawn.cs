using UnityEngine;

namespace NAAD.Core.Bootstrap
{
    /// <summary>
    /// Marks a valid player spawn point within a scene.
    /// <para>
    /// Place one <see cref="PlayerSpawn"/> GameObject in every environment scene
    /// (Apartment, CityStreet, SuyaSpot, Nightclub, Beach). The
    /// <see cref="FirstNightController"/> and scene transition logic read this
    /// component to position the player avatar after travel.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public sealed class PlayerSpawn : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField]
        [Tooltip("If true, the player is spawned at this point when the scene loads.")]
        private bool _spawnOnLoad = true;

        [SerializeField]
        [Tooltip("Optional offset applied to the spawn position.")]
        private Vector3 _offset = Vector3.zero;

        [SerializeField]
        [Tooltip("Y-axis rotation (degrees) the player should face on spawn.")]
        private float _facingYaw = 0f;

        /// <summary>World-space position of the spawn point, including offset.</summary>
        public Vector3 Position => transform.position + _offset;

        /// <summary>Yaw rotation the player should face on spawn.</summary>
        public float FacingYaw => _facingYaw;

        /// <summary>Whether this spawn should be used automatically on scene load.</summary>
        public bool SpawnOnLoad => _spawnOnLoad;

        private void OnValidate()
        {
            // Ensure a collider exists so designers can click the gizmo in the Scene view.
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<SphereCollider>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Position, 0.5f);
            Gizmos.DrawLine(Position, Position + Quaternion.Euler(0f, _facingYaw, 0f) * Vector3.forward * 1f);
        }
    }
}