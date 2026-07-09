using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField] private PlaneController planeController;
    [SerializeField] private BuildingSpawner buildingSpawner;
    [SerializeField] private Text scoreText;

    private bool canPlay;

   

    private int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    private void Update()
    {
        if (planeController.planeGetter.position.y > buildingSpawner.maxHeight)
        {
            canPlay = false;
        }
        else
        {
            canPlay = true;
        }
    }

    private void FixedUpdate()
    {
       scoreUpdate();
    }

    private void scoreUpdate()
    {
        if (canPlay)
        {
            score += 1;
        }
        else
        {
            score -= 1;
        }

        scoreText.text = "Score : " + score;
    }
}
