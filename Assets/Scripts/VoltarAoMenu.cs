using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Coloque este script em qualquer objeto da cena da feira (ex.: o GameManager).
// Volta para o menu inicial:
//   - PC: apertando Esc;
//   - VR: segurando o botão de menu (≡) do controle esquerdo por alguns segundos
//     (segurar evita sair da fase sem querer).
public class VoltarAoMenu : MonoBehaviour
{
    [Tooltip("Nome exato da cena do menu")]
    public string cenaDoMenu = "Menu";

    [Tooltip("VR: segundos segurando o botão de menu (≡) do controle esquerdo para voltar ao menu")]
    public float segundosSegurando = 1f;

    InputAction botaoMenu;
    float tempoSegurado;

    void OnEnable()
    {
        // Botão de menu (≡) do controle esquerdo do Quest
        botaoMenu = new InputAction("Menu VR", InputActionType.Button);
        botaoMenu.AddBinding("<XRController>{LeftHand}/{MenuButton}");
        botaoMenu.AddBinding("<XRController>{LeftHand}/menu");
        botaoMenu.Enable();
    }

    void OnDisable()
    {
        if (botaoMenu != null) { botaoMenu.Disable(); botaoMenu.Dispose(); botaoMenu = null; }
    }

    void Update()
    {
        // PC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Voltar();
            return;
        }

        // VR
        if (botaoMenu != null && botaoMenu.IsPressed())
        {
            tempoSegurado += Time.deltaTime;
            if (tempoSegurado >= segundosSegurando) Voltar();
        }
        else
        {
            tempoSegurado = 0f;
        }
    }

    void Voltar()
    {
        // Solta o mouse para poder clicar nos botões do menu (PC)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(cenaDoMenu);
    }
}
