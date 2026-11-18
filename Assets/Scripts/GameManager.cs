using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Coloque este script num objeto vazio chamado "GameManager".
// Ele acompanha as barracas (em qualquer ordem) e abre a feira
// quando todas estiverem completas.
public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Tooltip("Todas as barracas do jogo, em qualquer ordem (arraste as áreas com o script Barraca)")]
    public Barraca[] barracas;

    [Tooltip("Opcional: texto que mostra o progresso (ex.: \"Barracas completas: 2 de 4\")")]
    public TMP_Text textoProgresso;

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
    [Tooltip("Objetos que somem da tela quando o portão abre (ex.: o texto de Controles)")]
    public GameObject[] esconderAoAbrir;

    [Header("Virar o jogador para o portão (opcional)")]
    [Tooltip("Arraste o JogadorVR (ou o PlayerCapsule, na versão de PC)")]
    public GameObject jogador;
    [Tooltip("Só na versão de PC: o PlayerCameraRoot (dentro do PlayerCapsule). No VR fica vazio")]
    public Transform cameraDoJogador;
    [Tooltip("Para onde o jogador olha. Se ficar vazio, olha para o portão")]
    public Transform pontoParaOlhar;
    [Tooltip("Tempo do giro, em segundos")]
    public float duracaoGiro = 1.5f;

    [Header("Som de vitória (opcional)")]
    [Tooltip("O mesmo Audio Source que toca a música da feira")]
    public AudioSource audioSource;
    [Tooltip("Música ou som de fim: substitui a música da feira quando o jogo é concluído")]
    public AudioClip somVitoria;
    [Tooltip("Marcado: a música de fim fica repetindo até o jogador sair ou recomeçar")]
    public bool repetirMusicaFinal = false;

    [Tooltip("Outras ações ao abrir a feira")]
    public UnityEvent aoAbrirFeira;

    readonly HashSet<Barraca> concluidas = new HashSet<Barraca>();
    bool feiraAberta;

    public bool FeiraAberta => feiraAberta;

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        AtualizarProgresso();

        if (placaFechada != null) placaFechada.SetActive(true);
        if (placaAberta != null) placaAberta.SetActive(false);
        if (painelParabens != null) painelParabens.SetActive(false);
    }

    // Chamado pela barraca quando o jogador a completa
    public void BarracaConcluida(Barraca barraca)
    {
        if (!concluidas.Add(barraca)) return; // já contada
        AtualizarProgresso();

        if (concluidas.Count >= barracas.Length && !feiraAberta)
            AbrirFeira();
    }

    void AtualizarProgresso()
    {
        if (textoProgresso != null)
            textoProgresso.text = "Barracas completas: " + concluidas.Count + " de " + barracas.Length;
    }

    void AbrirFeira()
    {
        feiraAberta = true;

        // Tira da tela o que não é mais necessário (ex.: os controles)
        if (esconderAoAbrir != null)
            foreach (var objeto in esconderAoAbrir)
                if (objeto != null) objeto.SetActive(false);

        // Vira o jogador para o portão e só então abre a porta
        StartCoroutine(VirarEAbrir());
        if (placaFechada != null) placaFechada.SetActive(false);
        if (placaAberta != null) placaAberta.SetActive(true);
        if (painelParabens != null) Invoke(nameof(MostrarParabens), atrasoParabens);
        // Para a música da feira e toca a música de fim no lugar
        if (audioSource != null && somVitoria != null)
        {
            audioSource.Stop();
            audioSource.clip = somVitoria;
            audioSource.loop = repetirMusicaFinal;
            audioSource.Play();
        }

        aoAbrirFeira.Invoke();
    }

    IEnumerator VirarEAbrir()
    {
        Transform alvo = pontoParaOlhar != null ? pontoParaOlhar
                       : (portao != null ? portao.transform : null);

        // Jogador VR (XR Origin): gira só o corpo, em volta da cabeça.
        // Em VR não dá para inclinar a cabeça de ninguém à força (causa enjoo).
        var origemVR = jogador != null ? jogador.GetComponent<Unity.XR.CoreUtils.XROrigin>() : null;
        if (origemVR != null && alvo != null)
        {
            JogadorVR.Travar(jogador, true); // para de andar e girar com os analógicos

            Transform cabeca = origemVR.Camera.transform;
            Vector3 olhar = cabeca.forward; olhar.y = 0f;
            Vector3 direcao = alvo.position - cabeca.position; direcao.y = 0f;
            float anguloTotal = Vector3.SignedAngle(olhar, direcao, Vector3.up);

            float girado = 0f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duracaoGiro)
            {
                float alvoAgora = anguloTotal * Mathf.SmoothStep(0f, 1f, t);
                origemVR.RotateAroundCameraUsingOriginUp(alvoAgora - girado);
                girado = alvoAgora;
                yield return null;
            }
            origemVR.RotateAroundCameraUsingOriginUp(anguloTotal - girado);

            if (portao != null) portao.Abrir();
            yield break;
        }

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