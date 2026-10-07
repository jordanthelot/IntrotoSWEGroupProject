using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// PBI 4: Test movement, jumping, and collisions.
// Finds the object tagged "Player" and presses real keys through the Input System
// (D, A, Right/Left arrows, Space), then checks how the player moved.
// Put it in a scene that has the player standing on a platform. Optional: assign a wall
// to the left of the player to test that the player can't walk through it.
// Note: click inside the Game view after pressing Play if keys seem to be ignored.
public class MovementTest : MonoBehaviour
{
    [Tooltip("Optional: a solid wall to the LEFT of the player.")]
    [SerializeField] private Collider2D wallOnLeft;

    private readonly TestReport report = new TestReport("MovementTest");

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null || !player.TryGetComponent(out Rigidbody2D rb))
        {
            Debug.LogWarning("MovementTest SKIPPED: no object tagged Player with a Rigidbody2D in this scene.");
            yield break;
        }
        Keyboard kb = Keyboard.current != null ? Keyboard.current : InputSystem.AddDevice<Keyboard>();

        yield return Settle(rb);

        float x0 = rb.position.x;
        yield return new WaitForSeconds(0.5f);
        report.Record("No movement without input", Mathf.Abs(rb.position.x - x0) < 0.05f, $"moved {rb.position.x - x0:F2}");

        yield return CheckMove(kb, rb, Key.D, +1, "Moves right (D)");
        yield return CheckMove(kb, rb, Key.A, -1, "Moves left (A)");
        yield return CheckMove(kb, rb, Key.RightArrow, +1, "Moves right (Right arrow)");
        yield return CheckMove(kb, rb, Key.LeftArrow, -1, "Moves left (Left arrow)");

        // Jump
        yield return Settle(rb);
        float y0 = rb.position.y;
        float maxY = y0;
        Press(kb, Key.Space);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Release(kb);
        for (float t = 0; t < 1.5f; t += Time.deltaTime) { maxY = Mathf.Max(maxY, rb.position.y); yield return null; }
        report.Record("Jumps (Space)", maxY > y0 + 0.5f, $"rose {maxY - y0:F2}");

        yield return Settle(rb);
        report.Record("Lands back on the platform (ground collision)", Mathf.Abs(rb.position.y - y0) < 0.05f,
            $"y before {y0:F2}, after {rb.position.y:F2}");

        // Can't jump again in mid-air
        Press(kb, Key.Space);
        yield return new WaitForFixedUpdate();
        Release(kb);
        yield return new WaitForSeconds(0.25f);
        float peakAfterFirst = rb.position.y;
        Press(kb, Key.Space);
        yield return new WaitForFixedUpdate();
        Release(kb);
        float maxAfterSecond = rb.position.y;
        for (float t = 0; t < 0.6f; t += Time.deltaTime) { maxAfterSecond = Mathf.Max(maxAfterSecond, rb.position.y); yield return null; }
        report.Record("No double jump in mid-air", maxAfterSecond < peakAfterFirst + 1.5f,
            $"height at 2nd press {peakAfterFirst - y0:F2}, max after {maxAfterSecond - y0:F2}");
        yield return Settle(rb);

        if (wallOnLeft != null)
        {
            float wallRight = wallOnLeft.bounds.max.x;
            Press(kb, Key.A);
            yield return new WaitForSeconds(3f);
            Release(kb);
            yield return Settle(rb);
            float playerLeft = player.GetComponent<Collider2D>().bounds.min.x;
            report.Record("Wall collision: can't walk through a wall", playerLeft >= wallRight - 0.05f,
                $"player left edge {playerLeft:F2}, wall edge {wallRight:F2}");
        }

        Release(kb);
        report.Summary();
    }

    private IEnumerator CheckMove(Keyboard kb, Rigidbody2D rb, Key key, int direction, string caseName)
    {
        yield return Settle(rb);
        float x0 = rb.position.x;
        Press(kb, key);
        yield return new WaitForSeconds(0.4f);
        Release(kb);
        yield return new WaitForSeconds(0.1f);
        float dx = rb.position.x - x0;
        report.Record(caseName, dx * direction > 0.5f, $"moved {dx:F2}");
    }

    private static void Press(Keyboard kb, Key key) => InputSystem.QueueStateEvent(kb, new KeyboardState(key));
    private static void Release(Keyboard kb) => InputSystem.QueueStateEvent(kb, new KeyboardState());

    private static IEnumerator Settle(Rigidbody2D rb)
    {
        float t = 0f;
        yield return new WaitForSeconds(0.2f);
        while (rb.linearVelocity.sqrMagnitude > 0.0001f && t < 3f) { t += Time.deltaTime; yield return null; }
    }
}
