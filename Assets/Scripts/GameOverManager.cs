using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a GameOverManager GameObject in the scene.
/// Call GameOverManager.Instance.ShowGameOver() when the player dies.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Scene Name to Reload")]
    [Tooltip("Exact name of the gameplay scene to load on Replay.")]
    public string gameSceneName = "SampleScene";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Called by PlayerMovement when the player dies.</summary>
    public void ShowGameOver()
    {
        StartCoroutine(LoadGameOverScene());
    }

    IEnumerator LoadGameOverScene()
    {
        // Give the death animation a moment to play before cutting away
        yield return new WaitForSeconds(1.4f);
        SceneManager.LoadScene("GameOver");
    }
}
