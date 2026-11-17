using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Coloque este script no objeto principal da balança.
// Os dois pratos recebem objetos (pesos de ferro ou abacaxis). O braço inclina
// para o lado mais pesado e fica reto quando os dois lados pesam o mesmo.
// Funciona com pratos pendurados (braço em cima) ou com pratos em cima de
// colunas (braço embaixo): os pratos sempre ficam retos, só sobem e descem.
public class BalancaPesagem : MonoBehaviour
{
    [Header("Peças da balança")]
    [Tooltip("O braço que inclina")]
    public Transform braco;
    [Tooltip("O prato da esquerda (precisa ser filho direto da balança)")]
    public Transform origemEsquerda;
    [Tooltip("O prato da direita (precisa ser filho direto da balança)")]
    public Transform origemDireita;
    [Tooltip("Marcado: o braço gira em torno do próprio centro (balança com braço embaixo dos pratos). " +
             "Desmarcado: gira no ponto entre os pratos (balança de pratos pendurados)")]
    public bool girarNoCentroDoBraco = true;

    [Header("Movimento")]
    [Tooltip("Quantos graus o braço inclina por kg de diferença")]
    public float grausPorKg = 3f;
    [Tooltip("Inclinação máxima do braço, em graus")]
    public float anguloMaximo = 12f;
    [Tooltip("Velocidade do movimento do braço")]
    public float velocidade = 3f;

    [Header("Equilíbrio (opcional)")]
    [Tooltip("Objeto que aparece quando os dois lados pesam o mesmo (ex.: uma luz verde)")]
    public GameObject indicadorEquilibrio;

    [Header("Visor (opcional)")]
    [Tooltip("Texto do visor que diz se a balança está equilibrada")]
    public TMP_Text visor;
    public string textoEquilibrada = "EQUILIBRADA";
    public string textoDesequilibrada = "DESEQUILIBRADA";
    public string textoVazia = "BALANÇA VAZIA";
    public Color corEquilibrada = new Color(0.35f, 1f, 0.45f);
    public Color corDesequilibrada = new Color(1f, 0.35f, 0.3f);
    public Color corVazia = new Color(0.6f, 0.6f, 0.6f);

    // Conta quantos colliders de cada objeto estão no prato (um objeto pode ter vários)
    readonly Dictionary<MonoBehaviour, int> naEsquerda = new Dictionary<MonoBehaviour, int>();
    readonly Dictionary<MonoBehaviour, int> naDireita = new Dictionary<MonoBehaviour, int>();

    public int PesoEsquerda { get; private set; }
    public int PesoDireita { get; private set; }
    public bool Equilibrada => PesoEsquerda > 0 && PesoEsquerda == PesoDireita;

    float anguloAtual;
    Vector3 pivo, eixo, offsetEsquerda, offsetDireita, offsetBraco;
    Quaternion rotacaoInicialBraco;

    // Rigidbody cinemático nos pratos: faz o prato empurrar o que está em cima dele
    Rigidbody corpoEsquerda, corpoDireita;

    void Start()
    {
        Vector3 esquerda = origemEsquerda.localPosition;
        Vector3 direita = origemDireita.localPosition;

        // Ponto em que o braço gira
        pivo = girarNoCentroDoBraco ? braco.localPosition : (esquerda + direita) / 2f;

        // Eixo de rotação: horizontal e perpendicular ao braço
        Vector3 linha = direita - esquerda;
        linha.y = 0f;
        eixo = Vector3.Cross(linha.normalized, Vector3.up);

        offsetEsquerda = esquerda - pivo;
        offsetDireita = direita - pivo;
        offsetBraco = braco.localPosition - pivo;
        rotacaoInicialBraco = braco.localRotation;

        corpoEsquerda = PrepararCorpo(origemEsquerda);
        corpoDireita = PrepararCorpo(origemDireita);

        AtualizarIndicador();
    }

    static Rigidbody PrepararCorpo(Transform prato)
    {
        Rigidbody corpo = prato.GetComponent<Rigidbody>();
        if (corpo != null)
        {
            corpo.isKinematic = true;
            corpo.interpolation = RigidbodyInterpolation.Interpolate;
        }
        return corpo;
    }

