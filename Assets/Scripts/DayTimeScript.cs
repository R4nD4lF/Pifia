using UnityEngine;

public class DayTimeScript : MonoBehaviour
{
    [Tooltip("Duración del ciclo completo de día en segundos (360 grados).")]
    public float dayDurationInSeconds = 60f;

    void Update()
    {
        if (dayDurationInSeconds <= 0) return;

        float rotationSpeed = 360f / dayDurationInSeconds;
        
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}

