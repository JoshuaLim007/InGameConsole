using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System;
using System.Linq;

namespace Lim.InGameConsole
{
    [DefaultExecutionOrder(-5)]
    public class IGC_Main : MonoBehaviour
    {
        const string Version = "1.0";
        public static IGC_Main Instance { get; private set; }
        [Header("Controls")]
        [Tooltip("Key to toggle console")]
        [SerializeField] private KeyCode Trigger = KeyCode.F3;

        [Header("Refrences")]
        [SerializeField] private GameObject InputParser = null;
        [Tooltip("Add the assembly definitions your commands exist in.\nAssembly-CSharp added automatically")]
        [SerializeField] private List<string> AssemblyNames = new List<string>();
        [SerializeField] private List<Assembly> assembliesToCheck = new List<Assembly>();
        public Dictionary<string, Dictionary<string, MethodInfo>> Commands { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(Instance.gameObject);
                Debug.LogWarning("More than once IGC_Main instance found!");
            }
            Instance = this;

            Debug.Log("" +
                "Using InGameConsole" +
                "\nVersion: " + Version +
                "\nCreated by - Joshua Lim");
            Commands = new Dictionary<string, Dictionary<string, MethodInfo>>();
            assembliesToCheck.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => AssemblyNames.Contains(a.GetName().Name)).ToArray());
            foreach (var item in assembliesToCheck)
            {
                GetMethods(item);
            }
        }
        private void Start()
        {
            GameConsole.Print("InGameConsole activated");
        }
        private void Update()
        {
            if (Input.GetKeyDown(Trigger))
            {
                if (GameConsole.IsActive) GameConsole.SetActive(false);
                else GameConsole.SetActive(true);
            }
            SetVisibility();
        }

        public void SetAssemblies(List<string> assemblies)
        {
            AssemblyNames = assemblies;
        }
        public List<string> GetAssemblies()
        {
            return AssemblyNames;
        }
        public void SetKeyTrigger(KeyCode keyCode)
        {
            Trigger = keyCode;
        }
        public KeyCode GetKeyTrigger()
        {
            return Trigger;
        }

        private void SetVisibility()
        {
            InputParser.SetActive(GameConsole.IsActive);
        }
        private void GetMethods(Assembly assembly)
        {
            var methods = assembly
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(y => y.GetCustomAttributes().OfType<ConsoleCommand>().Any())
                .ToArray();

            foreach (var item in methods)
            {
                var className = item.DeclaringType.Name.ToLower();
                if (!Commands.ContainsKey(className))
                {
                    Commands.Add(className, new Dictionary<string, MethodInfo>());
                    Commands[className].Add(item.Name.ToLower(), item);
                }
                else
                {
                    Commands[className].Add(item.Name.ToLower(), item);
                }
            }
        }
        private void OnApplicationQuit()
        {
            Destroy(gameObject);
        }
    }

}