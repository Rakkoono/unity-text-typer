﻿namespace RedBlueGames.Tools.TextTyper
{
    using UnityEngine;

    public class TextSymbol
    {
        public TextSymbol Initialize(string character)
        {
            this.Tag = null;
            this.Character = character;

            return this;
        }

        public TextSymbol Initialize(RichTextTag tag)
        {
            this.Character = null;
            this.Tag = tag;

            return this;
        }

        public string Character { get; private set; }

        public RichTextTag Tag { get; private set; }

        public int Length
        {
            get
            {
                return this.Text.Length;
            }
        }

        public string Text
        {
            get
            {
                if (this.IsTag)
                {
                    return this.Tag.TagText;
                }
                else
                {
                    return this.Character;
                }
            }
        }

        public bool IsTag
        {
            get
            {
                return this.Tag != null;
            }
        }

        public float GetFloatParameter(float defaultValue = 0f)
        {
            if (!this.IsTag)
            {
                Debug.LogWarning("Attempted to retrieve parameter from symbol that is not a tag.");
                return defaultValue;
            }

            float paramValue;
            if (!float.TryParse(this.Tag.PrimaryParameter, out paramValue))
            {
                var warning = $"Found Invalid parameter format in tag [{this.Tag}]. "
                            + $"Parameter [{this.Tag.PrimaryParameter}] does not parse to a float.";
                Debug.LogWarning(warning);
                paramValue = defaultValue;
            }

            return paramValue;
        }
    }
}