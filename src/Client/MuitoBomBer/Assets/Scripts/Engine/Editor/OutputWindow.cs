using UnityEngine;
using UnityEditor;

public class OutputWindow : EditorWindow
{
    private string output;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    static void Init()
    {
    }

    public static void Show(string result)
    {
        // Get existing open window or if none, make a new one:
        OutputWindow window = (OutputWindow)EditorWindow.GetWindow(typeof(OutputWindow));
        window.output = result;
        window.minSize = new Vector2(200, 200);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Output", EditorStyles.boldLabel);

        GUILayout.TextArea(output, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Copy"))
        {
            EditorGUIUtility.systemCopyBuffer = output;
            Debug.Log("Copied query to clipboard!");
        }


        GUILayout.EndHorizontal();
    }
}