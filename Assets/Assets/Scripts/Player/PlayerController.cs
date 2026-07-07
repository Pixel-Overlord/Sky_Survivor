using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script Name: PlayerController
 * 
 * What it does: 
 * - This script is responsible for controlling the player(ship) in the game.
 * - It handles player input, movement, and interactions with the game world.
 */
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float pitchSpeed = 60f;
    [SerializeField] private float maxPitchAngle = 30f;

    private float currentPitch;
    private float targetPitch;

    private void Update()
    {
        handlePitch();
        moveForward();
    }

    private void handlePitch()
    {
        float input = Input.GetAxis("Vertical");

        targetPitch = input * maxPitchAngle;

        if (input != 0)
        {
            transform.Rotate(input * pitchSpeed * Time.deltaTime, 0f, 0f);
        }
        else
        {
            currentPitch = Mathf.Lerp(
                currentPitch,
                targetPitch,
                pitchSpeed * Time.deltaTime);

            transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }

    private void moveForward()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
