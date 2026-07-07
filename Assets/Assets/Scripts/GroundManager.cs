using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] ground;
    [SerializeField] private float groundLength = 1000f;

    private void Update()
    {
        for (int i = 0; i < ground.Length; i++)
        {
            if (player.position.z - ground[i].position.z > groundLength)
            {
                MoveGroundToFront(ground[i]);
            }
        }
    }

    private void MoveGroundToFront(Transform tile)
    {
        float endLimitOfGround = ground[0].position.z;

        for (int i = 1; i < ground.Length; i++)
        {
            if (ground[i].position.z > endLimitOfGround)
            {
                endLimitOfGround = ground[i].position.z;
            }
        }

        tile.position = new Vector3(
            tile.position.x,
            tile.position.y,
            endLimitOfGround + groundLength);
    }
}
