using UnityEngine;
using UnityEngine.SceneManagement;

// Coloque este script num objeto vazio na cena do menu.
// Ligue as funções nos OnClick dos botões:
//   Iniciar  -> MenuInicial.Iniciar   (abre o vídeo de abertura e, depois dele, o jogo)
//   Sair     -> MenuInicial.Sair
public class MenuInicial : MonoBehaviour
{
    [Tooltip("Painel com os botões Iniciar e Sair")]
    public GameObject painelMenu;
    [Tooltip("Nome exato da cena do jogo")]
    public string cenaDoJogo = "Feira";
    [Tooltip("Cena do vídeo de abertura, tocada antes do jogo. Deixe vazio para ir direto ao jogo")]
    public string cenaDoVideo = "GravacaoVideo";

    void Start()
    {
        if (painelMenu != null) painelMenu.SetActive(true);

        // Libera o mouse para clicar nos botões (o jogo trava o cursor)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Iniciar()
    {
        // Prende e esconde o mouse para o jogo em primeira pessoa
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Primeiro o vídeo de abertura (ele mesmo carrega o jogo quando termina)
        if (!string.IsNullOrEmpty(cenaDoVideo))
            SceneManager.LoadScene(cenaDoVideo);
        else
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
