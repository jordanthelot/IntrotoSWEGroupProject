using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the placeholder level and the test scenes from code, so they can be regenerated.
// Menu: Mirror > Build Level1 + Test Scenes.
// Everything here uses a plain white square as placeholder art until Amber's sprites exist.
public static class MirrorSceneBuilder
{
    private const string ArtDir = "Assets/Art/Placeholder";
    private const string SquarePath = ArtDir + "/WhiteSquare.png";
    private const string NoFrictionPath = ArtDir + "/PlayerNoFriction.physicsMaterial2D";
    private const string UnlitSpriteMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

    private static readonly Color Ground = new Color(0.33f, 0.36f, 0.45f);
    private static readonly Color PlayerColor = new Color(0.95f, 0.55f, 0.35f);
    private static readonly Color CheckpointColor = new Color(0.35f, 0.85f, 0.55f, 0.55f);
    private static readonly Color Sky = new Color(0.62f, 0.66f, 0.78f);

    [MenuItem("Mirror/Build Level1 + Test Scenes")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildLevel1();
        BuildGameplayTests();
        BuildMovementTests();
        SetMainMenuPlayScene("Level1");
        AddToBuildSettings("Assets/Scenes/Level1.unity");
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
        Debug.Log("MirrorSceneBuilder: built Level1, GameplayTests and MovementTests.");
    }

    // ---------- Level 1 ----------
    private static void BuildLevel1()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject player = CreatePlayer(new Vector2(0, 1));
        CreateCamera(player.transform);

        var level = new GameObject("Level");
        // Ground with two gaps (x 15-18 and 33-36), a raised step, and walls at both ends.
        Block(level, "Ground 1", new Vector2(5, -0.5f), new Vector2(20, 1), Ground);
        Block(level, "Ground 2", new Vector2(25.5f, -0.5f), new Vector2(15, 1), Ground);
        Block(level, "Ground 3", new Vector2(46, -0.5f), new Vector2(20, 1), Ground);
        Block(level, "Step", new Vector2(28, 1.5f), new Vector2(4, 0.5f), Ground);
        Block(level, "Wall Left", new Vector2(-5.5f, 4), new Vector2(1, 10), Ground);
        Block(level, "Wall Right", new Vector2(56.5f, 4), new Vector2(1, 10), Ground);

        var killZone = new GameObject("KillZone (under gaps)");
        killZone.transform.SetParent(level.transform);
        killZone.transform.position = new Vector2(25, -8);
        var kzCol = killZone.AddComponent<BoxCollider2D>();
        kzCol.size = new Vector2(120, 2);
        kzCol.isTrigger = true;
        killZone.AddComponent<KillZone>();

        var checkpoints = new GameObject("Checkpoints");
        CreateCheckpoint(checkpoints, "CP-1", new Vector2(8, 1));
        CreateCheckpoint(checkpoints, "CP-2", new Vector2(23, 1));
        CreateCheckpoint(checkpoints, "CP-3", new Vector2(44, 1));

        GameObject canvas = CreateReflectionUI();
        var manager = new GameObject("ReflectionManager");
        var trigger = manager.AddComponent<CheckpointReflectionTrigger>();
        var so = new SerializedObject(trigger);
        SerializedProperty list = so.FindProperty("questions");
        string[,] placeholder =
        {
            { "CP-1", "q-why-open", "PLACEHOLDER: Why did you open Instagram the last time you scrolled?" },
            { "CP-2", "q-feel-after", "PLACEHOLDER: How did you feel after your last scrolling session?" },
            { "CP-3", "q-content-mood", "PLACEHOLDER: What kind of content affects your mood the most?" },
        };
        list.arraySize = placeholder.GetLength(0);
        for (int i = 0; i < placeholder.GetLength(0); i++)
        {
            SerializedProperty e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("checkpointId").stringValue = placeholder[i, 0];
            e.FindPropertyRelative("questionId").stringValue = placeholder[i, 1];
            e.FindPropertyRelative("question").stringValue = placeholder[i, 2];
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level1.unity");
    }

