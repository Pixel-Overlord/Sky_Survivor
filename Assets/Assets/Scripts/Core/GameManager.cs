using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool isGameOver;
    private bool isPaused;

    [SerializeField] private Animator gameOverAnimaion;
    [SerializeField] private GameObject restartButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        restartButton.SetActive(false);
    }

    public bool IsGameOver => isGameOver;

    private void Update()
    {
        if (isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        gameOverAnimaion.Play("blink");

        Debug.Log("GAME OVER");

        restartButton.SetActive(true);
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}