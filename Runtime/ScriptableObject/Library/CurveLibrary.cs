namespace RedBlueGames.Tools.TextTyper
{
    using System;
    using UnityEngine;

    [Serializable]
    public class CurvePreset : IPreset
    {
        [SerializeField, Tooltip("Name identifying this preset. Can also be used as a CurveLibrary indexer key.")]
        private string name;
        public string Name => name;

        [Tooltip("Time offset between each character when calculating animation transform. 0 makes all characters move together. Other values produce a 'wave' effect.")]
        [Range(0f, 0.5f)]
        public float timeOffsetPerChar = 0f;

        [Tooltip("Curve showing x-position delta over time")]
        public AnimationCurve xPosCurve;
        [Tooltip("x-position curve is multiplied by this value")]
        [Range(0, 20)]
        public float xPosMultiplier = 0f;

        [Tooltip("Curve showing y-position delta over time")]
        public AnimationCurve yPosCurve;
        [Tooltip("y-position curve is multiplied by this value")]
        [Range(0, 20)]
        public float yPosMultiplier = 0f;

        [Tooltip("Curve showing 2D rotation delta over time")]
        public AnimationCurve rotationCurve;
        [Tooltip("2D rotation curve is multiplied by this value")]
        [Range(0, 90)]
        public float rotationMultiplier = 0f;

        [Tooltip("Curve showing uniform scale delta over time")]
        public AnimationCurve scaleCurve;
        [Tooltip("Uniform scale curve is multiplied by this value")]
        [Range(0, 10)]
        public float scaleMultiplier = 0f;
    }

    [CreateAssetMenu(fileName = "CurveAnimationLibrary", menuName = "Text Typer/Curve Animation Library", order = 1)]
    public class CurveLibrary : Library<CurvePreset>
    { }
}