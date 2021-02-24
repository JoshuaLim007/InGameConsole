using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lim.InGameConsole.Parser;
using UnityEngine.EventSystems;

namespace Lim.InGameConsole.Visual
{
    [DefaultExecutionOrder(-4)]
    [ExecuteAlways]
    public class IGC_VisualManager : MonoBehaviour
    {
        private static IGC_VisualManager instance;
        [Tooltip("Toggle canvas in Editor")]
        [SerializeField] private bool ShowPreview = false;

        private Vector2 HistoryTextPadding = new Vector2(0.98f, 0.98f);
        private Vector2 SuggTextPadding = new Vector2(0.98f, 1f);

        [Header("Settings")]
        [SerializeField] private float InputFieldSize = 35;
        [SerializeField] private float SuggestionFieldSize = 35;
        [SerializeField] private float HistoryFontSize = 25;

        [Header("References")]
        [SerializeField] private TMP_InputField CommandInputField = null;
        [SerializeField] private TextMeshProUGUI HistoryField = null;
        [SerializeField] private TextMeshProUGUI SuggestionsField = null;
        [SerializeField] private Image Background = null;
        private Image suggestionBackground = null;

        readonly float SuggestionFontSize = 36;
        readonly float InputFontSize = 15;
        readonly float inputFieldYOffset = 34;
        private Canvas canvas;
        private int SuggestionsCount = 1;

        public void SetSize(float InputFieldSize = 35, float SuggestionFieldSize = 35, float HistoryFontSize = 25)
        {
            this.InputFieldSize = InputFieldSize;
            this.SuggestionFieldSize = SuggestionFieldSize;
            this.HistoryFontSize = HistoryFontSize;
        }
        public void GetSize(out float InputFieldSize, out float SuggestionFieldSize, out float HistoryFontSize)
        {
            InputFieldSize = this.InputFieldSize;
            SuggestionFieldSize = this.SuggestionFieldSize;
            HistoryFontSize = this.HistoryFontSize;
        }

        private void OnEnable()
        {
            GameConsole.OnConsoleVisibilityChange += OnConsoleChange;
        }
        private void OnDisable()
        {
            GameConsole.OnConsoleVisibilityChange -= OnConsoleChange;
        }
        private void Awake()
        {
            if(instance != null)
            {
                DestroyImmediate(instance.gameObject);
                Debug.LogWarning("More than once IGC_VisualManager instance found!");
            }
            instance = this;
            Init();
            canvas = GetComponentInChildren<Canvas>();
            suggestionBackground = SuggestionsField.GetComponentInParent<Image>();
        }
        private void OnValidate()
        {
            canvas = GetComponentInChildren<Canvas>();
            suggestionBackground = SuggestionsField.GetComponentInParent<Image>();
        }
        private void Update()
        {


            UpdateScale();
            if (!Application.isPlaying)
            {
                canvas.enabled = ShowPreview;
            }
            else
            {
                SuggestionsCount = InputParser.Instance.SuggestedCommands.Count;
                canvas.enabled = true;
                DisplayHistory();
                DisplaySuggestions();
                if (GameConsole.IsActive)
                {
                    EventSystem.current.SetSelectedGameObject(CommandInputField.gameObject, null);
                    CommandInputField.OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
        }
        private void OnConsoleChange(bool visibility)
        {
            if (visibility)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        public void UpdateScale()
        {
            float suggestionUp = (inputFieldYOffset * 0.5f + InputFieldSize + 15 + (SuggestionFieldSize * SuggestionsCount + 5) * 0.5f);
            var backgroundRect = Background.GetComponent<RectTransform>();
            backgroundRect.sizeDelta = new Vector2(Screen.width, Screen.height);
            backgroundRect.anchoredPosition = Vector2.zero;

            var inputFieldRect = CommandInputField.GetComponent<RectTransform>();
            inputFieldRect.sizeDelta = new Vector2(Screen.width, inputFieldYOffset + InputFieldSize);
            inputFieldRect.anchoredPosition = new Vector2(0, inputFieldYOffset + InputFieldSize) * 0.5f;
            CommandInputField.pointSize = InputFontSize + InputFieldSize;

            var historyRect = HistoryField.GetComponent<RectTransform>();
            historyRect.sizeDelta = new Vector2(Screen.width, Screen.height - (inputFieldYOffset * 0.5f + InputFieldSize + 5)) * HistoryTextPadding;
            historyRect.anchoredPosition = Vector2.zero + Vector2.up * (inputFieldYOffset * 0.5f + InputFieldSize * 0.5f + 5 + SuggestionFieldSize * SuggestionsCount);
            HistoryField.fontSize = HistoryFontSize;

            var suggRect = suggestionBackground.GetComponent<RectTransform>();
            suggRect.sizeDelta = new Vector2(Screen.width, SuggestionFieldSize * SuggestionsCount);
            suggRect.anchoredPosition = Vector2.zero + Vector2.up * suggestionUp;
            SuggestionsField.fontSize = (SuggestionFontSize / 50f) * SuggestionFieldSize;

            var sugFieldRect = SuggestionsField.GetComponent<RectTransform>();
            sugFieldRect.sizeDelta = new Vector2(Screen.width, 50) * SuggTextPadding;
            sugFieldRect.anchoredPosition = new Vector2(0, 25);

            if (!Application.isPlaying)
            {
                CommandInputField.text = "Test";
                HistoryField.text = "1: Test";
                SuggestionsField.text = "TestScript.TestMethod()";
            }
        }
        private void Init()
        {
            GameConsole.History = new Queue<GameConsole.CommandHistory>();
            HistoryField.text = "";
            HistoryField.alignment = TextAlignmentOptions.BottomLeft;
            CommandInputField.text = "";
            SuggestionsField.text = "";
        }
        private void DisplayHistory()
        {
            HistoryField.text = "";
            string builder = "";
            var arr = GameConsole.History.ToArray();
            int len = arr.Length;
            for (int i = 0; i < len; i++)
            {
                if (arr[i].MessageType == GameConsole.CommandHistory.HistoryType.Command)
                {
                    if (arr[i].Message == "") //if message is not empty, then output message rather than the command
                    {
                        builder += arr[i].CommandName;
                        builder += "(";
                        int len1 = arr[i].Argument != null ? arr[i].Argument.Length : 0;
                        for (int j = 0; j < len1; j++)
                        {
                            builder += arr[i].Argument[j].ToString();
                            if (j + 1 < len1)
                            {
                                builder += ", ";
                            }
                        }
                        builder += ")";
                    }
                    else
                    {
                        builder += arr[i].Message;
                    }
                }
                else
                {
                    builder += arr[i].Message;
                }
                builder += "\n";
            }
            HistoryField.text = builder;
        }
        private void DisplaySuggestions()
        {
            var parser = InputParser.Instance;
            int count = parser.SuggestedCommands.Count;
            string temp = "";
            for (int i = 0; i < count; i++)
            {
                temp += parser.SuggestedCommands[i];
                temp += "\n";
            }
            SuggestionsField.text = temp;
        }
    }
}