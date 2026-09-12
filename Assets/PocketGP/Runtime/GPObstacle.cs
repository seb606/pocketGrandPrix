using UnityEngine;

namespace PocketGP {
public sealed class GPObstacle : MonoBehaviour {
    public bool IsBarrel;
    public float Radius = 1.1f;
    Vector3 initialPos;
    Quaternion initialRot;
    Rigidbody rb;
    Collider col;
    float respawnTimer;
    bool isHit;

    void Awake() {
        initialPos = transform.position;
        initialRot = transform.rotation;

        rb = gameObject.GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = IsBarrel ? 35f : 12f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.8f;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        col = gameObject.GetComponent<Collider>();
        if (!col) {
            col = gameObject.GetComponentInChildren<Collider>();
            if (!col) {
                if (IsBarrel) {
                    var cap = gameObject.AddComponent<CapsuleCollider>();
                    cap.radius = 0.44f;
                    cap.height = 1.25f;
                    cap.center = new Vector3(0, 0.62f, 0);
                    col = cap;
                } else {
                    var box = gameObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(0.78f, 0.88f, 0.78f);
                    box.center = new Vector3(0, 0.44f, 0);
                    col = box;
                }
            }
        }
    }

    public void OnCarHit(GPCar car) {
        if (isHit) return;
        isHit = true;
        respawnTimer = 6.0f;

        Vector3 pushDir = (transform.position - car.transform.position).normalized;
        if (pushDir.sqrMagnitude < 0.01f) pushDir = car.transform.forward;

        // Réaction physique et sonore sur la voiture
        car.HitObstacle(pushDir * (IsBarrel ? 4.5f : 3.0f));

        // Propulsion physique réaliste de l'obstacle
        rb.isKinematic = false;
        Vector3 launch = car.Velocity * 1.5f + pushDir * 3.5f + Vector3.up * Random.Range(5.0f, 8.0f);
        rb.linearVelocity = launch;
        rb.angularVelocity = Random.insideUnitSphere * Random.Range(18f, 32f);
    }

    void Update() {
        if (!isHit) return;
        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0) {
            ResetObstacle();
        }
    }

    public void ResetObstacle() {
        isHit = false;
        if (rb) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        transform.position = initialPos;
        transform.rotation = initialRot;
    }
}
}
