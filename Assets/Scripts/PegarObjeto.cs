using UnityEngine;
using UnityEngine.InputSystem;

// Coloque este script no PlayerCapsule.
// O jogador mira num abacaxi ou peso (centro da tela), aperta E para pegar,
// e aperta E de novo para soltar (o clique esquerdo do mouse faz o mesmo). Mirando num botão (ex.: "Verificar"),
// o E aciona o botão. Mirando num sino, o E toca o sino.
public class PegarObjeto : MonoBehaviour
{
    [Tooltip("Distância máxima para alcançar um abacaxi ou peso")]
    public float alcance = 3f;
    [Tooltip("Distância à frente da câmera em que o abacaxi fica segurado")]
    public float distanciaSegurar = 1.5f;
    [Tooltip("Rapidez com que o abacaxi acompanha a câmera")]
    public float forca = 15f;
    [Tooltip("Tecla para pegar e soltar")]
    public Key tecla = Key.E;
    [Tooltip("Marcado: o clique esquerdo do mouse também pega, solta e aperta botões")]
    public bool usarCliqueDoMouse = true;
    [Tooltip("Distância máxima para acionar um botão com a mira")]
    public float alcanceBotao = 6f;

    [Header("Sombra embaixo do objeto segurado")]
    [Tooltip("Material da sombra (Assets/Materiais/SombraAbacaxi). Vazio = sem sombra")]
    public Material materialSombra;
    [Tooltip("Tamanho da sombra em relação à largura do objeto")]
    public float tamanhoSombra = 1.3f;
    [Tooltip("Até que distância para baixo a sombra aparece")]
    public float alcanceSombra = 5f;

    Camera cam;
    Transform sombra;
    Rigidbody segurado;
    Collider colliderSegurado;
    Collider colliderJogador;
    RigidbodyConstraints restricoesOriginais;

    void Start()
    {
        cam = Camera.main;
        colliderJogador = GetComponent<Collider>(); // o CharacterController do jogador
        CriarSombra();
    }

    // Um quadrado com a textura de círculo escuro, sem collider
    void CriarSombra()
    {
        if (materialSombra == null) return;
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Sombra do objeto segurado";
        Destroy(quad.GetComponent<Collider>());
        Renderer r = quad.GetComponent<Renderer>();
        r.sharedMaterial = materialSombra;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        sombra = quad.transform;
        sombra.gameObject.SetActive(false);
    }

    // Coloca a sombra no que estiver logo abaixo do objeto segurado (bancada, chão, balança...)
    void LateUpdate()
    {
        if (sombra == null) return;
        if (segurado == null)
        {
            if (sombra.gameObject.activeSelf) sombra.gameObject.SetActive(false);
            return;
        }

        // Usa a posição interpolada (a mesma que aparece na tela) para a sombra não tremer
        Vector3 origem = segurado.transform.TransformPoint(segurado.centerOfMass);
        RaycastHit[] acertos = Physics.RaycastAll(origem, Vector3.down, alcanceSombra, ~0, QueryTriggerInteraction.Ignore);
        float maisPerto = float.MaxValue;
        RaycastHit chao = default;
        foreach (var acerto in acertos)
        {
            // Ignora o próprio objeto segurado e o jogador
            if (acerto.rigidbody == segurado || acerto.collider == colliderJogador) continue;
            if (acerto.distance < maisPerto) { maisPerto = acerto.distance; chao = acerto; }
        }

        if (maisPerto == float.MaxValue)
        {
            sombra.gameObject.SetActive(false);
            return;
        }

        // Largura do objeto (maior entre X e Z) define o tamanho da sombra
        Bounds b = colliderSegurado.bounds;
        foreach (var c in segurado.GetComponentsInChildren<Collider>())
            if (!c.isTrigger) b.Encapsulate(c.bounds);
        float largura = Mathf.Max(b.size.x, b.size.z) * tamanhoSombra;

        sombra.gameObject.SetActive(true);
        sombra.position = chao.point + chao.normal * 0.01f; // um pouquinho acima para não piscar
        sombra.rotation = Quaternion.LookRotation(-chao.normal);
        sombra.localScale = new Vector3(largura, largura, 1f);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Tecla E ou clique esquerdo do mouse fazem a mesma ação
        bool apertouTecla = Keyboard.current[tecla].wasPressedThisFrame;
        bool clicouMouse = usarCliqueDoMouse && Mouse.current != null
                           && Mouse.current.leftButton.wasPressedThisFrame;

        if (apertouTecla || clicouMouse)
        {
            if (segurado != null) Soltar();
            else if (!TentarClicarBotao()) TentarPegar(); // botão na mira? senão, tenta pegar
        }
    }

