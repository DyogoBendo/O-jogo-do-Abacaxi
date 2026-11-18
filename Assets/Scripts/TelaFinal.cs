using UnityEngine;
using UnityEngine.SceneManagement;

// Coloque este script no painel de parabéns.
// Quando o painel aparece, ele para o jogador e libera o mouse para clicar nos botões.
// No VR, o painel (Canvas em World Space) aparece na frente do jogador e os botões
// são apertados com o raio do controle + gatilho.
// No campo "Jogador", arraste o JogadorVR (ou o PlayerCapsule, no PC).
// Ligue as funções nos OnClick dos botões:
//   Recomeçar -> TelaFinal.Recomecar
//   Sair      -> TelaFinal.Sair
//   Próxima   -> TelaFinal.ProximaFase
public class TelaFinal : MonoBehaviour
{
    [Tooltip("Arraste o JogadorVR (ou o PlayerCapsule, no PC): ele para de andar quando a tela aparece")]
    public GameObject jogador;
    [Tooltip("VR: distância (metros) em que o painel aparece na frente do jogador")]
    public float distanciaDoJogador = 1.8f;

    [Header("Próxima fase")]
    [Tooltip("Nome exato da cena da próxima fase. Deixe vazio na última fase")]
    public string proximaFase = "";
    [Tooltip("O botão \"Próxima fase\": fica escondido quando não há próxima fase")]
    public GameObject botaoProximaFase;

    void OnEnable()
    {
        // Só age no final do jogo (evita travar o jogador se o painel
        // estiver visível na cena quando o jogo começa)
        if (GameManager.instancia == null || !GameManager.instancia.FeiraAberta) return;

        // VR: o jogador para de andar, e o painel aparece na frente dele
        Transform cabeca = JogadorVR.Cabeca(jogador);
        if (cabeca != null)
        {
            JogadorVR.Travar(jogador, true);
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
                JogadorVR.ColocarNaFrente(canvas.rootCanvas.transform, cabeca, distanciaDoJogador);
            return;
        }

        // PC: desliga os scripts do jogador (andar, olhar, pegar abacaxi)
        if (jogador != null)
            foreach (var script in jogador.GetComponents<MonoBehaviour>())
                script.enabled = false;
    }

    void Start()
    {
        // Na última fase (ou se a cena não estiver no Build Settings), esconde o botão
        if (botaoProximaFase != null)
            botaoProximaFase.SetActive(TemProximaFase());
    }

    bool TemProximaFase()
    {
        return !string.IsNullOrEmpty(proximaFase) && Application.CanStreamedLevelBeLoaded(proximaFase);
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

    public void ProximaFase()
    {
        if (!TemProximaFase()) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(proximaFase);
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