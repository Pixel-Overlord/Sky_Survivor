using UnityEngine;
using UnityEngine.UI;

/// <summary>Advances the score only while the player is actively flying through the city.</summary>
public class ScoreController : MonoBehaviour
{
    [SerializeField] private PlaneController planeController;
    [SerializeField] private Text scoreText;

    private bool isPlaneMoving;
    private int score;

    private void Start()
    {
        score = 0;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        isPlaneMoving = planeController.PlaneBody.velocity.sqrMagnitude > 0.01f;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        UpdateScore();
    }

    private void UpdateScore()
    {
        if (isPlaneMoving)
            score += 1;

        scoreText.text = "Score : " + score;
    }
}
