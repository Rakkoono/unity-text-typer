namespace RedBlueGames.Tools.TextTyper
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Class that tests TextTyper and shows how to interface with it.
    /// </summary>
    public class TextTyperTester : MonoBehaviour
    {
        [SerializeField]
        private AudioClip printSoundEffect = null;

        [Header("UI References")]

        [SerializeField]
        private Button nextButton = null;

        [SerializeField]
        private Toggle autoToggle = null;

        [SerializeField]
        [Tooltip("The text typer element to test typing with")]
        private TextTyper testTextTyper = null;

        [Header("Dialogue")]

        [SerializeField, TextArea(1, 5)]
        private string[] lines = { };
        private Queue<string> dialogueLines = new Queue<string>();

        private bool auto;

        private AudioSource audioSource;
        public AudioSource AudioSource
        {
            get
            {
                audioSource ??= GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
                return audioSource;
            }
        }

        public void Start()
        {
            this.testTextTyper.PrintCompleted.AddListener(this.HandlePrintCompleted);
            this.testTextTyper.CharacterPrinted.AddListener(this.HandleCharacterPrinted);

            this.nextButton.onClick.AddListener(this.HandleNextButtonClicked);
            this.autoToggle.onValueChanged.AddListener(this.HandleAutoToggleChanged);

            this.dialogueLines = new Queue<string>(lines);
            ShowDialogue();
        }

        private void HandleNextButtonClicked()
        {
            if (this.testTextTyper.IsSkippable() && this.testTextTyper.IsTyping)
                this.testTextTyper.Skip();
            else
                ShowDialogue();
        }

        private void HandleAutoToggleChanged(bool value) => auto = value;

        private void ShowDialogue(TextTyperConfig config = null)
        {
            if (this.dialogueLines.Count <= 0)
                dialogueLines = new Queue<string>(lines);

            this.testTextTyper.TypeText(this.dialogueLines.Dequeue(), config);
        }

        private void HandleCharacterPrinted(string printedCharacter)
        {
            // Do not play a sound for whitespace
            if (printedCharacter == " " || printedCharacter == "\n")
                return;

            AudioSource.clip = this.printSoundEffect;
            AudioSource.Play();
        }

        private void HandlePrintCompleted()
        {
            if (auto)
                ShowDialogue();
        }
    }
}