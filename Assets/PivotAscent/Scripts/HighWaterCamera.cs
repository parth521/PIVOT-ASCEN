using UnityEngine;
using PivotAscent;
public sealed class HighWaterCamera : MonoBehaviour
{
    public PivotPlayer player;
    public float minimumY = 6f;
    float highestY;
    float shakeTime;
    float shakeStrength;

    public void Shake(float duration = .38f, float strength = .16f)
    {
        shakeTime = Mathf.Max(shakeTime, duration);
        shakeStrength = Mathf.Max(shakeStrength, strength);
    }

    void LateUpdate()
    {
        if (player == null) return;
        highestY = Mathf.Max(highestY, player.transform.position.y, player.pivot.position.y, player.swingNode.position.y);
        float target = Mathf.Max(minimumY, highestY + 2.5f);
        Vector3 position = new Vector3(0, Mathf.Lerp(transform.position.y, target, Time.deltaTime * 2.5f), -10);
        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            float fade = Mathf.Clamp01(shakeTime / .38f);
            Vector2 offset = Random.insideUnitCircle * shakeStrength * fade;
            position += new Vector3(offset.x, offset.y, 0);
        }
        transform.position = position;
    }
}
