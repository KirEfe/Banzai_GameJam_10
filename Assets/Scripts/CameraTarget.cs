using UnityEngine;
using Unity.Cinemachine;

public class CameraTarget : MonoBehaviour
{
    // private void Start()
    // {
    //     GameObject player = GameObject.FindWithTag("Player");
    //     if (player != null)
    //         GetComponent<CinemachineCamera>().Target.TrackingTarget = player.transform;
    // }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player не найден! Проверь тег.");
            return;
        }

        var cam = GetComponent<CinemachineCamera>();
        if (cam == null)
        {
            Debug.LogError("CinemachineCamera не найден на объекте!");
            return;
        }

        cam.Target.TrackingTarget = player.transform;
        Debug.Log("Камера нашла игрока: " + player.name);
    }
}