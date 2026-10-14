
using UnityEngine;

public class SelectedCharacterLoader : MonoBehaviour
{
    public GameObject capybara;
    public GameObject duck;
    public GameObject monkey;

    public PlayerController playerController;

    void Start()
    {
        string selected = CharacterSelectionManager.SelectedCharacter;

        capybara.SetActive(selected == "Capybara");
        duck.SetActive(selected == "Duck");
        monkey.SetActive(selected == "Monkey");

        GameObject activeCharacter = capybara;

        if (selected == "Duck")
            activeCharacter = duck;
        else if (selected == "Monkey")
            activeCharacter = monkey;

        playerController.characterSprite =
            activeCharacter.GetComponent<SpriteRenderer>();

        playerController.characterAnimator =
            activeCharacter.GetComponent<Animator>();

        Debug.Log("Loaded character: " + selected);
    }
}
