using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

namespace Lim.InGameConsole.Parser
{
    [DefaultExecutionOrder(-3)]
    //Not the best looking code, BUT IT WORKS! MUAHAHAHA
    public class InputParser : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] private KeyCode EnterKeyCode = KeyCode.Return;
        [Tooltip("The controls for navigating the console history.")]
        [SerializeField] private KeyCode[] HistoryNavigator = new KeyCode[2] { KeyCode.UpArrow, KeyCode.DownArrow };
        [Tooltip("The controls for navigating command suggestions (auto filling).")]
        [SerializeField] private KeyCode SuggestionNavigator = KeyCode.Tab;
        [SerializeField] private TMP_InputField inputField;

        [Header("Debug")]
        [SerializeField] private string CurrentClass;
        [SerializeField] private string CurrentMethod;
        [SerializeField] private string CurrentArgument;
        public List<string> SuggestedCommands { get; private set; }
        public static InputParser Instance {get; private set;}

        int historyIndex = -1, suggestionIndex = -1;
        const float ogDelay = 2;
        float delay = ogDelay, inputTimer = 0;
        bool searchingSuggestions = false;
        string focusedSuggestionStr;

        private void Awake()
        {
            SuggestedCommands = new List<string>();
            searchingSuggestions = false;
            if (Instance != null)
            {
                DestroyImmediate(Instance);
                Debug.LogWarning("More than one InputParse found!");
            }
            Instance = this;
            inputField = GetComponentInChildren<TMP_InputField>();
        }
        private void Start()
        {
            inputField.enabled = true;
        }
        private void Update()
        {
            if (GameConsole.IsActive)
            {
                if (inputField.text == "")
                {
                    historyIndex = GameConsole.History.Count;
                    suggestionIndex = -1;
                }
                var historyInput = SetHistoryIndex(ref historyIndex);
                if (historyInput)
                {
                    suggestionIndex = -1;
                    string msg = GetHistory(historyIndex, out string str, out string mtstr, out object[] arg);
                    CurrentClass = str;
                    CurrentMethod = mtstr;
                    CurrentArgument = ParseArg(arg);

                    if (CurrentClass != "")
                    {
                        SetInputField(CurrentClass, CurrentMethod, CurrentArgument);
                    }
                    else
                    {
                        SetInputField(msg);
                    }
                }
                var suggInput = SetSuggestionIndex(ref suggestionIndex);
                if (suggInput)
                {
                    searchingSuggestions = true;
                    historyIndex = GameConsole.History.Count;
                    focusedSuggestionStr = GetSuggestion(suggestionIndex);
                    SetInputField(focusedSuggestionStr);
                }

                if (Input.GetKeyDown(EnterKeyCode))
                {
                    OnInputEnter();
                }
            }
            else
            {
                ClearInputField();
            }
        }

        private void OnInputChange(string t)
        {
            if (t != focusedSuggestionStr)
                searchingSuggestions = false;

            var split = t.Split(new[] { '.' }, 2);
            int lenMethodSplit = split.Length;
            if (lenMethodSplit > 1)
            {
                CurrentClass = split[0];
                string temp = split[1];
                var split1 = temp.Split(new char[1] { '(' });
                if (split1.Length > 1)
                {
                    CurrentMethod = split1[0];
                    if (split1[1].Length > 0)
                    {
                        CurrentArgument = split1[1];
                        if (CurrentArgument[CurrentArgument.Length - 1] != ')')
                        {
                            CurrentArgument = "";
                        }
                        else
                        {
                            if (CurrentArgument.Length - 1 == 0)
                            {
                                CurrentArgument = "void";
                            }
                            else
                            {
                                CurrentArgument = CurrentArgument.Remove(CurrentArgument.Length - 1);
                            }
                        }
                    }
                    else
                    {
                        CurrentArgument = "";
                    }
                }
                else
                {
                    CurrentMethod = split[1];
                    CurrentArgument = "";
                }
            }
            else
            {
                CurrentClass = t;
                CurrentMethod = "";
                CurrentArgument = "";
            }

            if (inputField.text != "")
            {
                if (!searchingSuggestions)
                    SuggestedCommands = GameConsole.GetFamiliarCommands(CurrentClass, CurrentMethod);
            }
            else
            {
                if (!searchingSuggestions)
                    SuggestedCommands = new List<string>();
            }
        }

        private bool SetHistoryIndex(ref int index)
        {
            bool changed = false;
            if (Input.GetKey(HistoryNavigator[0]))
            {
                if (inputTimer <= 0)
                {
                    inputTimer = Mathf.Clamp(delay, 0.4f, ogDelay);
                    changed = true;
                    historyIndex--;
                    historyIndex = Mathf.Clamp(historyIndex, 0, GameConsole.History.Count - 1);
                }
                inputTimer -= Time.deltaTime * 4;
                delay -= Time.deltaTime * 3;
            }
            else if (Input.GetKey(HistoryNavigator[1]))
            {
                if (inputTimer <= 0)
                {
                    inputTimer = Mathf.Clamp(delay, 0.4f, ogDelay);
                    changed = true;
                    historyIndex++;
                    historyIndex = Mathf.Clamp(historyIndex, 0, GameConsole.History.Count - 1);
                }
                inputTimer -= Time.deltaTime * 4;
                delay -= Time.deltaTime * 3;
            }
            else
            {
                inputTimer = 0;
                delay = ogDelay;
            }

            return changed;
        }
        private bool SetSuggestionIndex(ref int index)
        {
            if (Input.GetKeyDown(SuggestionNavigator))
            {
                int len = SuggestedCommands.Count-1;
                if(len < 0)
                {
                    return false;
                }
                suggestionIndex++;
                if(suggestionIndex > len)
                {
                    suggestionIndex = 0;
                }
                return true;
            }
            return false;
        }

        private string ParseArg(object[] arg)
        {
            string temp = "";
            if (arg != null)
            {
                for (int i = 0; i < arg.Length; i++)
                {
                    temp += arg[i].ToString();
                    if (i + 1 < arg.Length)
                    {
                        temp += ", ";
                    }
                }
            }
            return temp;
        }
        private object[] ParseArg(string arg)
        {
            List<object> Args = new List<object>();
            var split = arg.Split(',');
            Args.AddRange(split);
            return Args.ToArray();
        }

        private string GetHistory(int index, out string classStr, out string methodStr, out object[] args)
        {
            var arr = GameConsole.History.ToArray();
            string temp;
            if (arr.Length > 0)
            {
                if (arr[index].MessageType == GameConsole.CommandHistory.HistoryType.Command)
                {
                    if (arr[index].Message == "")
                    {
                        temp = arr[index].CommandName;
                        var split = temp.Split('.');
                        classStr = split[0];
                        methodStr = split[1];
                        args = arr[index].Argument;
                        return classStr + "." + methodStr + "(" + args?.ToString() + ")";
                    }
                    else
                    {
                        classStr = "";
                        methodStr = "";
                        args = null;
                        return arr[index].CommandName;
                    }
                }
                else
                {
                    classStr = "";
                    methodStr = "";
                    args = null;
                    return temp = arr[index].Message;
                }
            }
            else
            {
                classStr = "";
                methodStr = "";
                args = null;
                return temp = "";
            }
        }
        private string GetSuggestion(int index)
        {
            if (SuggestedCommands.Count > 0)
            {
                return SuggestedCommands[index];
            }
            else
            {
                return "";
            }
        }

        private void SetInputField(string classStr, string methodStr, string args)
        {
            inputField.text = (classStr + "." + methodStr + "(" + args + ")");
            inputField.caretPosition = inputField.text.Length;
            inputField.ForceLabelUpdate();
        }
        private void SetInputField(string message)
        {
            inputField.text = message;
            inputField.caretPosition = inputField.text.Length;
            inputField.ForceLabelUpdate();
        }
        private void ClearInputField()
        {
            inputField.text = "";
        }
        private void OnInputEnter()
        {
            searchingSuggestions = false;
            historyIndex = GameConsole.History.Count;
            OnInputChange(inputField.text);

            if (CurrentArgument == "")
            {
                GameConsole.CallCommand(CurrentClass, CurrentMethod, null);
            }
            else
            {
                GameConsole.CallCommand(CurrentClass, CurrentMethod, CurrentArgument != "void" ? ParseArg(CurrentArgument) : null);
            }
            ClearInputField();
        }
        private void OnApplicationQuit()
        {
            inputField.enabled = false;
        }
    }
}