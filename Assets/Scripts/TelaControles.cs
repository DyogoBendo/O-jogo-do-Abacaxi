using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

// Coloque este script num objeto da cena da feira (ex.: o Game Manager).
// Abre e fecha uma tela com os controles do jogo, sem sair da fase:
//   - VR: botão Y do controle esquerdo;
//   - PC: tecla F1.
// Enquanto a tela está aberta, o jogador não anda nem gira com os analógicos
// (mas continua olhando em volta com a cabeça e pode clicar no botão "Fechar").
public class TelaControles : MonoBehaviour
{
    [Tooltip("O Canvas (World Space) com a tela de controles. Ele começa escondido")]
    public GameObject painel;

    [Tooltip("O jogador VR (JogadorVR). Usado para pôr a tela na frente dele e travar os analógicos")]
    public GameObject jogador;

    [Tooltip("Distância, em metros, entre os olhos do jogador e a tela")]
    public float distancia = 1.5f;

    [Tooltip("Marcado: o jogador não anda nem gira enquanto a tela está aberta")]
    public bool travarJogador = true;

    [Header("Aviso \"Y - TELA DE CONTROLES\"")]
    [Tooltip("Canvas pequeno (World Space) com o aviso. Fica preso à câmera e só aparece com a tela fechada")]
    public GameObject dica;
    [Tooltip("Onde o aviso fica, em metros, em relação aos olhos: X = lado, Y = altura (negativo = embaixo), Z = distância")]
    public Vector3 posicaoDica = new Vector3(0f, -0.3f, 1f);

    InputAction botaoAbrir;

    // Os modos de andar/girar que estavam ligados quando a tela abriu.
    // Ao fechar, só esses voltam (teleporte, pulo etc. continuam desligados).
    readonly List<LocomotionProvider> travados = new List<LocomotionProvider>();

    public bool Aberta => painel != null && painel.activeSelf;

    void OnEnable()
    {
        // Botão Y = "secondary button" do controle esquerdo do Quest
        botaoAbrir = new InputAction("Tela de controles", InputActionType.Button);
        botaoAbrir.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");
        botaoAbrir.AddBinding("<XRController>{LeftHand}/secondaryButton");
        botaoAbrir.AddBinding("<Keyboard>/f1"); // PC
        botaoAbrir.Enable();
    }

    void OnDisable()
    {
        if (botaoAbrir != null) { botaoAbrir.Disable(); botaoAbrir.Dispose(); botaoAbrir = null; }
    }

    void Start()
    {
        if (painel != null) painel.SetActive(false);

        // Prende o aviso na câmera (cabeça): ele acompanha o olhar e fica sempre visível.
        // É feito só no Play, então o prefab do JogadorVR não é alterado.
        if (dica != null)
        {
            Transform cabeca = JogadorVR.Cabeca(jogador);
            if (cabeca == null && Camera.main != null) cabeca = Camera.main.transform;
            if (cabeca != null)
            {
                dica.transform.SetParent(cabeca, false);
                dica.transform.localPosition = posicaoDica;
                dica.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void Update()
    {
        // O aviso aparece só com a tela fechada e enquanto o jogo não terminou
        if (dica != null)
        {
            bool fimDeJogo = GameManager.instancia != null && GameManager.instancia.FeiraAberta;
            bool mostrar = !Aberta && !fimDeJogo;
            if (dica.activeSelf != mostrar) dica.SetActive(mostrar);
        }

        // Se a feira abrir com a tela aberta, ela sai da frente da tela de parabéns
        if (Aberta && GameManager.instancia != null && GameManager.instancia.FeiraAberta)
        {
            Fechar();
            return;
        }

        if (botaoAbrir != null && botaoAbrir.WasPressedThisFrame())
        {
            if (Aberta) Fechar();
            else Abrir();
        }
    }

    public void Abrir()
    {
        if (painel == null) return;

        // Depois que a feira abre, quem aparece é a tela de parabéns
        if (GameManager.instancia != null && GameManager.instancia.FeiraAberta) return;

        // Põe a tela na frente de onde o jogador está olhando
        Transform cabeca = JogadorVR.Cabeca(jogador);
        if (cabeca == null && Camera.main != null) cabeca = Camera.main.transform;
        JogadorVR.ColocarNaFrente(painel.transform, cabeca, distancia);

        painel.SetActive(true);
        if (travarJogador) TravarLocomocao();
    }

    // Ligado ao botão "Fechar" da tela
    public void Fechar()
    {
        if (painel == null) return;
        painel.SetActive(false);

        // Só destrava se a feira ainda não abriu (no fim do jogo o jogador fica parado)
        bool fimDeJogo = GameManager.instancia != null && GameManager.instancia.FeiraAberta;
        if (!fimDeJogo) DestravarLocomocao();
        travados.Clear();
    }

    // Desliga só o que estava ligado (andar e girar) e guarda a lista
    void TravarLocomocao()
    {
        travados.Clear();
        if (jogador == null) return;
        foreach (var provedor in jogador.GetComponentsInChildren<LocomotionProvider>(true))
        {
            if (!provedor.enabled) continue;
            provedor.enabled = false;
            travados.Add(provedor);
        }
    }

    // Religa exatamente o que foi desligado ao abrir a tela
    void DestravarLocomocao()
    {
        foreach (var provedor in travados)
            if (provedor != null) provedor.enabled = true;
    }
}
