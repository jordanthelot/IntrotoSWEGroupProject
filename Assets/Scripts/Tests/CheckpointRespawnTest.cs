using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// PBI 4 + 5: Unlimited lives / respawning (Soni) and checkpoint detection, tested against
// Amber's Checkpoint and PlayerCheckpoint.
//
// Part 1 builds a small test level from code: floor, a gap with a KillZone under it, and
// three of Amber's checkpoints. Part 2 loads the real Gameplay scene and checks that
// falling off the level respawns the player there too.
//
// How to run: open Assets/Scenes/Tests/CheckpointRespawnTests and press Play. Read the Console.
public class CheckpointRespawnTest : MonoBehaviour
{
    private readonly TestReport report = new TestReport("CheckpointRespawnTest");
    private Vector2 lastRespawnPosition;

    private IEnumerator Start()
    {
        Application.runInBackground = true; // keep running when Unity isn't the front window
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0) groundLayer = 6;

        // ---------- Part 1: test level ----------
        var level = new GameObject("TestLevel");
        Box(level, "FloorA", new Vector2(-2.5f, -0.5f), new Vector2(15, 1), groundLayer);   // x -10..5
        Box(level, "FloorB", new Vector2(19f, -0.5f), new Vector2(22, 1), groundLayer);     // x 8..30, gap 5..8
        var killZone = Box(level, "KillZone", new Vector2(10, -8f), new Vector2(60, 2), 0);
        killZone.GetComponent<Collider2D>().isTrigger = true;
        killZone.AddComponent<KillZone>();

        Checkpoint cp1 = MakeCheckpoint(level, "CP1", new Vector2(2, 1));
        Checkpoint cp2 = MakeCheckpoint(level, "CP2", new Vector2(12, 1));
        Checkpoint cp3 = MakeCheckpoint(level, "CP3", new Vector2(24, 1));

        Rigidbody2D rb = MakePlayer(level, new Vector2(-5, 0.6f), out PlayerRespawn respawn);
        respawn.fallDeathY = -30f;
        respawn.Respawned += r => lastRespawnPosition = r.GetComponent<Rigidbody2D>().position;
        Vector2 start = rb.position;
        yield return Wait(0.5f);

        // Fall before any checkpoint -> back to the start
        int count = respawn.RespawnCount;
        yield return FallIntoGap(rb);
        report.Record("Fall with no checkpoint respawns at level start",
            respawn.RespawnCount == count + 1 && Near(lastRespawnPosition, start),
            $"respawns {respawn.RespawnCount - count}, at {lastRespawnPosition}");

        // Reach checkpoint 1
        report.Record("Checkpoint starts inactive (yellow)", ColorOf(cp1) == cp1.inactiveColor);
        yield return Teleport(rb, new Vector2(2, 0.6f));
        report.Record("Reaching a checkpoint activates it (turns green)", ColorOf(cp1) == cp1.activeColor);

        count = respawn.RespawnCount;
        yield return FallIntoGap(rb);
        report.Record("Fall after checkpoint 1 respawns at checkpoint 1",
            respawn.RespawnCount == count + 1 && Near(lastRespawnPosition, cp1.transform.position),
            $"at {lastRespawnPosition}");

        // Reach checkpoint 2, then fall
        yield return Teleport(rb, new Vector2(12, 0.6f));
        yield return FallIntoGap(rb);
        report.Record("Multiple checkpoints: respawns at the latest one (2)", Near(lastRespawnPosition, cp2.transform.position),
            $"at {lastRespawnPosition}");

        // Going back past checkpoint 1 doesn't move the respawn point back
        yield return Teleport(rb, new Vector2(2, 0.6f));
        yield return FallIntoGap(rb);
        report.Record("Passing an earlier checkpoint again keeps the latest one", Near(lastRespawnPosition, cp2.transform.position),
            $"at {lastRespawnPosition}");

        // Unlimited lives
        count = respawn.RespawnCount;
        bool allBack = true;
        for (int i = 0; i < 10; i++)
        {
            yield return FallIntoGap(rb);
            allBack &= Near(lastRespawnPosition, cp2.transform.position);
        }
        report.Record("Unlimited lives: 10 falls in a row all respawn", respawn.RespawnCount == count + 10 && allBack,
            $"respawns {respawn.RespawnCount - count}");

        // Something that isn't the player doesn't trigger a checkpoint
        var crate = new GameObject("Crate");
        crate.transform.SetParent(level.transform);
        crate.transform.position = new Vector2(24, 3);
        crate.AddComponent<Rigidbody2D>().gravityScale = 3;
        crate.AddComponent<BoxCollider2D>().size = Vector2.one * 0.5f;
        yield return Wait(1f);
        report.Record("Only the player activates a checkpoint", ColorOf(cp3) == cp3.inactiveColor);

