using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace Lim.InGameConsole
{
    public sealed class GameConsole
    {
        const int HistorySize = 35;
        public struct CommandHistory
        {
            public enum HistoryType
            {
                Command,
                Message,
            }
            public HistoryType MessageType { get; set; }

            public string Message { get; set; }
            public string CommandName { get; set; }
            public MethodInfo Method { get; set; }
            public object[] Argument { get; set; }
        }
        public delegate void OnTrigger(bool visibility);




        public static OnTrigger OnConsoleVisibilityChange;
        public static bool IsActive { get; private set; }
        public static void SetActive(bool state)
        {
            if (IsActive != state)
            {
                OnConsoleVisibilityChange.Invoke(state);
            }
            IsActive = state;
        }
        public static Queue<CommandHistory> History { get; set; }
        public static Dictionary<string, Dictionary<string, MethodInfo>> Commands { get => IGC_Main.Instance?.Commands; }
        public static void CallCommand(string className, string methodName, object[] arg)
        {
            className = className.ToLower();
            methodName = methodName.ToLower();
            Debug.Log(className + " " + methodName);
            if (Commands.TryGetValue(className, out Dictionary<string, MethodInfo> dict))
            {
                if (dict.TryGetValue(methodName, out MethodInfo methodInfo))
                {
                    if (arg == null)
                    {
                        var tempArg = methodInfo.GetParameters();
                        List<object> args = new List<object>();
                        for (int i = 0; i < tempArg.Length; i++)
                        {
                            args.Add(tempArg[i].DefaultValue);
                        }
                        var arArray = args.ToArray();
                        methodInfo.Invoke(null, arArray);
                        AddToHistory(className + "." + methodName, methodInfo, arArray);
                    }
                    else
                    {
                        var par = methodInfo.GetParameters();
                        if(arg.Length > par.Length)
                        {
                            AddToHistoryWithMessage(className + "." + methodName + "()", methodInfo, arg, ("Parameter overflow! Expects " + par.Length + " parameter(s)!"));
                            return;
                        }
                        for (int i = 0; i < arg.Length; i++)
                        {
                            try
                            {
                                arg[i] = Convert.ChangeType(arg[i], par[i].ParameterType);
                            }
                            catch (Exception)
                            {
                                AddToHistoryWithMessage(className + "." + methodName + "()", methodInfo, arg, "Invalid parameter value!");
                                return;
                            }
                        }
                        methodInfo.Invoke(null, arg);
                        AddToHistory(className + "." + methodName, methodInfo, arg);
                    }
                }
                else
                {
                    AddToHistoryWithMessage(className + "." + methodName + "()", null, null, "No such method exists!");
                    //PrintToConsole("No such command exists!");
                }
            }
            else
            {
                string temp = methodName != "" ? className + "." + methodName : className;
                AddToHistoryWithMessage(temp, null, null, "No such class exists!");
                //PrintToConsole("No such command exists!");
            }
        }
        public static void Print(object message)
        {
            if (History.Count > HistorySize)
            {
                History.Dequeue();
            }
            History.Enqueue(new CommandHistory()
            {
                Message = message.ToString(),
                MessageType = CommandHistory.HistoryType.Message
            });
        }
        public static List<string> GetFamiliarCommands(string className, string methodName)
        {
            string[] cn;

            if (Commands != null)
            {
                className = className.ToLower();
                methodName = methodName.ToLower();
                if (methodName == "")
                {
                    cn = Commands.Keys.Where(a => a.StartsWith(className)).ToArray();
                }
                else
                {
                    cn = Commands.Keys.Where(a => a == className).ToArray();
                }
                Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
                if (cn.Length > 0)
                {
                    for (int i = 0; i < cn.Length; i++)
                    {
                        temp.Add(cn[i], new List<string>());
                        if (methodName != "")
                        {
                            temp[cn[i]].AddRange(Commands[cn[i]].Keys.Where(a => a.StartsWith(methodName)));
                        }
                        else
                        {
                            temp[cn[i]].AddRange(Commands[cn[i]].Keys);
                        }
                    }
                }

                List<string> allMethods = new List<string>();
                for (int i = 0; i < cn.Length; i++)
                {
                    for (int j = 0; j < temp[cn[i]].Count; j++)
                    {
                        var p = Commands[cn[i]][temp[cn[i]][j]].GetParameters();
                        string parStr = "";
                        for (int x = 0; x < p.Length; x++)
                        {
                            parStr += p[x].ParameterType.Name + " " + p[x].Name;
                            if(x + 1 < p.Length)
                            {
                                parStr += ", ";
                            }
                        }
                        allMethods.Add(cn[i] + "." + temp[cn[i]][j] + "(" + parStr + ")");
                    }
                }
                return allMethods;
            }
            return new List<string>();
        }



        private static void AddToHistoryWithMessage(string fullName, MethodInfo methodInfo, object[] arg, string msg)
        {
            if (History.Count > 15)
            {
                History.Dequeue();
            }
            History.Enqueue(new CommandHistory()
            {
                Message = msg,
                CommandName = fullName,
                Method = methodInfo,
                Argument = arg,
                MessageType = CommandHistory.HistoryType.Command
            });
        }
        private static void AddToHistory(string fullName, MethodInfo methodInfo, object[] arg)
        {
            if (History.Count > 15)
            {
                History.Dequeue();
            }
            History.Enqueue(new CommandHistory()
            {
                Message = "",
                CommandName = fullName,
                Method = methodInfo,
                Argument = arg,
                MessageType = CommandHistory.HistoryType.Command
            });
        }
    }
}
