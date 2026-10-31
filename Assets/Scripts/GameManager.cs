using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Coloque este script num objeto vazio chamado "GameManager".
// Ele libera as balanças em ordem (a 2 só depois da 1, a 3 só depois da 2)
// e abre a feira quando todas estiverem concluídas.
public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Tooltip("As balanças na ordem do jogo: Balança 1, Balança 2, Balança 3 (arraste os objetos Scale)")]
    public BalancaController[] balancas;

    [Header("Final")]
    [Tooltip("O portão que abre no final")]
    public Portao portao;
    [Tooltip("Placa com \"FEIRA FECHADA\" (some no final)")]
    public GameObject placaFechada;
    [Tooltip("Placa com \"FEIRA ABERTA\" (aparece no final)")]
    public GameObject placaAberta;
    [Tooltip("Painel de parabéns (aparece no final)")]
    public GameObject painelParabens;
    [Tooltip("Segundos de espera, depois da feira abrir, até mostrar o painel de parabéns")]
    public float atrasoParabens = 10f;

    [Header("Virar o jogador para o portão (opcional)")]
    [Tooltip("Arraste o PlayerCapsule")]
    public GameObject jogador;
    [Tooltip("Arraste o PlayerCameraRoot (dentro do PlayerCapsule)")]
    public Transform cameraDoJogador;
    [Tooltip("Para onde o jogador olha. Se ficar vazio, olha para o portão")]
    public Transform pontoParaOlhar;
    [Tooltip("Tempo do giro, em segundos")]
    public float duracaoGiro = 1.5f;

    [Header("Som de vitória (opcional)")]
    public AudioSource audioSource;
    public AudioClip somVitoria;

    [Tooltip("Outras ações ao abrir a feira")]
    public UnityEvent aoAbrirFeira;

    int concluidas;
    bool feiraAberta;

    public bool FeiraAberta => feiraAberta;

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        // Só a primeira balança começa liberada
        for (int i = 0; i < balancas.Length; i++)
            if (balancas[i] != null) balancas[i].Liberar(i == 0);

        if (placaFechada != null) placaFechada.SetActive(true);
        if (placaAberta != null) placaAberta.SetActive(false);
        if (painelParabens != null) painelParabens.SetActive(false);
    }

    // Chamado pela balança quando o jogador acerta
    public void BalancaConcluida(BalancaController balanca)
    {
        concluidas++;

        // Libera a próxima balança da lista
        int indice = System.Array.IndexOf(balancas, balanca);
        if (indice >= 0 && indice + 1 < balancas.Length && balancas[indice + 1] != null)
            balancas[indice + 1].Liberar(true);

        if (concluidas >= balancas.Length && !feiraAberta)
            AbrirFeira();
    }

    void AbrirFeira()
    {
        feiraAberta = true;

        // Vira o jogador para o portão e só então abre a porta
        StartCoroutine(VirarEAbrir());
        if (placaFechada != null) placaFechada.SetActive(false);
        if (placaAberta != null) placaAberta.SetActive(true);
        if (painelParabens != null) Invoke(nameof(MostrarParabens), atrasoParabens);
        if (audioSource != null && somVitoria != null) audioSource.PlayOneShot(somVitoria);

        aoAbrirFeira.Invoke();
    }

    IEnumerator VirarEAbrir()
    {
        Transform alvo = pontoParaOlhar != null ? pontoParaOlhar
                       : (portao != null ? portao.transform : null);

        if (jogador != null && cameraDoJogador != null && alvo != null)
        {
            // Trava os controles do jogador (andar, olhar, pegar abacaxi)
            foreach (var script in jogador.GetComponents<MonoBehaviour>())
                script.enabled = false;

            // Giro horizontal: o corpo do jogador vira para o alvo
            Vector3 direcao = alvo.position - jogador.transform.position;
            direcao.y = 0f;
            Quaternion corpoInicio = jogador.transform.rotation;
            Quaternion corpoFim = Quaternion.LookRotation(direcao);

            // Giro vertical: a câmera inclina para cima ou para baixo até o alvo
            Vector3 daCamera = alvo.position - cameraDoJogador.position;
            float distanciaPlana = new Vector2(daCamera.x, daCamera.z).magnitude;
            float inclinacao = -Mathf.Atan2(daCamera.y, distanciaPlana) * Mathf.Rad2Deg;
            Quaternion cameraInicio = cameraDoJogador.localRotation;
            Quaternion cameraFim = Quaternion.Euler(inclinacao, 0f, 0f);

            for (float t = 0f; t < 1f; t += Time.deltaTime / duracaoGiro)
            {
                float suave = Mathf.SmoothStep(0f, 1f, t);
                jogador.transform.rotation = Quaternion.Slerp(corpoInicio, corpoFim, suave);
                cameraDoJogador.localRotation = Quaternion.Slerp(cameraInicio, cameraFim, suave);
                yield return null;
            }
            jogador.transform.rotation = corpoFim;
            cameraDoJogador.localRotation = cameraFim;
        }

        if (portao != null) portao.Abrir();
    }

    void MostrarParabens()
    {
        painelParabens.SetActive(true);
    }
}