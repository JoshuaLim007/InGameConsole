#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Lim.InGameConsole.Visual;
using Lim.InGameConsole;
using System.Linq;
using UnityEditor.Compilation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

namespace Lim.GameEditor
{
    public class InGameConsoleEditor : EditorWindow
    {
        private static bool preview = false;
        private static float inputFieldSize = 35;
        private static float suggFieldSize = 35;
        private static float historyFieldSize = 35;
        private static KeyCode triggerCode = KeyCode.Tab;
        private static List<AssemblyEnabled> assemblies = new List<AssemblyEnabled>();
        private struct AssemblyEnabled{
            public string AssemblyName;
            public bool Enabled;
        }

        private static UnityEngine.Object consolePrefab;
        private static GameObject inSceneConsole;
        private const string consolePath = "Packages/com.limjoshua.igconsole/Console/InGameConsole.prefab";
        private Scene previousScene, currentScene;
        private int delayer;
        static Vector2 scrollPos;

        [InitializeOnLoadMethod]
        static void OnReload()
        {
            Flush();
            if (!Application.isPlaying)
            {
                consolePrefab = AssetDatabase.LoadAssetAtPath(consolePath, typeof(GameObject));
                if(consolePath != null)
                    GetSettings(consolePrefab);
            }
        }

        [MenuItem("InGameConsole/InGameConsole Manager")]
        static void Init()
        {
            Flush();
            InGameConsoleEditor window =
                (InGameConsoleEditor)EditorWindow.GetWindow(typeof(InGameConsoleEditor));

            consolePrefab = AssetDatabase.LoadAssetAtPath(consolePath, typeof(GameObject));
            if(consolePath != null)
                GetSettings(consolePrefab);
        }

        [RuntimeInitializeOnLoadMethod]
        static void OnStart()
        {
            if (Application.isPlaying)
            {
                Flush();
                consolePrefab = AssetDatabase.LoadAssetAtPath(consolePath, typeof(GameObject));
                Instantiate(consolePrefab);
            }
        }

        void OnGUI()
        {
            SceneChange();
            if (!Application.isPlaying)
            {
                if (consolePrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.TextField("Console: ", consolePrefab.name);
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Settings: ", EditorStyles.boldLabel);
                    if (GUILayout.Button("Toggle Preview"))
                    {
                        preview = !preview;
                    }
                    if (preview)
                    {
                        if (inSceneConsole == null)
                            inSceneConsole = Instantiate(consolePrefab) as GameObject;

                        GUI.enabled = true;
                        SetSettings();
                    }
                    else
                    {
                        Flush();
                        inSceneConsole = null;
                        GUI.enabled = false;
                        SetSettings();
                        GUI.enabled = true;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.TextField("Missing Console Prefab!");
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Cannot be used during play mode!");
            }

        }

        static void GetSettings(UnityEngine.Object consolePrefab)
        {
            var prefab = consolePrefab as GameObject;
            if (prefab != null)
            {
                var script = prefab.GetComponent<IGC_VisualManager>();
                if (script)
                {
                    script.GetSize(out float x, out float y, out float z);
                    inputFieldSize = x;
                    suggFieldSize = y;
                    historyFieldSize = z;
                    triggerCode = prefab.GetComponent<IGC_Main>().GetKeyTrigger();
                }
            }
        }
        static void SetSettings()
        {
            var prefab = consolePrefab as GameObject;
            inputFieldSize = EditorGUILayout.FloatField("Input Field Size: ", inputFieldSize);
            suggFieldSize = EditorGUILayout.FloatField("Suggestion Field Size: ", suggFieldSize);
            historyFieldSize = EditorGUILayout.FloatField("History Field Size: ", historyFieldSize);
            EditorGUILayout.LabelField("Controls: ", EditorStyles.boldLabel);
            triggerCode = (KeyCode)EditorGUILayout.EnumPopup("Console trigger key: ", triggerCode);
            EditorGUILayout.LabelField("Assemblies: ", EditorStyles.boldLabel);
            GetAssemblies(prefab);
            var enabledAssemblies = new List<string>();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(512), GUILayout.Height(256));
            for (int i = 0; i < assemblies.Count; i++)
            {
                EditorGUILayout.LabelField(assemblies[i].AssemblyName);
                var changeState = EditorGUILayout.Toggle("", assemblies[i].Enabled);
                assemblies[i] = new AssemblyEnabled { AssemblyName = assemblies[i].AssemblyName, Enabled = changeState };
                if (changeState)
                {
                    enabledAssemblies.Add(assemblies[i].AssemblyName);
                }
            }
            EditorGUILayout.EndScrollView();

            if (preview)
            {
                var igm = prefab.GetComponent<IGC_Main>();
                igm.SetAssemblies(enabledAssemblies);
                igm.SetKeyTrigger(triggerCode);
                prefab.GetComponent<IGC_VisualManager>().SetSize(inputFieldSize, suggFieldSize, historyFieldSize);
                var vss = inSceneConsole.GetComponent<IGC_VisualManager>();
                vss.SetSize(inputFieldSize, suggFieldSize, historyFieldSize);
                vss.UpdateScale();

            }

            if(GUILayout.Button("Save"))
            {
                prefab = PrefabUtility.SavePrefabAsset(prefab);
            }

        }

        private void OnDisable()
        {
            Flush();
        }
        private void OnDestroy()
        {
            Flush();      
        }
        static void GetAssemblies(GameObject prefab)
        {
            assemblies = new List<AssemblyEnabled>();
            var allAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player).Select(a=>a.name).ToList();
            var inMain = prefab.GetComponent<IGC_Main>().GetAssemblies();
            for (int i = 0; i < allAssemblies.Count; i++)
            {
                bool state = inMain.Contains(allAssemblies[i]);
                assemblies.Add(new AssemblyEnabled { 
                    AssemblyName = allAssemblies[i], 
                    Enabled = state
                });
            }
        }
        static void Flush()
        {
            var all = FindObjectsOfType<IGC_VisualManager>();
            for (int i = 0; i < all.Length; i++)
            {
                DestroyImmediate(all[i].gameObject);
            }
        }
        void SceneChange()
        {
            if (delayer == 0)
            {
                previousScene = SceneManager.GetActiveScene();
                delayer++;
            }
            else
            {
                currentScene = SceneManager.GetActiveScene();
                delayer = 0;
            }
            if (previousScene != currentScene)
            {
                preview = false;
            }
        }
        public static void CreateConsoleOnBuild(Scene scene)
        {
            if(consolePrefab == null)
                consolePrefab = AssetDatabase.LoadAssetAtPath(consolePath, typeof(GameObject));

            Flush();
            if (consolePrefab != null)
            {
                var go = Instantiate(consolePrefab) as GameObject;
                SceneManager.MoveGameObjectToScene(go, scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }

    public class ConsoleBeforeBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1;

        public void OnPreprocessBuild(BuildReport report)
        {
            string initialScene = SceneManager.GetActiveScene().path;

            Scene sene = SceneManager.GetActiveScene();

            int scenesInBuild = EditorBuildSettings.scenes.Count();

            for (int i = 0; i < scenesInBuild; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                var toload = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                InGameConsoleEditor.CreateConsoleOnBuild(toload);
            }

            EditorSceneManager.OpenScene(initialScene, OpenSceneMode.Single);
        }
    }
}
#endif