using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class BakeAllScenesWindow : EditorWindow
{
    private string scenesLocation = "";
    private SceneAsset[] scenes = new SceneAsset[0];
    private int numberOfScenes;

    [MenuItem("Tools/Bake multiple scenes")]
    static void ShowWindow()
    {
        BakeAllScenesWindow window = (BakeAllScenesWindow)EditorWindow.GetWindow(typeof(BakeAllScenesWindow));
        window.titleContent.text = "Multi-scene baking";
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label ("Select scenes manually", EditorStyles.boldLabel);

        numberOfScenes = EditorGUILayout.IntField("Number of scenes", numberOfScenes);
        if (numberOfScenes > scenes.Length)
        {
            // Number of scenes has increased, copy the current ones and leave space for new ones
            SceneAsset[] copy = (SceneAsset[])scenes.Clone();
            scenes = new SceneAsset[numberOfScenes];
            for (int i = 0; i < copy.Length; i++)
            {
                scenes[i] = copy[i];
            }
        }
        else if (numberOfScenes < scenes.Length)
        {
            // Number of scenes has decreased, remove the last ones
            SceneAsset[] copy = (SceneAsset[])scenes.Clone();
            scenes = new SceneAsset[numberOfScenes];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = copy[i];
            }
        }
        // if numberOfScenes equals scenes.Lenght, nothing has changed
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = (SceneAsset)EditorGUILayout.ObjectField(scenes[i], typeof(SceneAsset), false);
        }

        GUILayout.Space(15);
        GUILayout.Label ("Include all scenes in this folder", EditorStyles.boldLabel);
        scenesLocation = EditorGUILayout.TextField ("Scenes folder", scenesLocation);
        GUILayout.Label ("(for example: Assets/Scenes/", EditorStyles.miniLabel);

        GUILayout.Space(20);
        if (GUILayout.Button("Start baking"))
        {
            bool startBake = true;
            List<string> pathsToBake = new List<string>();
            if (!string.IsNullOrEmpty(scenesLocation))
            {
                try
                {
                    if (!scenesLocation.Contains("Assets"))
                    {
                        scenesLocation = (scenesLocation[0] == '/' ? "Assets" : "Assets/") + scenesLocation;
                    }
                    string[] files = Directory.GetFiles(scenesLocation);
                    foreach (string s in files)
                    {
                        if (s.Contains(".unity") && !s.Contains(".meta"))
                        {
                            pathsToBake.Add(s);
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Debug.LogError(string.Format("Cannot find directory: {0}", scenesLocation));
                    startBake = false;
                }
            }
            try
            {
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i] != null)
                    {
                        string path = AssetDatabase.GetAssetPath(scenes[i]);
                        if (pathsToBake.Contains(path))
                        {
                            Debug.Log(string.Format("{0} is already added", scenes[i]));
                        }
                        else
                        {
                            pathsToBake.Add(path);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("Unexpected error: {0}", ex.Message));
                startBake = false;
            }
            if (startBake)
            {
                BakeScenes(pathsToBake.ToArray());
            }
        }
    }

    private void BakeScenes(string[] toBake)
    {
        foreach (string scenePath in toBake)
        {
            Debug.Log(string.Format("Starting: {0}", scenePath));
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (Lightmapping.Bake())
            {
                Debug.Log(string.Format("Bake success: {0}", scenePath));
            }
            else
            {
                Debug.LogError(string.Format("Error baking: {0}", scenePath));
            }
            EditorSceneManager.SaveOpenScenes();
        }
    }
}