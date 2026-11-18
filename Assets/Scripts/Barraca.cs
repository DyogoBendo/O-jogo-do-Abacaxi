using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Coloque este script num objeto com Box Collider (Is Trigger marcado)
// que cubra a área da barraca onde os abacaxis devem ser colocados.
// A barraca confere sozinha: quando os abacaxis dentro dela estão soltos e parados,
// ela verifica se são os certos. Acertando, completa e solta confetes.
public class Barraca : MonoBehaviour
{
    [Header("Desafio")]
    [Tooltip("Peso (valor de x) dos abacaxis desta barraca")]
    public int pesoCerto = 3;
    [Tooltip("Quantos abacaxis a barraca precisa ter")]
    public int quantidadeNecessaria = 2;
    [Tooltip("Segundos que os abacaxis precisam ficar parados na barraca antes de conferir")]
    public float tempoParaConferir = 0.2f;

    [Header("Confete (opcional)")]
    [Tooltip("Prefab de confete (Assets/Prefabs/Confete) que estoura quando a barraca fica completa")]
    public ParticleSystem prefabConfete;
    [Tooltip("Altura, acima do chão da área, de onde os confetes saem")]
    public float alturaConfete = 1f;

    [Header("Retorno (opcional)")]
    [Tooltip("Texto da placa que mostra o resultado")]
    public TMP_Text textoRetorno;
    [Tooltip("Objeto que aparece quando a barraca fica completa (ex.: uma luz verde)")]
    public GameObject indicadorCompleta;

    [Header("Sons (opcional)")]
    public AudioSource audioSource;
    public AudioClip somAcerto;
    public AudioClip somErro;

    [Header("Ao completar")]
    [Tooltip("Ações chamadas quando a barraca fica completa (depois: avisar o GameManager)")]
    public UnityEvent aoCompletar;

    // Conta quantos colliders de cada abacaxi estão na área (um abacaxi pode ter vários)
    readonly Dictionary<Abacaxi, int> abacaxisNaArea = new Dictionary<Abacaxi, int>();

    public bool Completa { get; private set; }

    float tempoParado;
    string ultimoConferido = "";

    void Reset()
    {
        Collider area = GetComponent<Collider>();
        if (area != null) area.isTrigger = true;
    }

    void Start()
    {
        if (indicadorCompleta != null) indicadorCompleta.SetActive(false);
    }

    void OnTriggerEnter(Collider outro)
    {
        Abacaxi abacaxi = outro.GetComponentInParent<Abacaxi>();
        if (abacaxi == null) return;
        abacaxisNaArea.TryGetValue(abacaxi, out int quantos);
        abacaxisNaArea[abacaxi] = quantos + 1;
    }

    void OnTriggerExit(Collider outro)
    {
        Abacaxi abacaxi = outro.GetComponentInParent<Abacaxi>();
        if (abacaxi == null || !abacaxisNaArea.TryGetValue(abacaxi, out int quantos)) return;
        if (quantos <= 1) abacaxisNaArea.Remove(abacaxi);
        else abacaxisNaArea[abacaxi] = quantos - 1;
    }

    // Confere sozinha quando os abacaxis da área estão soltos e parados
    void Update()
    {
        if (Completa) return;

        if (abacaxisNaArea.Count == 0)
        {
            tempoParado = 0f;
            ultimoConferido = "";
            return;
        }

        foreach (var abacaxi in abacaxisNaArea.Keys)
        {
            if (!EstaParado(abacaxi))
            {
                tempoParado = 0f;
                return;
            }
        }

        tempoParado += Time.deltaTime;
        if (tempoParado < tempoParaConferir) return;

        // Só confere de novo se os abacaxis na área mudaram
        string estado = AbacaxisNaAreaComoTexto();
        if (estado == ultimoConferido) return;
        ultimoConferido = estado;
        Conferir();
    }

    // Solto (não está na mão do jogador) e sem se mexer
    static bool EstaParado(Abacaxi abacaxi)
    {
        if (abacaxi == null) return true;
        if (ObjetoNaMao.Esta(abacaxi)) return false; // ainda está na mão do jogador
        Rigidbody corpo = abacaxi.GetComponentInParent<Rigidbody>();
        if (corpo == null || corpo.isKinematic) return true;
        // Quase parado já basta (não espera o abacaxi parar de balançar/girar)
        return corpo.linearVelocity.sqrMagnitude < 0.25f;
    }

    string AbacaxisNaAreaComoTexto()
    {
        var ids = new List<int>();
        foreach (var abacaxi in abacaxisNaArea.Keys)
            if (abacaxi != null) ids.Add(abacaxi.GetHashCode());
        ids.Sort();
        return string.Join(",", ids);
    }

    void SoltarConfete()
    {
        if (prefabConfete == null) return;

        Collider area = GetComponent<Collider>();
        Vector3 origem = transform.position;
        if (area != null)
        {
            Bounds b = area.bounds;
            origem = new Vector3(b.center.x, b.min.y + alturaConfete, b.center.z);
        }

        ParticleSystem confete = Instantiate(prefabConfete, origem, Quaternion.identity);
        confete.Play();
        var principal = confete.main;
        Destroy(confete.gameObject, principal.duration + principal.startLifetime.constantMax + 0.5f);
    }

    // Confere os abacaxis da área (chamado sozinho pelo Update; pode ser chamado por outros scripts)
    public void Conferir()
    {
        if (Completa) return;

        int quantidade = abacaxisNaArea.Count;
        bool todosCertos = true;
        foreach (var abacaxi in abacaxisNaArea.Keys)
            if (abacaxi.peso != pesoCerto) todosCertos = false;

        if (quantidade == quantidadeNecessaria && todosCertos)
        {
            Completa = true;
            TravarAbacaxis();
            Mostrar("Barraca completa!");
            Tocar(somAcerto);
            SoltarConfete();
            if (indicadorCompleta != null) indicadorCompleta.SetActive(true);
            aoCompletar.Invoke();
            if (GameManager.instancia != null) GameManager.instancia.BarracaConcluida(this);
            return;
        }

        // Dicas sem entregar a resposta
        if (quantidade < quantidadeNecessaria)
            Mostrar("Faltam abacaxis! Esta barraca precisa de " + quantidadeNecessaria + ".");
        else if (quantidade > quantidadeNecessaria)
            Mostrar("Abacaxis demais! Esta barraca precisa de " + quantidadeNecessaria + ".");
        else
            Mostrar("Algum abacaxi não tem o peso certo...");

        // Som de erro só quando o jogador já colocou abacaxis suficientes (não a cada abacaxi)
        if (quantidade >= quantidadeNecessaria) Tocar(somErro);
    }

    // Os abacaxis entregues ficam parados no lugar e não podem mais ser pegos
    void TravarAbacaxis()
    {
        foreach (var abacaxi in abacaxisNaArea.Keys)
        {
            abacaxi.travado = true;

            // VR: desliga o "pegável" para o controle não conseguir mais segurar este abacaxi
            var pegavel = abacaxi.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (pegavel != null) pegavel.enabled = false;

            Rigidbody corpo = abacaxi.GetComponentInParent<Rigidbody>();
            if (corpo != null)
            {
                corpo.linearVelocity = Vector3.zero;
                corpo.angularVelocity = Vector3.zero;
                corpo.isKinematic = true;
            }
        }
    }

    void Mostrar(string mensagem)
    {
        if (textoRetorno != null) textoRetorno.text = mensagem;
    }

    void Tocar(AudioClip som)
    {
        if (audioSource != null && som != null) audioSource.PlayOneShot(som);
    }
}