    void Update()
    {
        // Recalcula todo quadro: um objeto que estava na mão do jogador
        // passa a contar no momento em que é solto
        Recalcular();

        // Ângulo positivo levanta o lado direito (lado esquerdo mais pesado)
        float anguloAlvo = Mathf.Clamp((PesoEsquerda - PesoDireita) * grausPorKg, -anguloMaximo, anguloMaximo);
        anguloAtual = Mathf.Lerp(anguloAtual, anguloAlvo, Time.deltaTime * velocidade);

        Quaternion rotacao = Quaternion.AngleAxis(anguloAtual, eixo);
        braco.localRotation = rotacao * rotacaoInicialBraco;
        braco.localPosition = pivo + rotacao * offsetBraco;
    }

    // Os pratos se movem no passo da física, para empurrarem o que está em cima
    void FixedUpdate()
    {
        Quaternion rotacao = Quaternion.AngleAxis(anguloAtual, eixo);

        // Os pratos acompanham as pontas do braço, mas continuam na vertical
        MoverPrato(origemEsquerda, corpoEsquerda, PosicaoDoPrato(rotacao, offsetEsquerda));
        MoverPrato(origemDireita, corpoDireita, PosicaoDoPrato(rotacao, offsetDireita));
    }

    // Só a parte horizontal (a ponta do braço) gira; a altura da coluna até o prato fica igual
    Vector3 PosicaoDoPrato(Quaternion rotacao, Vector3 offset)
    {
        Vector3 altura = new Vector3(0f, offset.y, 0f);
        Vector3 pontaDoBraco = offset - altura;
        return pivo + rotacao * pontaDoBraco + altura;
    }

    static void MoverPrato(Transform prato, Rigidbody corpo, Vector3 posicaoLocal)
    {
        if (corpo != null)
        {
            Vector3 posicaoMundo = prato.parent != null ? prato.parent.TransformPoint(posicaoLocal) : posicaoLocal;
            corpo.MovePosition(posicaoMundo);
        }
        else
        {
            prato.localPosition = posicaoLocal;
        }
    }

    // Chamados pelo SensorPesagem de cada prato
    public void Entrou(bool ladoEsquerdo, MonoBehaviour objeto)
    {
        var lado = ladoEsquerdo ? naEsquerda : naDireita;
        lado.TryGetValue(objeto, out int quantos);
        lado[objeto] = quantos + 1;
        Recalcular();
    }

    public void Saiu(bool ladoEsquerdo, MonoBehaviour objeto)
    {
        var lado = ladoEsquerdo ? naEsquerda : naDireita;
        if (!lado.TryGetValue(objeto, out int quantos)) return;
        if (quantos <= 1) lado.Remove(objeto);
        else lado[objeto] = quantos - 1;
        Recalcular();
    }

    void Recalcular()
    {
        PesoEsquerda = Somar(naEsquerda);
        PesoDireita = Somar(naDireita);
        AtualizarIndicador();
    }

    static int Somar(Dictionary<MonoBehaviour, int> lado)
    {
        int total = 0;
        foreach (var objeto in lado.Keys)
        {
            if (objeto == null || NaMaoDoJogador(objeto)) continue;
            if (objeto is Abacaxi abacaxi) total += abacaxi.peso;
            else if (objeto is PesoFerro ferro) total += ferro.peso;
        }
        return total;
    }

    // O PegarObjeto desliga a gravidade do que está segurando: ainda não está apoiado no prato
    static bool NaMaoDoJogador(MonoBehaviour objeto)
    {
        Rigidbody corpo = objeto.GetComponentInParent<Rigidbody>();
        return corpo != null && !corpo.isKinematic && !corpo.useGravity;
    }

    void AtualizarIndicador()
    {
        if (indicadorEquilibrio != null) indicadorEquilibrio.SetActive(Equilibrada);

        if (visor != null)
        {
            string texto; Color cor;
            if (PesoEsquerda == 0 && PesoDireita == 0) { texto = textoVazia; cor = corVazia; }
            else if (Equilibrada) { texto = textoEquilibrada; cor = corEquilibrada; }
            else { texto = textoDesequilibrada; cor = corDesequilibrada; }

            // Só mexe no texto quando muda (é chamado todo quadro)
            if (visor.text != texto) visor.text = texto;
            if (visor.color != cor) visor.color = cor;
        }
    }
}