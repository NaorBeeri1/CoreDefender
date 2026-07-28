using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEngine.SceneManagement;

public class DeepDiagnosticExporter : EditorWindow
{
    [MenuItem("Tools/CoreDefender/Export Deep Diagnostics to Clipboard")]
    public static void ShowWindow()
    {
        GetWindow<DeepDiagnosticExporter>("Diagnostic Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("CoreDefender Deep Diagnostic Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Run Full Inspection & Copy to Clipboard", GUILayout.Height(40)))
        {
            ExportDiagnostics();
        }
    }

    private static void ExportDiagnostics()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== CORE DEFENDER DEEP DIAGNOSTIC REPORT ===");
        sb.AppendLine($"Timestamp: {System.DateTime.Now}");
        sb.AppendLine($"Active Scene: {SceneManager.GetActiveScene().name}");
        sb.AppendLine();

        // 1. Inspect All Scene Root Objects & Components
        sb.AppendLine("## 1. SCENE HIERARCHY & COMPONENT INSPECTION");
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject rootObj in rootObjects)
        {
            InspectGameObjectRecursive(rootObj, sb, 0);
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 2. Inspect ScriptableObject Assets in Project
        sb.AppendLine("## 2. SCRIPTABLEOBJECT ASSETS INSPECTION");
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so != null)
            {
                sb.AppendLine($"- **Asset**: {so.name} ({path})");
                SerializedObject serializedSO = new SerializedObject(so);
                SerializedProperty prop = serializedSO.GetIterator();
                bool enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (prop.name == "m_Script") continue;
                    sb.AppendLine($"  - {prop.displayName}: {GetPropertyValue(prop)}");
                }
            }
        }

        // Copy to clipboard
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("[CoreDefender] Deep diagnostics successfully exported and copied to clipboard!");
        EditorUtility.DisplayDialog("Success", "Deep diagnostic report copied to clipboard! Paste it here for analysis.", "OK");
    }

    private static void InspectGameObjectRecursive(GameObject obj, StringBuilder sb, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        sb.AppendLine($"{indentStr}- **GameObject**: {obj.name} (Active: {obj.activeSelf})");

        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            sb.AppendLine($"{indentStr}  - **Component**: {comp.GetType().Name}");

            try
            {
                SerializedObject serializedObj = new SerializedObject(comp);
                SerializedProperty prop = serializedObj.GetIterator();
                bool enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (prop.name == "m_Script") continue;

                    string valStr = GetPropertyValue(prop);
                    sb.AppendLine($"{indentStr}    - {prop.displayName}: {valStr}");
                }
            }
            catch
            {
                // Ignore serialization exceptions on complex components
            }
        }

        foreach (Transform child in obj.transform)
        {
            InspectGameObjectRecursive(child.gameObject, sb, indent + 1);
        }
    }

    private static string GetPropertyValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                return prop.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return prop.boolValue.ToString();
            case SerializedPropertyType.Float:
                return prop.floatValue.ToString();
            case SerializedPropertyType.String:
                return string.IsNullOrEmpty(prop.stringValue) ? "(empty)" : prop.stringValue;
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue != null ? $"[FILLED] {prop.objectReferenceValue.name} ({prop.objectReferenceValue.GetType().Name})" : "[EMPTY / None]";
            case SerializedPropertyType.Enum:
                return prop.enumNames != null && prop.enumValueIndex < prop.enumNames.Length && prop.enumValueIndex >= 0
                    ? prop.enumNames[prop.enumValueIndex]
                    : prop.enumValueIndex.ToString();
            default:
                return prop.propertyType.ToString();
        }
    }
}