using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TurretUIAutoFixer : MonoBehaviour
{
    [MenuItem("Tools/CoreDefender/Auto-Fix Turret Health & Heat Bars")]
    public static void FixTurretCanvasPrefab()
    {
        string prefabPath = "Assets/Prefabs/TurretCanvas.prefab";
        GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (canvasPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'TurretCanvas.prefab' in Assets/Prefabs/. Please ensure it exists.", "OK");
            return;
        }

        // Instantiate prefab in scene to modify it cleanly
        GameObject canvasInstance = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);
        Undo.RegisterCreatedObjectUndo(canvasInstance, "Fix Turret Canvas");

        // 1. Configure the Background Bar (Extra wide and tall to fit font size 50)
        Transform bgTrans = canvasInstance.transform.Find("BackgroundBar");
        if (bgTrans != null)
        {
            RectTransform bgRt = bgTrans.GetComponent<RectTransform>();
            bgRt.sizeDelta = new Vector2(320f, 60f); // Scaled up to cleanly house size 50 text

            Image bgImage = bgTrans.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            }

            // 2. Configure the Fill Bar inside it
            Transform fillTrans = bgTrans.Find("FillBar");
            if (fillTrans != null)
            {
                RectTransform fillRt = fillTrans.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = new Vector2(4f, 4f);
                fillRt.offsetMax = new Vector2(-4f, -4f);

                Image fillImage = fillTrans.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.type = Image.Type.Filled;
                    fillImage.fillMethod = Image.FillMethod.Horizontal;
                    fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                }
            }

            // 3. Configure Text for HP/Heat Display
            Transform textTrans = bgTrans.Find("TurretHPText");
            TextMeshProUGUI hpText = null;
            if (textTrans == null)
            {
                GameObject textObj = new GameObject("TurretHPText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(bgTrans, false);
                hpText = textObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                hpText = textTrans.GetComponent<TextMeshProUGUI>();
            }

            if (hpText != null)
            {
                RectTransform textRt = hpText.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(-80f, -30f); // Expanded bounds to prevent text clipping
                textRt.offsetMax = new Vector2(80f, 30f);
                hpText.fontSize = 50; // Set font size to 50 as requested
                hpText.alignment = TextAlignmentOptions.Center;
                hpText.color = Color.white;
                hpText.enableWordWrapping = false; // Forces text to stay on a single line
            }
        }

        // Apply changes back to the prefab asset
        PrefabUtility.SaveAsPrefabAsset(canvasInstance, prefabPath);
        DestroyImmediate(canvasInstance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CoreDefender] TurretCanvas prefab updated with font size 50 and expanded width!");
        EditorUtility.DisplayDialog("Success", "TurretCanvas updated with font size 50 and a wider layout!", "OK");
    }
}