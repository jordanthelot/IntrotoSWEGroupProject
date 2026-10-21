using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor menu for Soni's PBI 4 work on top of the Gameplay scene.
//   Mirror > Add Respawn To Gameplay: adds PlayerRespawn to the Player and a KillZone under the level.
// Safe to run more than once.
public static class MirrorGameplaySetup
{
    private const string GameplayScene = "Assets/Scenes/Gameplay.unity";

    [MenuItem("Mirror/Add Respawn To Gameplay")]
    public static void AddRespawnToGameplay()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("Stop Play mode first."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);
        GameObject player = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.CompareTag("Player")) { player = t.gameObject; break; }
        if (player == null) { Debug.LogError("MirrorGameplaySetup: no object tagged Player in Gameplay."); return; }

        // Where is the level? Use every solid collider that isn't the player.
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Collider2D col in root.GetComponentsInChildren<Collider2D>(true))
            {
                if (col.isTrigger || col.GetComponentInParent<Rigidbody2D>() == playerRb) continue;
                Bounds b = ColliderBounds(col);
                minX = Mathf.Min(minX, b.min.x); maxX = Mathf.Max(maxX, b.max.x); minY = Mathf.Min(minY, b.min.y);
            }
        if (minX == float.MaxValue) { Debug.LogError("MirrorGameplaySetup: no ground colliders found in Gameplay."); return; }

        PlayerRespawn respawn = player.GetComponent<PlayerRespawn>();
        if (respawn == null) respawn = player.AddComponent<PlayerRespawn>();
        respawn.fallDeathY = minY - 30f;
        EditorUtility.SetDirty(respawn);

        GameObject kill = GameObject.Find("KillZone");
        if (kill == null || kill.scene != scene) kill = new GameObject("KillZone");
        kill.transform.position = new Vector3((minX + maxX) / 2f, minY - 10f, 0f);
        BoxCollider2D box = kill.GetComponent<BoxCollider2D>();
        if (box == null) box = kill.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2((maxX - minX) + 200f, 4f);
        if (kill.GetComponent<KillZone>() == null) kill.AddComponent<KillZone>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"MirrorGameplaySetup: PlayerRespawn on Player (fall death y {respawn.fallDeathY:F1}), " +
                  $"KillZone at {kill.transform.position}, width {box.size.x:F1}.");
    }

    // Bounds work for colliders in a closed-physics editor scene too.
    private static Bounds ColliderBounds(Collider2D col)
    {
        Bounds b = col.bounds;
        if (b.size == Vector3.zero && col is BoxCollider2D bc)
            b = new Bounds(bc.transform.TransformPoint(bc.offset), Vector3.Scale(bc.size, bc.transform.lossyScale));
        return b;
    }
}
