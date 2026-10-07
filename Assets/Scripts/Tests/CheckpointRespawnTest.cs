using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tests PBI 4 (unlimited lives / respawning) and PBI 5 (checkpoint detection, multiple checkpoints).
// Builds its own small test world when you press Play, so it doesn't depend on any level art.
// The test body has no controls; it just falls under gravity and gets teleported between cases.
public class CheckpointRespawnTest : MonoBehaviour
{
    private readonly TestReport report = new TestReport("CheckpointRespawnTest");

    private IEnumerator Start()
    {
        yield return null;

        var world = new GameObject("CheckpointRespawnTest_World");
        Collider2D killZone = Box(world, "KillZone", new Vector2(50, -10), new Vector2(400, 2), true);
        killZone.gameObject.AddComponent<KillZone>();
        Box(world, "GroundA", new Vector2(10, 0), new Vector2(4, 1), false);
        Box(world, "GroundB", new Vector2(20, 0), new Vector2(4, 1), false);
        Checkpoint cp1 = Box(world, "CP-1", new Vector2(10, 2), new Vector2(2, 1), true).gameObject.AddComponent<Checkpoint>();
        Checkpoint cp2 = Box(world, "CP-2", new Vector2(20, 2), new Vector2(2, 1), true).gameObject.AddComponent<Checkpoint>();

        var reached = new List<string>();
        Action<Checkpoint> onReached = c => reached.Add(c.Id);
        Checkpoint.AnyReached += onReached;

        Vector2 start = new Vector2(0, 5);
        var body = new GameObject("TestPlayer") { tag = "Player" };
        body.transform.position = start;
        Rigidbody2D rb = body.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        body.AddComponent<BoxCollider2D>().size = Vector2.one;
        PlayerRespawn respawn = body.AddComponent<PlayerRespawn>();

        Vector2 lastRespawnPos = new Vector2(float.NaN, float.NaN);
        respawn.Respawned += p => lastRespawnPos = p.transform.position;

        // 1. No ground under the start: falls into the KillZone.
        yield return WaitFor(() => respawn.RespawnCount >= 1, 5f);
        report.Record("Fall with no checkpoint respawns at level start",
            respawn.RespawnCount >= 1 && Near(lastRespawnPos, start), $"respawned at {lastRespawnPos}");

        // 2. Keeps falling: every fall respawns, no game over.
        yield return WaitFor(() => respawn.RespawnCount >= 4, 10f);
        report.Record("Unlimited lives: respawns on every fall (4 falls)",
            respawn.RespawnCount >= 4 && body.activeInHierarchy, $"respawns={respawn.RespawnCount}");

        // 3. Drop through CP-1 onto GroundA.
        Teleport(rb, new Vector2(10, 5));
        yield return WaitFor(() => cp1.IsReached, 3f);
        report.Record("Checkpoint detected when the player reaches it", cp1.IsReached && reached.Contains("CP-1"));
        report.Record("Respawn point moves to the checkpoint", Near(respawn.RespawnPoint, new Vector2(10, 2)),
            $"respawn point {respawn.RespawnPoint}");
        yield return new WaitForSeconds(1f);
        report.Record("Player lands on a platform without falling through", Mathf.Abs(rb.position.y - 1f) < 0.1f,
            $"y={rb.position.y:F2}");

        // 4. Fall somewhere else: should come back to CP-1.
        int before = respawn.RespawnCount;
        Teleport(rb, new Vector2(40, 5));
        yield return WaitFor(() => respawn.RespawnCount > before, 5f);
        report.Record("Fall after a checkpoint respawns at that checkpoint",
            respawn.RespawnCount > before && Near(lastRespawnPos, new Vector2(10, 2)), $"respawned at {lastRespawnPos}");
        yield return new WaitForSeconds(1f);
        report.Record("A checkpoint only fires once", reached.FindAll(id => id == "CP-1").Count == 1,
            $"CP-1 fired {reached.FindAll(id => id == "CP-1").Count} times");

        // 5. Reach CP-2, then fall: the newest checkpoint wins.
        Teleport(rb, new Vector2(20, 5));
        yield return WaitFor(() => cp2.IsReached, 3f);
        before = respawn.RespawnCount;
        Teleport(rb, new Vector2(40, 5));
        yield return WaitFor(() => respawn.RespawnCount > before, 5f);
        report.Record("Multiple checkpoints: respawns at the latest one",
            cp2.IsReached && Near(lastRespawnPos, new Vector2(20, 2)), $"respawned at {lastRespawnPos}");

        // 6. Gap with no KillZone: the Fall Death Y safety net still respawns.
        killZone.enabled = false;
        yield return new WaitForSeconds(0.5f);
        before = respawn.RespawnCount;
        Teleport(rb, new Vector2(40, 5));
        yield return WaitFor(() => respawn.RespawnCount > before, 6f);
        report.Record("Safety net: falling below Fall Death Y respawns without a KillZone",
            respawn.RespawnCount > before && Near(lastRespawnPos, new Vector2(20, 2)));

        Checkpoint.AnyReached -= onReached;
        Destroy(body);
        Destroy(world);
        report.Summary();
    }

    private static Collider2D Box(GameObject parent, string name, Vector2 pos, Vector2 size, bool trigger)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
        col.isTrigger = trigger;
        return col;
    }

    private static void Teleport(Rigidbody2D rb, Vector2 p)
    {
        rb.linearVelocity = Vector2.zero;
        rb.position = p;
        rb.transform.position = p;
    }

    private static bool Near(Vector2 a, Vector2 b) => Vector2.Distance(a, b) < 0.05f;

    private static IEnumerator WaitFor(Func<bool> condition, float timeout)
    {
        float t = 0f;
        while (!condition() && t < timeout) { t += Time.deltaTime; yield return null; }
    }
}
