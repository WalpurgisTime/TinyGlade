using UnityEngine;

public class MainCamera: MonoBehaviour
{
    public Transform target; // La cible autour de laquelle orbiter
    public float distance = 10.0f; // Distance initiale à la cible
    public float orbitSpeed = 5.0f; // Vitesse d'orbite
    public float panSpeed = 0.5f; // Vitesse de déplacement latéral
    public float zoomSpeed = 5.0f; // Vitesse de zoom avec la molette

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start()
    {
        if (target == null)
        {
            GameObject pivot = new GameObject("Camera Target");
            pivot.transform.position = transform.position + transform.forward * distance;
            target = pivot.transform;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed;
            pitch -= Input.GetAxis("Mouse Y") * orbitSpeed;
            pitch = Mathf.Clamp(pitch, -85, 85);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;

        if (Input.GetMouseButton(0)) 
        {
            Vector3 move = -transform.right * Input.GetAxis("Mouse X") * panSpeed
                         - transform.up * Input.GetAxis("Mouse Y") * panSpeed;
            target.position += move;
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, 2f, 50f);
    }
}
