using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu: Mirror > Wire Login Buttons.
// Adds Jordan's UnityPlayerAccountLogin + LoginFlow to the SceneManager object in the
// Login and CreateAccount scenes and points the sign-in buttons at LoginFlow.OnLoginPressed.
// Safe to run more than once: it never adds duplicate components or listeners.
public static class MirrorLoginWiring
{
    [MenuItem("Mirror/Wire Login Buttons")]
    public static void WireAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Wire("Assets/Scenes/Login.unity", "LoginButton");
        Wire("Assets/Scenes/CreateAccount.unity", "CreateAccountButton");

        EditorSceneManager.OpenScene("Assets/Scenes/Login.unity");
        Debug.Log("MirrorLoginWiring: done.");
    }

    private static void Wire(string scenePath, string buttonName)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject manager = Find(scene, "SceneManager");
        GameObject buttonObject = Find(scene, buttonName);
        if (manager == null || buttonObject == null)
        {
            Debug.LogError($"MirrorLoginWiring: {scenePath} is missing SceneManager or {buttonName}. Nothing changed.");
            return;
        }

        if (manager.GetComponent<UnityPlayerAccountLogin>() == null) manager.AddComponent<UnityPlayerAccountLogin>();
        LoginFlow flow = manager.GetComponent<LoginFlow>();
        if (flow == null) flow = manager.AddComponent<LoginFlow>();

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"MirrorLoginWiring: {buttonName} in {scenePath} has no Button component.");
            return;
        }

        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (button.onClick.GetPersistentTarget(i) == flow &&
                button.onClick.GetPersistentMethodName(i) == nameof(LoginFlow.OnLoginPressed))
                UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
        UnityEventTools.AddPersistentListener(button.onClick, flow.OnLoginPressed);

        EditorUtility.SetDirty(button);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"MirrorLoginWiring: {buttonName} in {scenePath} -> LoginFlow.OnLoginPressed ({button.onClick.GetPersistentEventCount()} listener(s)).");
    }

    private static GameObject Find(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t.gameObject;
        }
        return null;
    }
}
