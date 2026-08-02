using UnityEngine;
using UnityEditor;
using System.IO;

public class CoreDefenderBalanceTool : MonoBehaviour
{
    [MenuItem("Tools/CoreDefender/Apply Balanced Roguelike Curve")]
    public static void ApplyRoguelikeBalance()
    {
        Debug.Log("[CoreDefender] Applying roguelike balance patch...");

        // 1. Update WaveManager default values if found in scene
        WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
        if (waveManager != null)
        {
            SerializedObject serializedWaveMgr = new SerializedObject(waveManager);
            SerializedProperty timeProp = serializedWaveMgr.FindProperty("timeBetweenWaves");
            if (timeProp != null)
            {
                timeProp.floatValue = 8f; // Balanced prep window
                serializedWaveMgr.ApplyModifiedProperties();
                EditorUtility.SetDirty(waveManager);
            }
        }

        // 2. Adjust all ScriptableObject Data Assets (WaveData, Turrets, Mass Driver, Ion Beacon)
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        int updatedAssetsCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null) continue;

            SerializedObject serializedSO = new SerializedObject(so);

            // Balance Wave Data health scaling
            if (path.Contains("Wave") || so.name.Contains("Wave"))
            {
                SerializedProperty healthBonusProp = serializedSO.FindProperty("healthBonusPerEnemy");
                if (healthBonusProp != null)
                {
                    healthBonusProp.intValue = 18; // Fair scaling curve
                    serializedSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(so);
                    updatedAssetsCount++;
                }
            }

            // Balance End-Game Turrets (Mass Driver & Ion Beacon thermal pressure)
            if (so.name.Contains("MassDriver") || so.name.Contains("Mass Driver"))
            {
                SetPropertySafely(serializedSO, "heatPerShot", 50f);
                SetPropertySafely(serializedSO, "coolingRate", 20f);
                SetPropertySafely(serializedSO, "cost", 350);
                serializedSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(so);
                updatedAssetsCount++;
            }
            else if (so.name.Contains("IonBeacon") || so.name.Contains("Ion Beacon"))
            {
                SetPropertySafely(serializedSO, "heatPerShot", 100f);
                SetPropertySafely(serializedSO, "coolingRate", 15f);
                SetPropertySafely(serializedSO, "cost", 500);
                serializedSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(so);
                updatedAssetsCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CoreDefender] Balance adjustment complete! Updated {updatedAssetsCount} data assets and scene managers.");
        EditorUtility.DisplayDialog("CoreDefender Balance Tool", "Roguelike balance adjustments applied successfully!\n\n- Wave preparation time increased to 8s\n- Enemy health scaling curves balanced\n- End-game thermal cooldown weights calibrated", "OK");
    }

    private static void SetPropertySafely(SerializedObject serializedObj, string propName, float value)
    {
        SerializedProperty prop = serializedObj.FindProperty(propName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Float)
        {
            prop.floatValue = value;
        }
    }

    private static void SetPropertySafely(SerializedObject serializedObj, string propName, int value)
    {
        SerializedProperty prop = serializedObj.FindProperty(propName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Integer)
        {
            prop.intValue = value;
        }
    }
}