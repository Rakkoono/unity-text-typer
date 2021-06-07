namespace RedBlueGames.Tools.TextTyper
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TagPreset : IPreset
    {
        [SerializeField, Tooltip("Name identifying this preset. Can also be used as a TagLibrary indexer key.")]
        private string name = "";
        public string Name => name;

        [Tooltip(@"String that replaces the opening tag.

:: Replacement ::

${} will get replaced with the primary parameter. Will remove assignment if there is no primary parameter.

${PARAM} will get replaced with the parameter PARAM. Will remove assignment if there is no parameter PARAM.

${}{ALT_VALUE} will insert the ALT_VALUE if there is no primary parameter.

${PARAM}{ALT_VALUE} will insert the ALT_VALUE if there is no parameter PARAM.

:: Conditional ::

$if{PARAM}[...] will insert the text in the square brackets only if PARAM is found.

$if{PARAM}[...][...] will insert the text in the second square bracket if PARAM isn't found.")]
        [TextArea]
        public string onOpeningTag = "";

        [Tooltip(@"String that replaces the closing tag.

:: Replacement ::

${} will get replaced with the primary parameter. Will remove assignment if there is no primary parameter.

${PARAM} will get replaced with the parameter PARAM. Will remove assignment if there is no parameter PARAM.

${}{ALT_VALUE} will insert the ALT_VALUE if there is no primary parameter.

${PARAM}{ALT_VALUE} will insert the ALT_VALUE if there is no parameter PARAM.

:: Conditional ::

$if{PARAM}[...] will insert the text in the square brackets only if PARAM is found.

$if{PARAM}[...][...] will insert the text in the second square bracket if PARAM isn't found.")]
        [TextArea]
        public string onClosingTag = "";

        [Space(2)]

        [Tooltip("If set, the closing tags get inserted immediately after opening.")]
        public bool CloseImmediately = false;
    }

    [CreateAssetMenu(fileName = "TagLibrary", menuName = "Text Typer/Tag Library", order = 1)]
    public class TagLibrary : Library<TagPreset> { }
}