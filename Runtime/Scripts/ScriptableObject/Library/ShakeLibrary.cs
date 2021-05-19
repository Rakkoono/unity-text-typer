namespace RedBlueGames.Tools.TextTyper
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ShakePreset : IPreset
    {
        [SerializeField, Tooltip("Name identifying this preset. Can also be used as a ShakeLibrary indexer key.")]
        private string name;
        public string Name => name;

        [Range(0, 20)]
        [Tooltip("Amount of x-axis shake to apply during animation")]
        public float xPosStrength = 0f;

        [Range(0, 20)]
        [Tooltip("Amount of y-axis shake to apply during animation")]
        public float yPosStrength = 0f;

        [Range(0, 90)]
        [Tooltip("Amount of rotational shake to apply during animation")]
        public float RotationStrength = 0f;

        [Range(0, 10)]
        [Tooltip("Amount of scale shake to apply during animation")]
        public float ScaleStrength = 0f;
    }

    [CreateAssetMenu(fileName = "ShakeAnimationLibrary", menuName = "Text Typer/Shake Animation Library", order = 1)]
    public class ShakeLibrary : Library<ShakePreset> { }
}