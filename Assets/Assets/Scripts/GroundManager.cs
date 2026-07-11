using UnityEngine;

/// <summary>
/// Keeps the visual ground centered beneath a freely turning plane. The ground is visual support for the
/// streamed city, so it follows on both horizontal axes instead of assuming forward movement on Z only.
/// </summary>
public class GroundManager : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        position.x = player.position.x;
        position.z = player.position.z;
        transform.position = position;
    }
}
