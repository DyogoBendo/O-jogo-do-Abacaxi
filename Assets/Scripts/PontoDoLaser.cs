using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

// Coloque este script no JogadorVR (o XR Origin) ou no XR Origin do Menu.
// Mostra uma bolinha na ponta do raio (laser) de cada mão,
// no ponto exato onde ele está tocando um objeto que dá para pegar (abacaxi ou peso)
// ou um botão de um painel (Menu, tela de parabéns).
// A bolinha some quando a mão está segurando algo ou quando o raio não está em nada disso.
public class PontoDoLaser : MonoBehaviour
{
    [Tooltip("Marcado: a bolinha também aparece quando o raio está num botão (ou outro controle de UI que dá para apertar)")]
    public bool mostrarNosBotoes = true;
    [Tooltip("Material da bolinha (Assets/Materiais/PontoLaser, vermelho)")]
    public Material material;
    [Tooltip("Diâmetro da bolinha em metros")]
    public float tamanho = 0.03f;
    [Tooltip("Marcado: a bolinha cresce com a distância, para continuar visível de longe")]
    public bool crescerComDistancia = true;
    [Tooltip("Quanto a bolinha cresce por metro de distância (se a opção acima estiver marcada)")]
    public float crescimentoPorMetro = 0.01f;

    // Uma mão (interactor) e a bolinha dela
    class Mao
    {
        public NearFarInteractor interactor;
        public Transform bolinha;
    }

    readonly List<Mao> maos = new List<Mao>();

    void Start()
    {
        foreach (var interactor in GetComponentsInChildren<NearFarInteractor>(true))
        {
            GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esfera.name = "Ponto do laser " + interactor.transform.parent.name;
            Destroy(esfera.GetComponent<Collider>()); // não pode atrapalhar o próprio raio
            var r = esfera.GetComponent<MeshRenderer>();
            if (material != null) r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            esfera.SetActive(false);

            maos.Add(new Mao { interactor = interactor, bolinha = esfera.transform });
        }
    }

    void LateUpdate()
    {
        foreach (var mao in maos)
            Atualizar(mao);
    }

    void Atualizar(Mao mao)
    {
        bool mostrar = false;
        Vector3 ponto = Vector3.zero;

        // Só mostra se a mão está ligada e não está segurando nada
        if (mao.interactor != null && mao.interactor.isActiveAndEnabled && !mao.interactor.hasSelection)
        {
            // ValidCastHit = o raio está num objeto que dá para pegar (abacaxi ou peso)
            EndPointType tipo = mao.interactor.TryGetCurveEndPoint(out ponto, false, false);
            mostrar = tipo == EndPointType.ValidCastHit;

            // UI = o raio está num painel. Só mostra se for um botão que dá para apertar
            // (no fundo do painel ou num texto solto, a bolinha não aparece)
            if (tipo == EndPointType.UI && mostrarNosBotoes)
                mostrar = EstaNumBotao(mao.interactor);
        }

        if (mao.bolinha.gameObject.activeSelf != mostrar)
            mao.bolinha.gameObject.SetActive(mostrar);
        if (!mostrar) return;

        float diametro = tamanho;
        if (crescerComDistancia)
            diametro += Vector3.Distance(mao.interactor.transform.position, ponto) * crescimentoPorMetro;

        mao.bolinha.position = ponto;
        mao.bolinha.localScale = Vector3.one * diametro;
    }

    // Diz se o raio desta mão está em cima de um botão (Button, Toggle, Slider...) que está ativo
    bool EstaNumBotao(NearFarInteractor interactor)
    {
        if (!interactor.TryGetCurrentUIRaycastResult(out RaycastResult resultado)) return false;
        if (resultado.gameObject == null) return false;

        // O raio costuma bater no texto ou na imagem de dentro do botão, por isso procura nos pais
        Selectable botao = resultado.gameObject.GetComponentInParent<Selectable>();
        return botao != null && botao.IsInteractable();
    }
}
