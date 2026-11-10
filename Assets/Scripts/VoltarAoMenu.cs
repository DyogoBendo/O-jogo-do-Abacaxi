using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Coloque este script em qualquer objeto da cena da feira (ex.: o GameManager).
// Ao apertar Esc, o jogo volta para o menu inicial.
public class VoltarAoMenu : MonoBehaviour
{
    [Tooltip("Nome exato da cena do menu")]
    public string cenaDoMenu = "Menu";

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Solta o mouse para poder clicar nos botões do menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(cenaDoMenu);
        }
    }
}
