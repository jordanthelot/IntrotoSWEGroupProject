using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MainMenuReturn : MonoBehaviour
{
    [SerializeField] private ReflectionManager reflectionManager;

    private Button menuButton;

    private void Awake()
    {
        menuButton = GetComponent<Button>();
        menuButton.interactable = false;
    }

    private void Update()
    {
        menuButton.interactable =
            reflectionManager != null &&
            reflectionManager.CanReturnToMainMenu;
    }

    public void ReturnToMenu()
    {
        if (reflectionManager != null)
        {
            reflectionManager.ReturnToMainMenu();
        }
    }
}