        // Safety net: falling below Fall Death Y respawns even with no KillZone
        killZone.SetActive(false);
        count = respawn.RespawnCount;
        yield return FallIntoGap(rb, 4f);
        report.Record("Safety net: falling below Fall Death Y respawns without a KillZone",
            respawn.RespawnCount == count + 1 && Near(lastRespawnPosition, cp2.transform.position),
            $"respawns {respawn.RespawnCount - count}");
        Destroy(level);
        yield return null;

        // ---------- Part 2: the real Gameplay scene ----------
        if (!Application.CanStreamedLevelBeLoaded("Gameplay"))
        {
            report.Record("Gameplay scene is in the build scene list", false);
            report.Summary();
            yield break;
        }
        SceneManager.LoadScene("Gameplay");
        yield return null;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerRespawn gameRespawn = player != null ? player.GetComponent<PlayerRespawn>() : null;
        Vector2 spawnPoint = player != null ? (Vector2)player.transform.position : Vector2.zero; // where PlayerCheckpoint starts
        yield return Wait(1f);
        report.Record("Gameplay: Player has PlayerRespawn", gameRespawn != null);
        report.Record("Gameplay: a KillZone is in the scene", FindAnyObjectByType<KillZone>() != null);

        if (gameRespawn != null)
        {
            Rigidbody2D grb = player.GetComponent<Rigidbody2D>();
            Vector2 gameStart = spawnPoint;
            gameRespawn.Respawned += r => lastRespawnPosition = r.GetComponent<Rigidbody2D>().position;

            // Drop the player off the right end of the level
            float rightEdge = float.MinValue, top = float.MinValue;
            foreach (var col in Physics2D.OverlapAreaAll(new Vector2(-100000, -100000), new Vector2(100000, 100000)))
            {
                if (col.isTrigger || col.attachedRigidbody == grb) continue;
                rightEdge = Mathf.Max(rightEdge, col.bounds.max.x);
                top = Mathf.Max(top, col.bounds.max.y);
            }
            count = gameRespawn.RespawnCount;
            yield return Teleport(grb, new Vector2(rightEdge + 5f, top + 2f));
            yield return Wait(4f);
            report.Record("Gameplay: falling off the level respawns the player",
                gameRespawn.RespawnCount > count && Near(lastRespawnPosition, gameStart),
                $"respawns {gameRespawn.RespawnCount - count}, at {lastRespawnPosition}, start {gameStart}");
        }

        report.Summary();
    }

    // Drops the player into the gap between the two floors and waits for the respawn.
    private IEnumerator FallIntoGap(Rigidbody2D rb, float timeout = 2.5f)
    {
        PlayerRespawn respawn = rb.GetComponent<PlayerRespawn>();
        int before = respawn.RespawnCount;
        rb.linearVelocity = Vector2.zero;
        rb.position = new Vector2(6.5f, 1f);
        for (float t = 0; t < timeout && respawn.RespawnCount == before; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();
        yield return Wait(0.3f);
    }

    private static IEnumerator Teleport(Rigidbody2D rb, Vector2 p)
    {
        rb.linearVelocity = Vector2.zero;
        rb.position = p;
        for (int i = 0; i < 5; i++) yield return new WaitForFixedUpdate();
    }

    private static IEnumerator Wait(float seconds)
    {
        for (float t = 0; t < seconds; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
    }

    private static bool Near(Vector2 a, Vector2 b) => Vector2.Distance(a, b) < 0.2f;

    private static Color ColorOf(Checkpoint cp) => cp.GetComponent<SpriteRenderer>().color;

    private static GameObject Box(GameObject parent, string name, Vector2 center, Vector2 size, int layer)
    {
        var go = new GameObject(name) { layer = layer };
        go.transform.SetParent(parent.transform);
        go.transform.position = center;
        go.AddComponent<BoxCollider2D>().size = size;
        return go;
    }

    private static Checkpoint MakeCheckpoint(GameObject parent, string name, Vector2 pos)
    {
        var go = new GameObject(name);
        go.SetActive(false);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<BoxCollider2D>().size = new Vector2(1, 2);
        var cp = go.AddComponent<Checkpoint>();
        go.SetActive(true);
        return cp;
    }

    private static Rigidbody2D MakePlayer(GameObject parent, Vector2 pos, out PlayerRespawn respawn)
    {
        var go = new GameObject("TestPlayer") { tag = "Player" };
        go.SetActive(false);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.5f, 1f);
        go.AddComponent<PlayerCheckpoint>();
        respawn = go.AddComponent<PlayerRespawn>();
        go.SetActive(true);
        return rb;
    }
}
