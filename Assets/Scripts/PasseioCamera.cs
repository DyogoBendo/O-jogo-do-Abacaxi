using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Coloque este script numa câmera usada só para o vídeo (ex.: "CameraVideo").
// A câmera passa suavemente pelos pontos da lista, chegando em cada um
// no tempo indicado (em segundos), e toca a narração ao começar.
// Quando o passeio e a narração terminam, carrega a cena do jogo (se houver uma configurada).
public class PasseioCamera : MonoBehaviour
{
    [Header("Depois do vídeo")]
    [Tooltip("Cena carregada quando o vídeo termina. Deixe vazio para não trocar de cena (ex.: na gravação)")]
    public string cenaAoTerminar = "Feira";

    [Tooltip("Segundos de espera depois que o vídeo termina, antes de trocar de cena")]
    public float esperaNoFinal = 1f;

    [Tooltip("Se marcado, o jogador pode pular o vídeo com Espaço, Enter ou clique")]
    public bool permitirPular = true;

    bool trocandoDeCena;

    [System.Serializable]
    public class Parada
    {
        [Tooltip("Objeto vazio que marca a posição e a direção da câmera")]
        public Transform ponto;
        [Tooltip("Segundo do vídeo em que a câmera chega neste ponto")]
        public float tempo;
    }

    [Tooltip("Pontos do passeio, em ordem. O primeiro normalmente tem tempo 0")]
    public Parada[] paradas;

    [Tooltip("Áudio com a narração completa (opcional)")]
    public AudioSource narracao;

    float inicio;

    void Start()
    {
        inicio = Time.time;
        if (narracao != null) narracao.Play();
        if (paradas.Length > 0) IrPara(paradas[0].ponto, paradas[0].ponto, 0f);
    }

    void Update()
    {
        if (trocandoDeCena || string.IsNullOrEmpty(cenaAoTerminar)) return;

        float t = Time.time - inicio;

        // Pular o vídeo (só depois de meio segundo, para o clique do menu não pular sem querer)
        if (permitirPular && t > 0.5f && JogadorQuerPular())
        {
            IrParaOJogo();
            return;
        }

        // O vídeo termina quando a câmera chegou no último ponto
        // E a narração acabou de tocar (o que demorar mais)
        bool cameraTerminou = paradas == null || paradas.Length == 0
                              || t >= paradas[paradas.Length - 1].tempo;
        bool narracaoTerminou = narracao == null || !narracao.isPlaying;

        if (cameraTerminou && narracaoTerminou)
        {
            trocandoDeCena = true;
            Invoke(nameof(IrParaOJogo), esperaNoFinal);
        }
    }

    bool JogadorQuerPular()
    {
        var teclado = Keyboard.current;
        var mouse = Mouse.current;
        return (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame))
            || (mouse != null && mouse.leftButton.wasPressedThisFrame);
    }

    void IrParaOJogo()
    {
        trocandoDeCena = true;
        SceneManager.LoadScene(cenaAoTerminar);
    }

    void LateUpdate()
    {
        if (paradas == null || paradas.Length == 0) return;

        float t = Time.time - inicio;

        // Encontra o trecho atual: entre a parada i e a parada i + 1
        for (int i = 0; i < paradas.Length - 1; i++)
        {
            Parada a = paradas[i];
            Parada b = paradas[i + 1];
            if (t <= b.tempo)
            {
                float duracao = Mathf.Max(b.tempo - a.tempo, 0.01f);
                float progresso = Mathf.Clamp01((t - a.tempo) / duracao);
                IrPara(a.ponto, b.ponto, Mathf.SmoothStep(0f, 1f, progresso));
                return;
            }
        }

        // Depois do último tempo, fica parada no último ponto
        Parada ultima = paradas[paradas.Length - 1];
        IrPara(ultima.ponto, ultima.ponto, 1f);
    }

    void IrPara(Transform de, Transform para, float progresso)
    {
        transform.position = Vector3.Lerp(de.position, para.position, progresso);
        transform.rotation = Quaternion.Slerp(de.rotation, para.rotation, progresso);
    }
}
