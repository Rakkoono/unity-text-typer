namespace RedBlueGames.Tools.TextTyper
{
    using System;
    using UnityEngine;

    public enum Tag
    {
        TwoSided,
        OneSidedOnOpen,
        OneSidedOnClose,
        OneSidedOnBoth,
    }

    [Serializable]
    public class TagPreset : IPreset
    {

        [SerializeField, Tooltip("Name identifying this preset. Can also be used as a TagLibrary indexer key.")]
        private string name = "";
        public string Name => name;

        [Tooltip("String inserted after the opening tag.")]
        public string prefix = "";

        [Tooltip("String inserted before the closing tag.")]
        public string suffix = "";

        [Tooltip("If set, the closing tags get inserted immediately after opening.")]
        public bool CloseImmediately = false;

        [Tooltip("Tags applied every time this tag is used.")]
        public SubTagPreset[] subTags = new SubTagPreset[1];
    }

    [Serializable]
    public class SubTagPreset : IPreset
    {
        [Tooltip("A Unity tag (b, i, ...), TextTyper tag (anim, speed, ...) or another library defined tag.")]
        public string tag = "";
        public string Name => tag;

        [Tooltip("If this tag has to be opened and closed (e.g. <b>Text</b>) and where it should be inserted if it is one sided (e.g. <sprite=0>)")]
        public Tag type = Tag.TwoSided;

        [Tooltip("The argument applied to the tag e.g. '#000000' for 'color' or 'slowsine' for 'anim'. Leave empty for tags without arguments.")]
        public string argument = "";

        [Tooltip("Index of the parameter (starting at 1) which overrides the argument above if supplied. Never overrides if it is 0.")]
        public int overrideArgument = 0;

        [Tooltip("If set, this tag gets skipped when no argument or override is provided.")]
        public bool argumentRequired = false;

        [Tooltip("This string gets inserted in the opening tag. It can contain secondary arguments like the 'name=\"SpriteAsset\"' used in the <sprite> tag. Use %param=INDEX% (e.g. %param=1%) to insert a parameter.")]
        public string secondaryArguments = "";
    }

    [CreateAssetMenu(fileName = "TagLibrary", menuName = "Text Typer/Tag Library", order = 1)]
    public class TagLibrary : Library<TagPreset> { }
}