using UnityEngine;
using UnityEngine.SceneManagement;

// Coloque este script no painel de parabéns.
// Quando o painel aparece, ele para o jogador e libera o mouse para clicar nos botões.
// No campo "Jogador", arraste o PlayerCapsule.
// Ligue as funções nos OnClick dos botões:
//   Recomeçar -> TelaFinal.Recomecar
//   Sair      -> TelaFinal.Sair
public class TelaFinal : MonoBehaviour
{
    [Tooltip("Arraste o PlayerCapsule: os controles dele param quando a tela aparece")]
    public GameObject jogador;

    void OnEnable()
    {
        // Só age no final do jogo (evita travar o jogador se o painel
        // estiver visível na cena quando o jogo começa)
        if (GameManager.instancia == null || !GameManager.instancia.FeiraAberta) return;

        // Desliga os scripts do jogador (andar, olhar, pegar abacaxi)
        if (jogador != null)
            foreach (var script in jogador.GetComponents<MonoBehaviour>())
                script.enabled = false;
    }

    void Update()
    {
        // Mantém o mouse livre enquanto a tela estiver aberta
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Recomecar()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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