using UnityEngine;
using UnityEngine.InputSystem;

// Coloque este script no PlayerCapsule.
// O jogador mira num abacaxi (centro da tela), aperta E para pegar,
// e aperta E de novo para soltar. Mirando num botão (ex.: "Verificar"),
// o E aciona o botão.
public class PegarObjeto : MonoBehaviour
{
    [Tooltip("Distância máxima para alcançar um abacaxi")]
    public float alcance = 3f;
    [Tooltip("Distância à frente da câmera em que o abacaxi fica segurado")]
    public float distanciaSegurar = 1.5f;
    [Tooltip("Rapidez com que o abacaxi acompanha a câmera")]
    public float forca = 15f;
    [Tooltip("Tecla para pegar e soltar")]
    public Key tecla = Key.E;
    [Tooltip("Distância máxima para acionar um botão com a mira")]
    public float alcanceBotao = 6f;

    Camera cam;
    Rigidbody segurado;
    Collider colliderSegurado;
    Collider colliderJogador;
    RigidbodyConstraints restricoesOriginais;

    void Start()
    {
        cam = Camera.main;
        colliderJogador = GetComponent<Collider>(); // o CharacterController do jogador
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[tecla].wasPressedThisFrame)
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

        Abacaxi abacaxi = hit.collider.GetComponentInParent<Abacaxi>();
        Rigidbody rb = hit.rigidbody;
        if (abacaxi == null)
        {
            Debug.Log("PegarObjeto: acertou '" + hit.collider.name + "', que não tem o script Abacaxi");
            return;
        }
        if (rb == null)
        {
            Debug.Log("PegarObjeto: acertou '" + hit.collider.name + "', mas ele não tem Rigidbody");
            return;
        }
        Debug.Log("PegarObjeto: pegou '" + abacaxi.name + "'");

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