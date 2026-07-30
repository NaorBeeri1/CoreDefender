using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

public class CoreDefenderDebugExporter : EditorWindow
{
    [MenuItem("Tools/CoreDefender/Export Deep Diagnostic Report")]
    public static void ShowWindow()
    {
        GetWindow<CoreDefenderDebugExporter>("Debug Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("CoreDefender Diagnostic Exporter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Extract & Copy All Diagnostics to Clipboard", GUILayout.Height(40)))
        {
            ExportDiagnostics();
        }
    }

    private static void ExportDiagnostics()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== CORE DEFENDER DEEP DIAGNOSTIC REPORT ===");
        sb.AppendLine($"Timestamp: {System.DateTime.Now}");
        sb.AppendLine();

        // 1. Scene Turrets & Stats
        sb.AppendLine("## 1. SCENE TURRETS & STATS");
        GameObject[] turretObjs = GameObject.FindGameObjectsWithTag("Turret");
        sb.AppendLine($"Total Turrets Found: {turretObjs.Length}");
        foreach (GameObject tObj in turretObjs)
        {
            if (tObj == null) continue;
            TurretController tc = tObj.GetComponent<TurretController>();
            if (tc != null)
            {
                TurretData data = tc.GetTurretData();
                int dmg = data != null ? data.damage : -1;
                sb.AppendLine($"- Turret Name: {tObj.name} | Position: {tObj.transform.position} | Damage: {dmg} | HP: {tc.GetCurrentHealth()}/{tc.GetMaxHealth()}");
            }
            else
            {
                sb.AppendLine($"- Turret Name: {tObj.name} (Missing TurretController component!)");
            }
        }

        sb.AppendLine();

        // 2. Scene Enemies & Stats
        sb.AppendLine("## 2. SCENE ENEMIES & STATS");
        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        sb.AppendLine($"Total Enemies Found: {enemyObjs.Length}");
        foreach (GameObject eObj in enemyObjs)
        {
            if (eObj == null) continue;
            
            EnemyContext ctx = eObj.GetComponent<EnemyContext>();
            EnemyController oldCtrl = eObj.GetComponent<EnemyController>();
            LaserDroneController drone = eObj.GetComponent<LaserDroneController>();

            string enemyType = "Unknown";
            string details = "";

            if (ctx != null) { enemyType = "EnemyContext"; }
            else if (oldCtrl != null) { enemyType = "EnemyController"; }
            else if (drone != null) { enemyType = "LaserDroneController"; }

            sb.AppendLine($"- Enemy Name: {eObj.name} | Type: {enemyType} | Position: {eObj.transform.position}");
        }

        sb.AppendLine();

        // 3. TargetingManager State Inspection via Reflection / Public API
        sb.AppendLine("## 3. TARGETING MANAGER INSPECTION");
        TargetingManager tm = Object.FindAnyObjectByType<TargetingManager>();
        if (tm != null)
        {
            sb.AppendLine("- TargetingManager instance found in scene.");
            SerializedObject serializedTM = new SerializedObject(tm);
            SerializedProperty prop = serializedTM.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.name == "m_Script") continue;
                sb.AppendLine($"  - {prop.displayName}: {GetPropertyValue(prop)}");
            }
        }
        else
        {
            sb.AppendLine("- [ERROR] TargetingManager instance NOT found in the active scene!");
        }

        // Copy to clipboard
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("[CoreDefender] Deep diagnostics successfully exported and copied to clipboard!");
        EditorUtility.DisplayDialog("Success", "Diagnostic report copied to clipboard! Paste it here for analysis.", "OK");
    }

    private static string GetPropertyValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: return prop.intValue.ToString();
            case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
            case SerializedPropertyType.Float: return prop.floatValue.ToString();
            case SerializedPropertyType.String: return string.IsNullOrEmpty(prop.stringValue) ? "(empty)" : prop.stringValue;
            case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "None";
            default: return prop.propertyType.ToString();
        }
    }
}