    // Retorna true se havia um botão (ex.: "Verificar") na mira e ele foi acionado
    bool TentarClicarBotao()
    {
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] acertos = Physics.RaycastAll(raio, alcanceBotao, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(acertos, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var acerto in acertos)
        {
            BotaoMira botao = acerto.collider.GetComponent<BotaoMira>();
            if (botao != null)
            {
                Debug.Log("PegarObjeto: acionou o botão '" + botao.name + "'");
                botao.Clicar();
                return true;
            }

            // Sino: mirar no sino ou na cordinha toca o sino
            Sino sino = acerto.collider.GetComponentInParent<Sino>();
            if (sino != null)
            {
                Debug.Log("PegarObjeto: tocou o sino '" + sino.name + "'");
                sino.Tocar();
                return true;
            }
            // Para no primeiro objeto sólido (uma parede bloqueia o clique)
            if (!acerto.collider.isTrigger)
            {
                Debug.Log("PegarObjeto: nenhum botão na mira (o raio bateu em '" + acerto.collider.name + "')");
                return false;
            }
        }
        Debug.Log("PegarObjeto: nenhum botão na mira (alcance de " + alcanceBotao + " m)");
        return false;
    }

    void FixedUpdate()
    {
        if (segurado == null) return;

        // Puxa o abacaxi para um ponto à frente da câmera.
        // Usa o centro de massa (centro da fruta), não a origem do modelo.
        Vector3 alvo = cam.transform.position + cam.transform.forward * distanciaSegurar;
        segurado.linearVelocity = (alvo - segurado.worldCenterOfMass) * forca;
    }

    void TentarPegar()
    {
        // Raio a partir do centro da tela, ignorando triggers (como o sensor do prato)
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(raio, out RaycastHit hit, alcance, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("PegarObjeto: o raio não acertou nada (alcance de " + alcance + " m)");
            return;
        }

        // Pode pegar abacaxis e pesos de ferro
        MonoBehaviour objeto = hit.collider.GetComponentInParent<Abacaxi>();
        if (objeto == null) objeto = hit.collider.GetComponentInParent<PesoFerro>();
        Rigidbody rb = hit.rigidbody;
        if (objeto == null)
        {
            Debug.Log("PegarObjeto: acertou '" + hit.collider.name + "', que não é abacaxi nem peso");
            return;
        }
        if (objeto is Abacaxi abacaxi && abacaxi.travado)
        {
            Debug.Log("PegarObjeto: '" + abacaxi.name + "' já foi entregue numa barraca concluída");
            return;
        }
        if (rb == null)
        {
            Debug.Log("PegarObjeto: acertou '" + hit.collider.name + "', mas ele não tem Rigidbody");
            return;
        }
        Debug.Log("PegarObjeto: pegou '" + objeto.name + "'");

        segurado = rb;
        colliderSegurado = hit.collider;
        segurado.useGravity = false;

        // Enquanto está na mão: endireita e trava toda a rotação
        restricoesOriginais = segurado.constraints;
        segurado.angularVelocity = Vector3.zero;
        segurado.rotation = Quaternion.Euler(0f, segurado.rotation.eulerAngles.y, 0f);
        segurado.constraints = RigidbodyConstraints.FreezeRotation;
        segurado.interpolation = RigidbodyInterpolation.Interpolate;

        // Evita que o abacaxi empurre o próprio jogador
        if (colliderJogador != null)
            Physics.IgnoreCollision(colliderSegurado, colliderJogador, true);
    }

    void Soltar()
    {
        segurado.useGravity = true;
        segurado.linearVelocity = Vector3.zero;
        segurado.angularVelocity = Vector3.zero;
        segurado.constraints = restricoesOriginais;

        if (colliderJogador != null)
            Physics.IgnoreCollision(colliderSegurado, colliderJogador, false);

        segurado = null;
        colliderSegurado = null;
    }
}
