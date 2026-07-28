using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameOverUISetupTool : MonoBehaviour
{
    [MenuItem("Tools/CoreDefender/Setup Game Over UI")]
    public static void SetupGameOverUI()
    {
        // Find GameCanvas
        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (canvasObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'GameCanvas' in the active scene. Please create a Canvas named 'GameCanvas' first.", "OK");
            return;
        }

        UIManager uiManager = canvasObj.GetComponent<UIManager>();
        if (uiManager == null)
        {
            uiManager = canvasObj.AddComponent<UIManager>();
        }

        // Find or create GameOverPanel
        Transform panelTrans = canvasObj.transform.Find("GameOverPanel");
        GameObject panelObj;
        if (panelTrans == null)
        {
            panelObj = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObj.transform.SetParent(canvasObj.transform, false);
        }
        else
        {
            panelObj = panelTrans.gameObject;
            // Clear existing children to prevent duplication when re-running the tool
            for (int i = panelObj.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(panelObj.transform.GetChild(i).gameObject);
            }
        }

        // Configure GameOverPanel RectTransform to fill screen
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Set panel background color (Semi-transparent black)
        Image panelImage = panelObj.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        // Create "YOU DIED" Text (TMP)
        GameObject textObj = new GameObject("GameOverText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -200f);
        textRect.sizeDelta = new Vector2(600f, 100f);

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = "YOU DIED";
        tmpText.fontSize = 64;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = new Color(1f, 0.1f, 0.3f, 1f); // Neon red/pink

        // Create Restart Button
        GameObject btnObj = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(panelObj.transform, false);

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0f, -50f);
        btnRect.sizeDelta = new Vector2(200f, 60f);

        Button restartButton = btnObj.GetComponent<Button>();
        Image btnImage = btnObj.GetComponent<Image>();
        btnImage.color = new Color(0.1f, 0.8f, 1f, 1f); // Cyberpunk cyan

        // Create Button Text (TMP)
        GameObject btnTextObj = new GameObject("ButtonText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(btnObj.transform, false);

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnTmpText = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnTmpText.text = "REPLAY";
        btnTmpText.fontSize = 28;
        btnTmpText.alignment = TextAlignmentOptions.Center;
        btnTmpText.color = Color.black;

        // Wire UIManager private fields safely via SerializedObject
        SerializedObject serializedUI = new SerializedObject(uiManager);
        serializedUI.FindProperty("gameOverPanel").objectReferenceValue = panelObj;
        serializedUI.FindProperty("restartButton").objectReferenceValue = restartButton;
        serializedUI.ApplyModifiedProperties();

        // Ensure panel starts disabled
        panelObj.SetActive(false);

        EditorUtility.SetDirty(canvasObj);
        Debug.Log("[CoreDefender] Game Over UI successfully rebuilt without duplicates!");
        EditorUtility.DisplayDialog("Success", "Game Over UI layout cleaned and rebuilt successfully!", "OK");
    }
}