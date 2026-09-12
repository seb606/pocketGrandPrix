using UnityEngine;

namespace PocketGP {
public enum GPAnimType { None, ItemBox, Windmill, Fan, Toaster, Butterfly, Spectator }

public sealed class GPAnimatedDecor : MonoBehaviour {
    public GPAnimType Type;
    public Transform Part1, Part2;
    float timer;
    Vector3 basePos;

    void Start() {
        basePos = transform.localPosition;
        timer = Random.Range(0f, 10f);
    }

    void Update() {
        float dt = Time.deltaTime;
        timer += dt;

        switch (Type) {
            case GPAnimType.ItemBox:
                transform.Rotate(0, 120 * dt, 0, Space.World);
                transform.localPosition = basePos + Vector3.up * (Mathf.Sin(timer * 3f) * 0.15f);
                break;

            case GPAnimType.Windmill:
                if (Part1) Part1.Rotate(0, 0, 75 * dt, Space.Self);
                break;

            case GPAnimType.Fan:
                if (Part1) Part1.Rotate(0, 500 * dt, 0, Space.Self);
                break;

            case GPAnimType.Toaster:
                float cycle = Mathf.Repeat(timer, 4f);
                if (Part1) {
                    float y = cycle > 3.2f && cycle < 3.7f ? Mathf.Sin((cycle - 3.2f) / 0.5f * Mathf.PI) * 1.6f : 0f;
                    Part1.localPosition = new Vector3(0, 1.8f + y, 0);
                }
                break;

            case GPAnimType.Butterfly:
                float angle = timer * 1.8f;
                float r = 2.4f;
                transform.localPosition = basePos + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(timer * 4f) * 0.35f, Mathf.Sin(angle) * r);
                transform.localRotation = Quaternion.Euler(Mathf.Sin(timer * 12f) * 15f, -angle * Mathf.Rad2Deg + 90, 0);
                break;

            case GPAnimType.Spectator:
                float hop = Mathf.Abs(Mathf.Sin(timer * 5f + basePos.x)) * 0.35f;
                transform.localPosition = basePos + Vector3.up * hop;
                if (Part1) Part1.localRotation = Quaternion.Euler(0, Mathf.Sin(timer * 6f) * 25f, 0);
                break;
        }
    }
}
}