    // ---------- Test scenes ----------
    private static void BuildGameplayTests()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(null);
        var tests = new GameObject("Tests");
        tests.AddComponent<CheckpointRespawnTest>();
        tests.AddComponent<ReflectionSubmissionTest>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameplayTests.unity");
    }

    private static void BuildMovementTests()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject player = CreatePlayer(new Vector2(0, 1));
        CreateCamera(player.transform);
        var level = new GameObject("Level");
        Block(level, "Floor", new Vector2(0, -0.5f), new Vector2(20, 1), Ground);
        GameObject wall = Block(level, "Wall", new Vector2(-6.5f, 2.5f), new Vector2(1, 6), Ground);
        var tester = new GameObject("MovementTest").AddComponent<MovementTest>();
        var so = new SerializedObject(tester);
        so.FindProperty("wallOnLeft").objectReferenceValue = wall.GetComponent<Collider2D>();
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MovementTests.unity");
    }

    // ---------- Pieces ----------
    private static GameObject CreatePlayer(Vector2 position)
    {
        var player = new GameObject("Player") { tag = "Player" };
        player.transform.position = position;
        player.transform.localScale = new Vector3(0.8f, 1f, 1f);
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = Square();
        sr.color = PlayerColor;
        sr.sortingOrder = 10;
        ApplyUnlit(sr);
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = player.AddComponent<BoxCollider2D>();
        col.sharedMaterial = NoFriction();
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerRespawn>();
        return player;
    }

    private static void CreateCamera(Transform follow)
    {
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        camGo.transform.position = new Vector3(0, 2, -10);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Sky;
        camGo.AddComponent<AudioListener>();
        if (follow != null) camGo.AddComponent<CameraFollow>().target = follow;
    }

    private static void CreateCheckpoint(GameObject parent, string id, Vector2 pos)
    {
        var go = new GameObject(id);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1, 2, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square();
        sr.color = CheckpointColor;
        ApplyUnlit(sr);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<Checkpoint>(); // ID defaults to the object name (CP-1, CP-2, CP-3)
    }

    private static GameObject Block(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square();
        sr.color = color;
        ApplyUnlit(sr);
        go.AddComponent<BoxCollider2D>();
        return go;
    }

    private static GameObject CreateReflectionUI()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        var canvasGo = new GameObject("ReflectionCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Dim background + centered card
        var panel = UIObject("ReflectionPanel", canvasGo.transform);
        Stretch(panel);
        panel.AddComponent<Image>().color = new Color(0, 0, 0, 0.55f);

        var card = UIObject("Card", panel.transform);
        var cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(1000, 560);
        card.AddComponent<Image>().color = new Color(0.97f, 0.96f, 0.93f);

        TMP_Text title = Text("Title", card.transform, "Time to reflect", 30, FontStyles.Bold, new Vector2(0, 220), new Vector2(900, 50));
        title.color = new Color(0.35f, 0.35f, 0.4f);
        TMP_Text question = Text("Question", card.transform, "Question goes here", 38, FontStyles.Normal, new Vector2(0, 130), new Vector2(900, 120));

        var resources = new TMP_DefaultControls.Resources();
        GameObject inputGo = TMP_DefaultControls.CreateInputField(resources);
        inputGo.name = "AnswerInput";
        inputGo.transform.SetParent(card.transform, false);
        var inputRt = inputGo.GetComponent<RectTransform>();
        inputRt.anchoredPosition = new Vector2(0, -20);
        inputRt.sizeDelta = new Vector2(900, 160);
        var input = inputGo.GetComponent<TMP_InputField>();
        input.lineType = TMP_InputField.LineType.MultiLineSubmit;
        input.textComponent.fontSize = 30;
        input.textComponent.textWrappingMode = TextWrappingModes.Normal;
        input.textComponent.color = Color.black;
        var ph = (TMP_Text)input.placeholder;
        ph.text = "Type your answer, then press Enter";
        ph.fontSize = 30;
        ph.textWrappingMode = TextWrappingModes.Normal;
        input.textComponent.alignment = TextAlignmentOptions.TopLeft;
        ph.alignment = TextAlignmentOptions.TopLeft;

        TMP_Text error = Text("Error", card.transform, "", 26, FontStyles.Normal, new Vector2(0, -135), new Vector2(900, 40));
        error.color = new Color(0.8f, 0.15f, 0.15f);

        GameObject buttonGo = TMP_DefaultControls.CreateButton(resources);
        buttonGo.name = "SubmitButton";
        buttonGo.transform.SetParent(card.transform, false);
        var btnRt = buttonGo.GetComponent<RectTransform>();
        btnRt.anchoredPosition = new Vector2(0, -210);
        btnRt.sizeDelta = new Vector2(260, 70);
        buttonGo.GetComponent<Image>().color = new Color(0.35f, 0.6f, 0.45f);
        var btnText = buttonGo.GetComponentInChildren<TMP_Text>();
        btnText.text = "Submit";
        btnText.fontSize = 30;
        btnText.color = Color.white;

        var ui = canvasGo.AddComponent<ReflectionQuestionUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue = panel;
        so.FindProperty("questionText").objectReferenceValue = question;
        so.FindProperty("answerInput").objectReferenceValue = input;
        so.FindProperty("submitButton").objectReferenceValue = buttonGo.GetComponent<Button>();
        so.FindProperty("errorText").objectReferenceValue = error;
        so.ApplyModifiedPropertiesWithoutUndo();
        return canvasGo;
    }

    private static GameObject UIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TMP_Text Text(string name, Transform parent, string value, float size, FontStyles style, Vector2 pos, Vector2 box)
    {
        var go = UIObject(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = value;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.12f, 0.12f, 0.15f);
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    // ---------- Assets & settings ----------
    private static Sprite Square()
    {
        if (!File.Exists(SquarePath))
        {
            Directory.CreateDirectory(ArtDir);
            var tex = new Texture2D(4, 4);
            tex.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
            File.WriteAllBytes(SquarePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SquarePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SquarePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 4;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
    }

    private static PhysicsMaterial2D NoFriction()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(NoFrictionPath);
        if (mat == null)
        {
            Directory.CreateDirectory(ArtDir);
            mat = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, NoFrictionPath);
        }
        return mat;
    }

    // Unlit so the placeholder blocks are visible without 2D lights.
    private static void ApplyUnlit(SpriteRenderer sr)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(UnlitSpriteMat);
        if (mat != null) sr.sharedMaterial = mat;
    }

    private static void SetMainMenuPlayScene(string sceneName)
    {
        Scene menu = EditorSceneManager.OpenScene("Assets/MainMenu.unity", OpenSceneMode.Single);
        var controller = Object.FindAnyObjectByType<MainMenuController>();
        if (controller == null) { Debug.LogWarning("MirrorSceneBuilder: MainMenuController not found in MainMenu."); return; }
        var so = new SerializedObject(controller);
        so.FindProperty("gameSceneName").stringValue = sceneName;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(menu);
        EditorSceneManager.SaveScene(menu);
    }

    private static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
