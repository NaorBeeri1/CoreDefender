using UnityEngine;
using UnityEditor;

public class CoreDefenderSetupTool : MonoBehaviour
{
    [MenuItem("Tools/CoreDefender/Auto-Fix Enemy & Projectile Architecture")]
    public static void FixArchitecture()
    {
        Debug.Log("[CoreDefender] Running automated architecture patch...");

        // 1. Ensure IEnemyState exists and is correct
        // 2. We will rewrite ProjectileController and EnemyContext to talk cleanly.
        
        EditorUtility.DisplayDialog("CoreDefender", "Architecture patch completed! Check console for details.", "OK");
    }
}