using System.Collections;
using UnityEngine;

// PBI 4: Test movement, jumping, and collisions (Soni).
// Tests Amber's PlayerController. Builds a small test level from code (floor + wall),
// adds a player set up like the one in the Gameplay scene, and drives it through
// PlayerController.inputOverride instead of the keyboard, so the result doesn't depend
// on anyone pressing keys.
//
// How to run: open Assets/Scenes/Tests/MovementTests and press Play. Read the Console.
public class MovementTest : MonoBehaviour
{
    private readonly TestReport report = new TestReport("MovementTest");
    private const float O = 1000f; // x offset of the test level

    private class ScriptedInput : IPlayerInputSource
    {
        public float horizontal;
        private bool jumpPressed;
        public float Horizontal => horizontal;
        public void PressJump() => jumpPressed = true;
        public bool ConsumeJumpPressed() { bool j = jumpPressed; jumpPressed = false; return j; }
    }

    private IEnumerator Start()
    {
        Application.runInBackground = true; // keep running when Unity isn't the front window
        Time.timeScale = 1f;
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0) groundLayer = 6; // the layer Amber's player checks for ground

        // Built far away from anything else in the scene. Floor from O-10 to O+30, top at y=0.
        // Wall on the left at O-10.
        Box("TestFloor", new Vector2(O + 10, -0.5f), new Vector2(40, 1), groundLayer);
        Box("TestWall", new Vector2(O - 10.5f, 2), new Vector2(1, 6), groundLayer);
        if (Camera.main != null) Camera.main.transform.position = new Vector3(O + 5, 2, -10);

        var input = new ScriptedInput();
        PlayerController player = MakePlayer(new Vector2(O, 0.6f), groundLayer, input, out Rigidbody2D rb, out SpriteRenderer sprite);

        yield return Settle(rb);
        float x0 = rb.position.x;
        yield return new WaitForSeconds(0.5f);
        report.Record("No movement without input", Mathf.Abs(rb.position.x - x0) < 0.05f, $"moved {rb.position.x - x0:F2}");

        // Right
        x0 = rb.position.x;
        input.horizontal = 1;
        yield return new WaitForSeconds(0.5f);
        input.horizontal = 0;
        report.Record("Moves right", rb.position.x > x0 + 1f, $"moved {rb.position.x - x0:F2}");
        report.Record("Faces right when moving right", !sprite.flipX);
        yield return Settle(rb);

        // Left
        x0 = rb.position.x;
        input.horizontal = -1;
        yield return new WaitForSeconds(0.5f);
        input.horizontal = 0;
        report.Record("Moves left", rb.position.x < x0 - 1f, $"moved {rb.position.x - x0:F2}");
        report.Record("Faces left when moving left", sprite.flipX);
        yield return Settle(rb);
        report.Record("Stops when input is released", Mathf.Abs(rb.linearVelocity.x) < 0.01f, $"vx {rb.linearVelocity.x:F2}");

        // Jump
        float y0 = rb.position.y;
        float maxY = y0;
        input.PressJump();
        for (float t = 0; t < 1.5f; t += Time.deltaTime) { maxY = Mathf.Max(maxY, rb.position.y); yield return null; }
        report.Record("Jumps", maxY > y0 + 1f, $"rose {maxY - y0:F2}");
        yield return Settle(rb);
        report.Record("Lands back on the floor (ground collision)", Mathf.Abs(rb.position.y - y0) < 0.05f,
            $"y before {y0:F2}, after {rb.position.y:F2}");

        // No double jump: press jump again while in the air
        input.PressJump();
        yield return new WaitForSeconds(0.3f);
        float heightAtSecondPress = rb.position.y;
        input.PressJump();
        float maxAfter = heightAtSecondPress;
        for (float t = 0; t < 0.8f; t += Time.deltaTime) { maxAfter = Mathf.Max(maxAfter, rb.position.y); yield return null; }
        report.Record("No double jump in mid-air", maxAfter < heightAtSecondPress + 1f,
            $"height at 2nd press {heightAtSecondPress - y0:F2}, highest after {maxAfter - y0:F2}");
        yield return Settle(rb);

        // Wall collision
        input.horizontal = -1;
        yield return new WaitForSeconds(3f);
        input.horizontal = 0;
        yield return Settle(rb);
        float playerLeft = player.GetComponent<Collider2D>().bounds.min.x;
        report.Record("Wall collision: can't walk through a wall", playerLeft >= O - 10f - 0.05f,
            $"player left edge {playerLeft - O:F2}, wall at -10");

        // Disabled controller (what the reflection panel does) ignores input
        player.enabled = false;
        x0 = rb.position.x;
        input.horizontal = 1;
        yield return new WaitForSeconds(0.5f);
        input.horizontal = 0;
        player.enabled = true;
        report.Record("No movement while controls are disabled (reflection open)", Mathf.Abs(rb.position.x - x0) < 0.3f,
            $"moved {rb.position.x - x0:F2}");

        report.Summary();
    }

    private static GameObject Box(string name, Vector2 center, Vector2 size, int layer)
    {
        var go = new GameObject(name) { layer = layer };
        go.transform.position = center;
        go.AddComponent<BoxCollider2D>().size = size;
        return go;
    }

    // Same physics settings as the Player in Amber's Gameplay scene.
    private static PlayerController MakePlayer(Vector2 pos, int groundLayer, IPlayerInputSource input,
                                                out Rigidbody2D rb, out SpriteRenderer sprite)
    {
        var go = new GameObject("TestPlayer");
        go.SetActive(false);
        go.transform.position = pos;
        rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.5f, 1f);

        var check = new GameObject("GroundCheck").transform;
        check.SetParent(go.transform, false);
        check.localPosition = new Vector3(0, -0.5f, 0);

        var visual = new GameObject("Sprite");
        visual.transform.SetParent(go.transform, false);
        sprite = visual.AddComponent<SpriteRenderer>();

        var pc = go.AddComponent<PlayerController>();
        pc.moveSpeed = 5f;
        pc.jumpForce = 10f;
        pc.groundCheck = check;
        pc.groundCheckRadius = 0.15f;
        pc.groundLayer = 1 << groundLayer;
        pc.characterSprite = sprite;
        pc.inputOverride = input;
        go.SetActive(true);
        return pc;
    }

    private static IEnumerator Settle(Rigidbody2D rb)
    {
        for (int i = 0; i < 200; i++)
        {
            yield return new WaitForFixedUpdate();
            if (rb.linearVelocity.sqrMagnitude < 0.0001f) { yield return new WaitForFixedUpdate(); yield break; }
        }
    }
}
