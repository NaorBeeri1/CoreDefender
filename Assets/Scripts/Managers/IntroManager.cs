using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float waitTime = 2.0f;
    [SerializeField] private string nextScene = "MainMenuScene";

    private void Awake()
    {
        // Automatically grabs the CanvasGroup attached to this same Canvas object
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f; // Start invisible
        }
    }

    private void Start()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(PlayIntroSequence());
        }
        else
        {
            Debug.LogError("[IntroManager] No CanvasGroup found on this GameObject!");
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Fade In
        yield return StartCoroutine(Fade(0f, 1f));

        // 2. Wait 
        yield return new WaitForSeconds(waitTime);

        // 3. Fade Out
        yield return StartCoroutine(Fade(1f, 0f));

        // 4. Load Main Menu
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeSpeed);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}