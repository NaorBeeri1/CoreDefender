using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.UI;
using TMPro;

public class ProjectExporterWindow : EditorWindow
{
    [MenuItem("Tools/Export Project State to Clipboard")]
    public static void ShowWindow()
    {
        GetWindow<ProjectExporterWindow>("Project Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Project State Exporter (Deep Inspection + Diagnostics)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (GUILayout.Button("Export Everything & Copy to Clipboard", GUILayout.Height(40)))
        {
            ExportProject();
        }
    }

    private static void ExportProject()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# Unity Project State Export (Enhanced Diagnostic Build)");
        sb.AppendLine($"Project Path: {Application.dataPath}");
        sb.AppendLine();

        // 1. Assets Folder Structure & Code Files
        sb.AppendLine("## 1. Assets Folder Structure & Code Files");
        string assetsPath = Application.dataPath;
        AppendDirectoryRecursive(assetsPath, sb, 0);

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 2. Scene Hierarchy & Inspector Properties
        sb.AppendLine("## 2. Current Scene Hierarchy & Inspector Data");
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject rootObj in rootObjects)
        {
            DumpGameObject(rootObj, sb, 0);
        }

        // 3. Automated Diagnostic Audit
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine("## 3. Automated Diagnostic Audit");
        RunDiagnostics(sb);

        // Copy to clipboard
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("Enhanced deep project state & diagnostics successfully exported and copied to clipboard!");
        EditorUtility.DisplayDialog("Success", "Enhanced project state & diagnostics copied to clipboard!", "OK");
    }

    private static void AppendDirectoryRecursive(string dirPath, StringBuilder sb, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        sb.AppendLine($"{indentStr}- [DIR] {dirInfo.Name}");

        foreach (FileInfo file in dirInfo.GetFiles())
        {
            if (file.Extension == ".meta") continue;

            string fileIndent = new string(' ', (indent + 1) * 2);
            sb.AppendLine($"{fileIndent}- [FILE] {file.Name}");

            if (file.Extension == ".cs")
            {
                sb.AppendLine($"{fileIndent}  ```csharp");
                string[] lines = File.ReadAllLines(file.FullName);
                foreach (string line in lines)
                {
                    sb.AppendLine($"{fileIndent}  {line}");
                }
                sb.AppendLine($"{fileIndent}  ```");
            }
        }

        foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
        {
            AppendDirectoryRecursive(subDir.FullName, sb, indent + 1);
        }
    }

    private static void DumpGameObject(GameObject obj, StringBuilder sb, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        sb.AppendLine($"{indentStr}- **GameObject**: {obj.name} (Active: {obj.activeSelf})");
        sb.AppendLine($"{indentStr}  - Position: {obj.transform.position}");
        sb.AppendLine($"{indentStr}  - Rotation: {obj.transform.eulerAngles}");
        sb.AppendLine($"{indentStr}  - Scale: {obj.transform.localScale}");

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

                    string valStr = GetDetailedPropertyValue(prop);
                    sb.AppendLine($"{indentStr}    - {prop.displayName}: {valStr}");
                }
            }
            catch
            {
                // Ignored if layout serialization fails
            }
        }

        foreach (Transform child in obj.transform)
        {
            DumpGameObject(child.gameObject, sb, indent + 1);
        }
    }

    private static string GetDetailedPropertyValue(SerializedProperty prop)
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
                return string.IsNullOrEmpty(prop.stringValue) ? "(empty string)" : prop.stringValue;
            case SerializedPropertyType.Color: 
                return prop.colorValue.ToString();
            case SerializedPropertyType.Vector2: 
                return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3: 
                return prop.vector3Value.ToString();
            case SerializedPropertyType.Vector4: 
                return prop.vector4Value.ToString();
            case SerializedPropertyType.Rect: 
                return prop.rectValue.ToString();
            case SerializedPropertyType.Enum:
                return prop.enumNames != null && prop.enumValueIndex < prop.enumNames.Length && prop.enumValueIndex >= 0 
                    ? prop.enumNames[prop.enumValueIndex] 
                    : prop.enumValueIndex.ToString();
            case SerializedPropertyType.ObjectReference: 
                return prop.objectReferenceValue != null ? $"[FILLED] {prop.objectReferenceValue.name}" : "[EMPTY / None]";
            case SerializedPropertyType.ArraySize:
                return $"Array Size: {prop.intValue}";
            default: 
                return prop.propertyType.ToString();
        }
    }

    private static void RunDiagnostics(StringBuilder sb)
    {
        // Check for GameCanvas active state
        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (canvasObj != null)
        {
            if (!canvasObj.activeSelf)
            {
                sb.AppendLine("- [WARNING] 'GameCanvas' GameObject is currently DISABLED (`activeSelf = false`). UI manager scripts on disabled GameObjects cannot execute initialization or enable child panels upon game over!");
            }
            else
            {
                sb.AppendLine("- [OK] 'GameCanvas' is active.");
            }
        }
        else
        {
            sb.AppendLine("- [ERROR] 'GameCanvas' GameObject could not be found in the active scene!");
        }

        // Check for GameManager / PlayerStats
        if (Object.FindObjectOfType<PlayerStats>() == null)
        {
            sb.AppendLine("- [ERROR] No 'PlayerStats' component found in the scene. Economy features will fail.");
        }
        else
        {
            sb.AppendLine("- [OK] 'PlayerStats' singleton instance detected.");
        }
    }
}