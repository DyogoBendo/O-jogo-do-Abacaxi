using UnityEngine;
using UnityEngine.SceneManagement;

// Coloque este script num objeto vazio na cena do menu.
// Ligue as funções nos OnClick dos botões:
//   Iniciar  -> MenuInicial.Iniciar
//   Sair     -> MenuInicial.Sair
//   Começar  -> MenuInicial.Comecar   (no painel de instruções)
//   Voltar   -> MenuInicial.Voltar    (opcional, no painel de instruções)
public class MenuInicial : MonoBehaviour
{
    [Tooltip("Painel com os botões Iniciar e Sair")]
    public GameObject painelMenu;
    [Tooltip("Painel com a explicação do jogo e o botão Começar")]
    public GameObject painelInstrucoes;
    [Tooltip("Nome exato da cena do jogo")]
    public string cenaDoJogo = "Feira";

    void Start()
    {
        painelMenu.SetActive(true);
        painelInstrucoes.SetActive(false);

        // Libera o mouse para clicar nos botões (o jogo trava o cursor)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Iniciar()
    {
        painelMenu.SetActive(false);
        painelInstrucoes.SetActive(true);
    }

    public void Voltar()
    {
        painelInstrucoes.SetActive(false);
        painelMenu.SetActive(true);
    }

    public void Comecar()
    {
        SceneManager.LoadScene(cenaDoJogo);
    }

    public void Sair()
    {
        Application.Quit();
#if UNITY_EDITOR
        // No editor, o Quit não funciona; então só para o Play
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
