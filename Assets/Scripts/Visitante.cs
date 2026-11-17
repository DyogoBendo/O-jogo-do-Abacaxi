using System.Collections.Generic;
using UnityEngine;

// Coloque este script em cada boneco visitante (o objeto precisa de um CharacterController
// e, num filho, do modelo com Animator). O boneco fica parado até a FilaDeVisitantes
// mandar ele entrar; aí segue o caminho recebido e depois passeia pela feira.
//
// O Animator precisa de um parâmetro bool "Andando" (parado <-> andando).
[RequireComponent(typeof(CharacterController))]
public class Visitante : MonoBehaviour
{
    [Tooltip("Velocidade da caminhada, em metros por segundo")]
    public float velocidade = 0.9f;
    [Tooltip("Velocidade em que a animação de andar foi feita (m/s). A animação é acelerada ou " +
             "desacelerada para acompanhar a velocidade real, assim os pés não deslizam no chão")]
    public float velocidadeDaAnimacao = 0.55f;
    [Tooltip("Velocidade para virar, em graus por segundo")]
    public float velocidadeGiro = 300f;
    [Tooltip("Segundos parado em cada ponto antes de ir para outro (mínimo e máximo)")]
    public Vector2 tempoParado = new Vector2(3f, 8f);
    [Tooltip("Depois de entrar, continua andando entre os pontos da feira")]
    public bool passear = true;

    static readonly int ParametroAndando = Animator.StringToHash("Andando");

    CharacterController controle;
    Animator animator;

    readonly List<Vector3> caminho = new List<Vector3>();
    Vector3[] pontosDePasseio;
    bool liberado;
    float esperarAte;
    float velocidadeQueda;

    // Para perceber quando está trombando em algo (ex.: o jogador)
    float tempoTravado;

    public bool Liberado => liberado;

    void Awake()
    {
        controle = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Cada boneco começa a animação num momento diferente, para não ficarem sincronizados
        if (animator != null)
            animator.Play(0, 0, Random.value);
    }

    // Chamado pela FilaDeVisitantes: o caminho até dentro da feira e os pontos para passear depois
    public void Entrar(IEnumerable<Vector3> rota, Vector3[] pontosNaFeira)
    {
        caminho.Clear();
        caminho.AddRange(rota);
        pontosDePasseio = pontosNaFeira;
        liberado = true;
        esperarAte = 0f;
    }

    void Update()
    {
        bool andando = false;
        Vector3 movimento = Vector3.zero;

        if (liberado && Time.time >= esperarAte)
        {
            if (caminho.Count == 0 && passear && pontosDePasseio != null && pontosDePasseio.Length > 0)
            {
                Vector3 ponto = pontosDePasseio[Random.Range(0, pontosDePasseio.Length)];
                ponto += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                caminho.Add(ponto);
            }

            if (caminho.Count > 0)
            {
                Vector3 direcao = caminho[0] - transform.position;
                direcao.y = 0f;

                if (direcao.magnitude < 0.35f)
                {
                    caminho.RemoveAt(0);
                    tempoTravado = 0f;
                    // Chegou no fim do caminho: fica parado um pouco
                    if (caminho.Count == 0)
                        esperarAte = Time.time + Random.Range(tempoParado.x, tempoParado.y);
                }
                else
                {
                    andando = true;
                    Quaternion alvo = Quaternion.LookRotation(direcao);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, alvo, velocidadeGiro * Time.deltaTime);

                    // Só anda para a frente quando está mais ou menos virado para o destino
                    float alinhamento = Mathf.Clamp01(Vector3.Dot(transform.forward, direcao.normalized));
                    movimento = transform.forward * velocidade * alinhamento;
                }
            }
        }

        // Gravidade, para ficar sempre no chão
        if (controle.isGrounded) velocidadeQueda = -1f;
        else velocidadeQueda -= 9.81f * Time.deltaTime;

        Vector3 antes = transform.position;
        controle.Move((movimento + Vector3.up * velocidadeQueda) * Time.deltaTime);

        if (andando) VerificarSeTravou(antes, movimento);
        if (animator != null)
        {
            animator.SetBool(ParametroAndando, andando);
            // Passos no ritmo do deslocamento (senão parece que o boneco está deslizando)
            animator.speed = andando && velocidadeDaAnimacao > 0f
                ? Mathf.Clamp(movimento.magnitude / velocidadeDaAnimacao, 0.6f, 3f)
                : 1f;
        }
    }

    // Se quase não saiu do lugar por um tempo, está trombando em algo: dá um passo para o lado
    void VerificarSeTravou(Vector3 antes, Vector3 movimento)
    {
        Vector3 andou = transform.position - antes;
        andou.y = 0f;
        float esperado = movimento.magnitude * Time.deltaTime;

        if (esperado > 0.001f && andou.magnitude < esperado * 0.3f) tempoTravado += Time.deltaTime;
        else tempoTravado = 0f;

        if (tempoTravado > 0.8f)
        {
            tempoTravado = 0f;
            float lado = Random.value < 0.5f ? -1f : 1f;
            Vector3 desvio = transform.position + transform.right * lado * 1.2f - transform.forward * 0.3f;
            caminho.Insert(0, desvio);
        }
    }
}
