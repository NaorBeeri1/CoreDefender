using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class EnemyCanvasSetupTool : MonoBehaviour
{
    [MenuItem("Tools/CoreDefender/Setup Enemy Health Canvas Prefab")]
    public static void SetupEnemyCanvas()
    {
        // 1. Create root Canvas GameObject
        GameObject canvasObj = new GameObject("EnemyCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        
        // Configure Canvas for World Space
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 2f);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // 2. Create BackgroundBar (Container)
        GameObject bgObj = new GameObject("BackgroundBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(60f, 10f);
        bgRect.anchoredPosition = new Vector2(0f, 0f);

        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        bgImage.type = Image.Type.Sliced;
        bgImage.color = new Color(0.102f, 0.102f, 0.102f, 0.9f); // #1A1A1AE6 Dark cyberpunk slate

        // 3. Create FillBar (The health inner meter)
        GameObject fillObj = new GameObject("FillBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObj.transform.SetParent(bgObj.transform, false);

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObj.GetComponent<Image>();
        fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        fillImage.color = new Color(1f, 0.102f, 0.302f, 1f); // #FF1A4D Neon Red/Pink for enemy health

        // 4. Save as a Prefab in Assets/Prefabs/
        string prefabFolderPath = "Assets/Prefabs";
        if (!Directory.Exists(prefabFolderPath))
        {
            Directory.CreateDirectory(prefabFolderPath);
        }

        string prefabPath = Path.Combine(prefabFolderPath, "EnemyCanvas.prefab");
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
        DestroyImmediate(canvasObj);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CoreDefender] EnemyCanvas prefab generated successfully at: " + prefabPath);
        EditorUtility.DisplayDialog("Success", "EnemyCanvas prefab created and saved in Assets/Prefabs/!", "OK");
    }
}