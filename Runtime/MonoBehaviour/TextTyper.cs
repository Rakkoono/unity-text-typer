﻿namespace RedBlueGames.Tools.TextTyper
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Type text component types out Text one character at a time. Heavily adapted from synchrok's GitHub project.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TextTyper : MonoBehaviour
    {
        /// <summary>
        /// The delay time between each print.
        /// </summary>
        public const float PrintDelay = 0.02f;

        /// <summary>
        /// The amount of characters to be printed each time.
        /// </summary>
        public const int PrintAmount = 1;

        /// <summary>
        /// Default delay setting will be multiplied by this when the character is a punctuation mark
        /// </summary>
        public const float PunctuationDelayMultiplier = 8f;

        /// <summary>
        /// Characters that are considered punctuation in this language. TextTyper pauses on these characters
        /// a bit longer by default. Could be a setting sometime since this doesn't localize.
        /// </summary>
        private static readonly List<string> _punctuations = new List<string>
        {
            ".",
            ",",
            "!",
            "?",
            ";",
            ":"
        };

        /// <summary>
        /// Characters that are considered punctuation in this language. TextTyper pauses on these characters
        /// a bit longer by default. Could be a setting sometime since this doesn't localize.
        /// </summary>
        public static IEnumerable<string> Punctuations
        {
            get
            {
                return _punctuations;
            }
        }

        [SerializeField]
        [Tooltip("The configuration that overrides default settings.")]
        private TextTyperConfig config = null;

        [SerializeField]
        [Tooltip("The library of ShakePreset animations that can be used by this component.")]
        private ShakeLibrary shakeLibrary = null;

        [SerializeField]
        [Tooltip("The library of CurvePreset animations that can be used by this component.")]
        private CurveLibrary curveLibrary = null;

        [SerializeField]
        [Tooltip("The library of TagPreset animations that can be used by this component.")]
        private TagLibrary tagLibrary = null;

        [SerializeField]
        [Tooltip("If set, the typer will type text even if the game is paused (Time.timeScale = 0).")]
        private bool useUnscaledTime = false;

        [SerializeField]
        [Tooltip("If set, animation and custom tags will be resolved on awake.")]
        private bool initializeOnAwake = false;

        [SerializeField]
        [Tooltip("Event that's called when the text has finished printing.")]
        private UnityEvent printCompleted = new UnityEvent();

        [SerializeField]
        [Tooltip("Event called when a character is printed. Inteded for audio callbacks.")]
        private CharacterPrintedEvent characterPrinted = new CharacterPrintedEvent();

        private TMP_Text textComponent;
        private TextTyperConfig defaultConfig;
        private Coroutine typeTextCoroutine;

        private string text;
        private string taglessText;

        private readonly List<float> characterPrintDelays;
        private readonly List<TextAnimation> animations;
        private readonly List<TextSymbol> textAsSymbolList;

        /// <summary>
        /// Gets the PrintCompleted callback event.
        /// </summary>
        /// <value>The print completed callback event.</value>
        public UnityEvent PrintCompleted
        {
            get
            {
                return this.printCompleted;
            }
        }

        /// <summary>
        /// Gets the CharacterPrinted event, which includes a string for the character that was printed.
        /// </summary>
        /// <value>The character printed event.</value>
        public CharacterPrintedEvent CharacterPrinted
        {
            get
            {
                return this.characterPrinted;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="TextTyper"/> is currently printing text.
        /// </summary>
        /// <value><c>true</c> if printing; otherwise, <c>false</c>.</value>
        public bool IsTyping
        {
            get
            {
                return this.typeTextCoroutine != null;
            }
        }

        private TMP_Text TextComponent
        {
            get
            {
                if (this.textComponent == null)
                {
                    this.textComponent = GetComponent<TMP_Text>();
                }

                return this.textComponent;
            }
        }

        /// <summary>
        /// Gets the number of characters that have been printed.
        /// This number will be reset each time this <see cref="TextTyper"/> starts printing text.
        /// </summary>
        public int PrintedCharacters { get; private set; }

        private void Awake()
        {
            if (initializeOnAwake)
            {
                string text = this.TextComponent.text;
                TypeText(text, config, text.Length);
            }
        }

        public TextTyper()
        {
            this.characterPrintDelays = new List<float>();
            this.animations = new List<TextAnimation>();
            this.textAsSymbolList = new List<TextSymbol>();
        }

        private float GetPrintDelay()
        {
            if (this.defaultConfig)
                return this.defaultConfig.PrintDelay;

            return PrintDelay;
        }

        private int GetPrintAmount()
        {
            if (this.defaultConfig)
                return this.defaultConfig.PrintAmount;

            return PrintAmount;
        }

        private float GetPunctuationDelayMultiplier()
        {
            if (this.defaultConfig)
                return this.defaultConfig.PunctuationDelayMultiplier;

            return PunctuationDelayMultiplier;
        }

        private List<string> GetPunctutations()
        {
            if (this.defaultConfig)
                return this.defaultConfig.Punctuations;

            return _punctuations;
        }

        /// <summary>
        /// Types the text into the Text component character by character, using the specified (optional) print delay per character.
        /// </summary>
        /// <param name="text">Text to type.</param>
        /// <param name="printDelay">Print delay (in seconds) per character.</param>
        /// <param name="skipChars">The number of characters to be already typed initially.</param>
        /// <param name="printAmount">The amount of characters to be printed each time.</param>
        public void TypeText(string text, float printDelay = -1, int skipChars = 0, int printAmount = -1)
        {
            TypeText(text, this.config, printDelay, skipChars, printAmount);
        }

        /// <summary>
        /// Types the text into the Text component character by character, using the specified (optional) print delay per character.
        /// </summary>
        /// <param name="text">Text to type.</param>
        /// <param name="config">The alternated config. If null the <see cref="TextTyper.config"/> will be used.</param>
        /// <param name="skipChars">The number of characters to be already typed initially.</param>
        public void TypeText(string text, TextTyperConfig config, int skipChars = 0)
        {
            TypeText(text, config, -1f, skipChars, -1);
        }

        private void TypeText(string text, TextTyperConfig config, float printDelay, int skipChars, int printAmount)
        {
            CleanupCoroutine();

            // Remove all existing TextAnimations
            // TODO - Would be better to pool/reuse these components
            foreach (var anim in GetComponents<TextAnimation>())
            {
                Destroy(anim);
            }

            this.defaultConfig = config ? config : this.config;

            // Save text
            this.text = text;
            TextTagParser.CreateSymbolListFromText(text, this.textAsSymbolList);

            ProcessCustomTags(printDelay > 0 ? printDelay : GetPrintDelay());

            if (skipChars < 0)
                skipChars = 0;

            if (printAmount < 1)
                printAmount = GetPrintAmount();

            this.typeTextCoroutine = StartCoroutine(TypeTextCharByChar(skipChars, printAmount));
        }

        /// <summary>
        /// Pauses the typing.
        /// </summary>
        public void Pause()
        {
            CleanupCoroutine();
        }

        /// <summary>
        /// Resume the typing.
        /// </summary>
        /// <param name="printDelay">Print delay (in seconds) per character.</param>
        /// <param name="skipChars">The number of characters to be already typed initially.</param>
        /// <param name="printAmount">The amount of characters to be printed each time.</param>
        public void Resume(float printDelay = -1, int? skipChars = null, int printAmount = -1)
        {
            Resume(this.config, printDelay, skipChars ?? this.PrintedCharacters, printAmount);
        }

        /// <summary>
        /// Resume the typing.
        /// </summary>
        /// <param name="config">The alternated config. If null the <see cref="TextTyper.config"/> will be used.</param>
        /// <param name="skipChars">The number of characters to be already typed initially.</param>
        public void Resume(TextTyperConfig config, int? skipChars = null)
        {
            Resume(config, -1f, skipChars ?? this.PrintedCharacters, -1);
        }

        private void Resume(TextTyperConfig config, float printDelay, int skipChars, int printAmount)
        {
            if (skipChars >= this.taglessText.Length)
                return;

            CleanupCoroutine();

            this.defaultConfig = config ? config : this.config;
            ProcessCustomTags(printDelay > 0 ? printDelay : GetPrintDelay());

            if (skipChars < 0)
                skipChars = 0;

            if (printAmount < 1)
                printAmount = GetPrintAmount();

            this.typeTextCoroutine = StartCoroutine(ResumeTypingTextCharByChar(skipChars, printAmount));
        }

        /// <summary>
        /// Skips the typing to the end.
        /// </summary>
        public void Skip()
        {
            CleanupCoroutine();

            this.TextComponent.maxVisibleCharacters = int.MaxValue;
            UpdateMeshAndAnims();

            OnTypewritingComplete();
        }

        /// <summary>
        /// Determines whether this instance is skippable.
        /// </summary>
        /// <returns><c>true</c> if this instance is skippable; otherwise, <c>false</c>.</returns>
        public bool IsSkippable()
        {
            // For now there's no way to configure this. Just make sure it's currently typing.
            return this.IsTyping;
        }

        private void CleanupCoroutine()
        {
            if (this.typeTextCoroutine != null)
            {
                StopCoroutine(this.typeTextCoroutine);
                this.typeTextCoroutine = null;
            }
        }

        private IEnumerator TypeTextCharByChar(int skipChars, int printAmount)
        {
            this.taglessText = TextTagParser.RemoveAllTags(text, tagLibrary);
            var totalChars = this.taglessText.Length;
            this.PrintedCharacters = Mathf.Clamp(skipChars, 0, totalChars);

            this.TextComponent.SetText(TextTagParser.RemoveLibraryTags(TextTagParser.RemoveCustomTags(text), tagLibrary));

            while (this.PrintedCharacters < totalChars)
            {
                this.PrintedCharacters = Mathf.Clamp(this.PrintedCharacters + printAmount, 0, totalChars);
                var index = this.PrintedCharacters - 1;
                this.TextComponent.maxVisibleCharacters = this.PrintedCharacters;

                UpdateMeshAndAnims();
                OnCharacterPrinted(this.taglessText[index].ToString());

                var delay = this.characterPrintDelays[index];

                if (this.useUnscaledTime)
                    yield return new WaitForSecondsRealtime(delay);
                else
                    yield return new WaitForSeconds(delay);
            }

            this.typeTextCoroutine = null;
            OnTypewritingComplete();
        }

        private IEnumerator ResumeTypingTextCharByChar(int skipChars, int printAmount)
        {
            var totalChars = this.taglessText.Length;
            this.PrintedCharacters = Mathf.Clamp(skipChars, 0, totalChars);

            while (this.PrintedCharacters < totalChars)
            {
                this.PrintedCharacters = Mathf.Clamp(this.PrintedCharacters + printAmount, 0, totalChars);
                var index = this.PrintedCharacters - 1;
                this.TextComponent.maxVisibleCharacters = this.PrintedCharacters;

                UpdateMeshAndAnims();
                OnCharacterPrinted(this.taglessText[index].ToString());

                var delay = this.characterPrintDelays[index];

                if (this.useUnscaledTime)
                    yield return new WaitForSecondsRealtime(delay);
                else
                    yield return new WaitForSeconds(delay);
            }

            this.typeTextCoroutine = null;
            OnTypewritingComplete();
        }

        private void UpdateMeshAndAnims()
        {
            // This must be done here rather than in each TextAnimation's OnTMProChanged
            // b/c we must cache mesh data for all animations before animating any of them

            // Update the text mesh data (which also causes all attached TextAnimations to cache the mesh data)
            this.TextComponent.ForceMeshUpdate();

            // Force animate calls on all TextAnimations because TMPro has reset the mesh to its base state
            // NOTE: This must happen immediately. Cannot wait until end of frame, or the base mesh will be rendered
            for (var i = 0; i < this.animations.Count; i++)
            {
                this.animations[i].AnimateAllChars();
            }
        }

        /// <summary>
        /// Calculates print delays for every visible character in the string.
        /// Processes delay tags, punctuation delays, and default delays
        /// Also processes shake and curve animations and spawns
        /// the appropriate TextAnimation components
        /// </summary>
        private void ProcessCustomTags(float printDelay)
        {
            this.characterPrintDelays.Clear();
            this.animations.Clear();

            var printedCharCount = 0;
            var customTagOpenIndex = 0;
            var customTagParams = new Stack<string>();
            var userDefinedTagStack = new Stack<RichTextTag>();

            var nextDelay = printDelay;
            var punctuations = GetPunctutations();
            var punctuationDelayMultiplier = GetPunctuationDelayMultiplier();

            int indexInText = 0;
            for (int i = 0; i < this.textAsSymbolList.Count; i++)
            {
                var symbol = this.textAsSymbolList[i];
                indexInText += symbol.Length;

                if (symbol.IsTag)
                {
                    TagPreset tagPreset = default(TagPreset);
                    if (tagLibrary != null)
                        tagPreset = tagLibrary.Presets.FirstOrDefault(tag => symbol.Tag.Equals(tag.Name));

                    if (tagPreset != default(TagPreset))
                    {
                        // Wrapping the same user defined tag twice won't work properly
                        RichTextTag tag;
                        if (symbol.Tag.IsOpeningTag)
                            userDefinedTagStack.Push(tag = symbol.Tag);
                        else
                        {
                            tag = userDefinedTagStack.Pop();
                            var otherTags = new Stack<RichTextTag>();
                            while (tag.TagType != symbol.Tag.TagType)
                            {
                                otherTags.Push(tag);
                                tag = userDefinedTagStack.Pop();
                            }
                            foreach (var otherTag in otherTags)
                                userDefinedTagStack.Push(otherTag);
                        }

                        // Construct tag string
                        string tags = "";
                        if (symbol.Tag.IsOpeningTag)
                        {
                            // Add On Opening Tag string
                            tags = tags + tagPreset.onOpeningTag;

                            // If close immediately, insert closing tag
                            if (tagPreset.CloseImmediately)
                                tags += "</" + tagPreset.Name + ">";
                        }
                        else
                        {
                            // Add On Closing Tag string
                            tags = tagPreset.onClosingTag + tags;
                        }

                        // Replace $if{}[][]
                        string ifRegex =
                            @"\$if{([^{}]*)}" +     // Matches "$if{PARAM}"     ($1)
                            @"\[([^\[\]]*)\]" +     // Matches "[IF]"           ($2)
                            @"(\[[^\[\]]*\])?";     // Matches "[ELSE]"         (Optional, $3 in {})
                        tags = Regex.Replace(tags, ifRegex, match =>
                        {
                            string value = tag.GetParameter(match.Groups[1].Value);
                            if (value != "")
                                return match.Groups[2].Value;

                            string altValueMatch = match.Groups[3].Value;
                            return altValueMatch == "" ? "" : altValueMatch.Substring(1, altValueMatch.Length - 2);
                        });

                        // Replace <...=${}{}...>
                        string tagRegex =
                            @"(<[\w]+=)" +      // Matches "<TAG="          ($1)
                            @"\${([^{}]*)}" +   // Matches "${PARAM}"       ($2)
                            @"({[^{}]*})?" +    // Matches "{ALT VALUE}"    (Optional, $3 in {})
                            @"([^<>]*>)";       // Matches "ANY STRING>"    ($4)
                        tags = Regex.Replace(tags, tagRegex, EvaluateMatch);

                        // Replace ...${}{}...
                        string paramRegex =
                            @"([\w]*=?)" +      // Matches "ARG(=)"         ($1)
                            @"\${([^{}]*)}" +   // Matches "${PARAM}"       ($2)
                            @"({[^{}]*})?" +    // Matches "{ALT_VALUE}"    (Optional, $3 in {})
                            @"()";              // Matches ""               ($4)
                        tags = Regex.Replace(tags, paramRegex, EvaluateMatch);

                        string EvaluateMatch(Match match) {
                            string before = match.Groups[1].Value;
                            string after = match.Groups[4].Value;

                            string value = tag.GetParameter(match.Groups[2].Value);
                            if (value != "")
                                return before + value + after;

                            string altValue = match.Groups[3].Value;
                            if (altValue != "")
                                return before + altValue.Substring(1, altValue.Length - 2) + after;

                            return "";
                        }

                        // Insert tag string in text and symbol list
                        this.text = this.text.Insert(indexInText, tags);
                        var tagsAsSymbolList = new List<TextSymbol>();
                        TextTagParser.CreateSymbolListFromText(tags, tagsAsSymbolList);
                        this.textAsSymbolList.InsertRange(i + 1, tagsAsSymbolList);
                    }
                    else
                    {
                        // Save tag parameters
                        string tagParam;
                        if (symbol.Tag.IsOpeningTag)
                            customTagParams.Push(tagParam = symbol.Tag.PrimaryParameter);
                        else
                            tagParam = customTagParams.Pop();

                        if (symbol.Tag.Equals(TextTagParser.CustomTags.Delay))
                        {
                            if (symbol.Tag.IsClosingTag)
                            {
                                nextDelay = printDelay;
                            }
                            else
                            {
                                nextDelay = symbol.GetFloatParameter(printDelay);
                            }
                        }
                        else if (symbol.Tag.Equals(TextTagParser.CustomTags.Speed))
                        {
                            if (symbol.Tag.IsClosingTag)
                            {
                                nextDelay = printDelay;
                            }
                            else
                            {
                                var speed = symbol.GetFloatParameter(1f);

                                if (Mathf.Approximately(speed, 0f))
                                {
                                    nextDelay = printDelay;
                                }
                                else if (speed < 0f)
                                {
                                    nextDelay = printDelay * Mathf.Abs(speed);
                                }
                                else if (speed > 0f)
                                {
                                    nextDelay = printDelay / Mathf.Abs(speed);
                                }
                                else
                                {
                                    nextDelay = printDelay;
                                }
                            }
                        }
                        else if (symbol.Tag.Equals(TextTagParser.CustomTags.Anim) ||
                                 symbol.Tag.Equals(TextTagParser.CustomTags.Animation))
                        {
                            if (symbol.Tag.IsClosingTag)
                            {
                                // Add a TextAnimation component to process this animation
                                TextAnimation anim = null;
                                if (IsAnimationShake(tagParam))
                                {
                                    anim = this.gameObject.AddComponent<ShakeAnimation>();
                                    ((ShakeAnimation)anim).LoadPreset(this.shakeLibrary, tagParam);
                                }
                                else if (IsAnimationCurve(tagParam))
                                {
                                    anim = this.gameObject.AddComponent<CurveAnimation>();
                                    ((CurveAnimation)anim).LoadPreset(this.curveLibrary, tagParam);
                                }
                                else
                                {
                                    Debug.LogWarning("Animation '" + tagParam + "' not found.");
                                    continue;
                                }

                                anim.UseUnscaledTime = this.useUnscaledTime;
                                anim.SetCharsToAnimate(customTagOpenIndex, printedCharCount - 1);
                                anim.enabled = true;
                                this.animations.Add(anim);
                            }
                            else
                                customTagOpenIndex = printedCharCount;
                        }
                        else if (!TextTagParser.UnityTags.Contains(symbol.Tag.TagType))
                        {
                            Debug.LogWarning("Tag '" + symbol.Tag.TagType + "' not found.");
                        }
                    }
                }
                else
                {
                    printedCharCount++;

                    if (punctuations.Contains(symbol.Character))
                    {
                        this.characterPrintDelays.Add(nextDelay * punctuationDelayMultiplier);
                    }
                    else
                    {
                        this.characterPrintDelays.Add(nextDelay);
                    }
                }
            }
        }

        private bool IsAnimationShake(string animName) => this.shakeLibrary.ContainsKey(animName);

        private bool IsAnimationCurve(string animName) => this.curveLibrary.ContainsKey(animName);

        private void OnCharacterPrinted(string printedCharacter)
        {
            if (this.CharacterPrinted != null)
                this.CharacterPrinted.Invoke(printedCharacter);
        }

        private void OnTypewritingComplete()
        {
            if (this.PrintCompleted != null)
                this.PrintCompleted.Invoke();
        }

        /// <summary>
        /// Event that signals a Character has been printed to the Text component.
        /// </summary>
        [System.Serializable]
        public class CharacterPrintedEvent : UnityEvent<string> { }
    }
}
