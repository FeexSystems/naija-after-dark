using System;
using UnityEngine;

namespace NAAD.Gameplay.World
{
    /// <summary>
    /// Day/night lighting profiles for the Night District.
    /// Each profile defines ambient, directional, and fog parameters for a game period
    /// (MORNING / DAY / TRANSITION / NIGHT / LATE_NIGHT / AFTER_HOURS).
    /// </summary>
    [CreateAssetMenu(fileName = "NightLightingProfile", menuName = "NAAD/World/Lighting Profile", order = 100)]
    public sealed class NightLightingProfile : ScriptableObject
    {
        [Header("Period")]
        public string Period = "MORNING";

        [Header("Ambient")]
        [SerializeField] private Color _ambientColor = Color.white;
        [SerializeField] private float _ambientIntensity = 0.4f;
        [SerializeField] private float _ambientMode = 3f;

        [Header("Directional (Sun/Moon)")]
        [SerializeField] private Color _directionalColor = Color.white;
        [SerializeField] private Gradient _sunSkyboxHDR;
        [SerializeField] private Vector3 _sunDirection = new Vector3(0, -0.5f, 0.8f);
        [SerializeField] private float _sunIntensity = 0.6f;

        [Header("Skybox")]
        [SerializeField] private Material _skyboxMaterial;

        [Header("Fog")]
        [SerializeField] private Color _fogColor = Color.clear;
        [SerializeField] private float _fogDensity = 0.01f;

        [Header("Nightlife")]
        [SerializeField] private float _nightlifeLevel = 45;

        public static NightLightingProfile Default => ScriptableObject.CreateInstance<NightLightingProfile>();
    }
}
