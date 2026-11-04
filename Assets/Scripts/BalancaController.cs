using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Coloque este script no objeto principal da balança ("Scale").
// Ele soma o peso dos abacaxis no prato, inclina o braço para o lado
// mais pesado e confere o equilíbrio quando o botão "Verificar" é clicado.
public class BalancaController : MonoBehaviour
{
    [Header("Desafio")]
    [Tooltip("Peso fixo do outro prato (5, 9 ou 10 kg)")]
    public int pesoAlvo = 5;
    [Tooltip("Peso que já começa no prato dos abacaxis. Use 3 na Balança 3 só se o abacaxi fixo NÃO tiver o script Abacaxi")]
    public int pesoInicial = 0;

    [Header("Peças da balança")]
    [Tooltip("Arraste o Beam")]
    public Transform braco;
    [Tooltip("Arraste o ChainsOriginLeft")]
    public Transform origemEsquerda;
    [Tooltip("Arraste o ChainsOriginRight")]
    public Transform origemDireita;
    [Tooltip("Marcado: abacaxis vão no prato direito. Desmarcado: no esquerdo")]
    public bool abacaxisNoPratoDireito = true;

    [Header("Movimento")]
    [Tooltip("Quantos graus o braço inclina por kg de diferença")]
    public float grausPorKg = 3f;
    [Tooltip("Inclinação máxima do braço, em graus")]
    public float anguloMaximo = 12f;
    [Tooltip("Velocidade do movimento do braço")]
    public float velocidade = 3f;

    [Header("Interface (opcional)")]
    public TMP_Text textoPrato;
    public TMP_Text textoRetorno;

    [Header("Sons (opcional)")]
    public AudioSource audioSource;
    public AudioClip somAcerto;
    [Range(0f, 1f)] public float volumeAcerto = 1f;
    public AudioClip somErro;
    [Range(0f, 1f)] public float volumeErro = 1f;

    [Header("Bloqueio")]
    [Tooltip("Desmarcado: a balança começa bloqueada (o GameManager libera na ordem)")]
    public bool liberada = true;
    [Tooltip("Opcional: objeto que aparece enquanto a balança está bloqueada (ex.: uma corda ou cerca)")]
    public GameObject objetoBloqueio;

    [Header("Ao equilibrar")]
    [Tooltip("Ações chamadas quando o jogador acerta (ex.: avisar o GameManager)")]
    public UnityEvent aoEquilibrar;

    readonly HashSet<Abacaxi> abacaxisNoPrato = new HashSet<Abacaxi>();
    int pesoNoPrato;
    bool concluida;
    float anguloAtual;

    // Posições iniciais usadas para calcular o movimento
    Vector3 pivo, eixo, offsetEsquerda, offsetDireita, offsetBraco;
    Quaternion rotacaoInicialBraco;

    public int PesoNoPrato => pesoNoPrato;
    public bool Concluida => concluida;

    void Start()
    {
        Vector3 esquerda = origemEsquerda.localPosition;
        Vector3 direita = origemDireita.localPosition;

        // O centro do braço fica no meio dos dois pontos onde os pratos ficam pendurados
        pivo = (esquerda + direita) / 2f;

        // Eixo de rotação: horizontal e perpendicular ao braço
        Vector3 linha = direita - esquerda;
        linha.y = 0f;
        eixo = Vector3.Cross(linha.normalized, Vector3.up);

        offsetEsquerda = esquerda - pivo;
        offsetDireita = direita - pivo;
        offsetBraco = braco.localPosition - pivo;
        rotacaoInicialBraco = braco.localRotation;

        RecalcularPeso();
    }

    void Update()
    {
        float pesoEsquerda = abacaxisNoPratoDireito ? pesoAlvo : pesoNoPrato;
        float pesoDireita = abacaxisNoPratoDireito ? pesoNoPrato : pesoAlvo;

        // Ângulo positivo levanta o lado direito (lado esquerdo mais pesado)
        float anguloAlvo = Mathf.Clamp((pesoEsquerda - pesoDireita) * grausPorKg, -anguloMaximo, anguloMaximo);
        if (concluida || !liberada) anguloAlvo = 0f; // bloqueada: o braço fica parado e reto

        anguloAtual = Mathf.Lerp(anguloAtual, anguloAlvo, Time.deltaTime * velocidade);

        Quaternion rotacao = Quaternion.AngleAxis(anguloAtual, eixo);
        braco.localRotation = rotacao * rotacaoInicialBraco;
        braco.localPosition = pivo + rotacao * offsetBraco;

        // Os pratos acompanham as pontas do braço, mas continuam na vertical
        origemEsquerda.localPosition = pivo + rotacao * offsetEsquerda;
        origemDireita.localPosition = pivo + rotacao * offsetDireita;
    }

    // Chamados pelo SensorPrato
    public void AbacaxiEntrou(Abacaxi abacaxi)
    {
        if (abacaxisNoPrato.Add(abacaxi)) RecalcularPeso();
    }

    public void AbacaxiSaiu(Abacaxi abacaxi)
    {
        if (abacaxisNoPrato.Remove(abacaxi)) RecalcularPeso();
    }

    void RecalcularPeso()
    {
        pesoNoPrato = pesoInicial;
        foreach (var abacaxi in abacaxisNoPrato)
            if (abacaxi != null) pesoNoPrato += abacaxi.peso;

        if (textoPrato != null)
            textoPrato.text = "No prato: " + pesoNoPrato + " kg";
    }

    // Ligue no OnClick do botão "Verificar"
    public void Verificar()
    {
        if (concluida) return;

        if (!liberada)
        {
            if (textoRetorno != null) textoRetorno.text = "Bloqueada! Termine a balança anterior.";
            Tocar(somErro, volumeErro);
            return;
        }

        if (pesoNoPrato == pesoAlvo)
        {
            concluida = true;
            if (textoRetorno != null) textoRetorno.text = "Equilibrado!";
            Tocar(somAcerto, volumeAcerto);
            aoEquilibrar.Invoke();
            if (GameManager.instancia != null)
                GameManager.instancia.BalancaConcluida(this);
        }
        else
        {
            if (textoRetorno != null)
                textoRetorno.text = "Ainda não está igual! No prato: " + pesoNoPrato + " kg";
            Tocar(somErro, volumeErro);
        }
    }

    // Chamado pelo GameManager para bloquear ou liberar a balança
    public void Liberar(bool valor)
    {
        liberada = valor;
        if (objetoBloqueio != null) objetoBloqueio.SetActive(!valor);
        if (textoRetorno != null)
            textoRetorno.text = valor ? "" : "Bloqueada! Termine a balança anterior.";
    }

    void Tocar(AudioClip som, float volume)
    {
        if (audioSource != null && som != null)
            audioSource.PlayOneShot(som, volume);
    }
}