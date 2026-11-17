using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Coloque este script no objeto principal do sino (ex.: TreeToy_bell).
// Quando o jogador mira no sino ou na cordinha e aperta E (ou clica),
// o sino toca, balança e a cordinha dá um puxão.
public class Sino : MonoBehaviour
{
    [Header("Peças")]
    [Tooltip("A parte que balança (ex.: Toy_bell)")]
    public Transform corpo;
    [Tooltip("A cordinha (ex.: Toy_rope), opcional")]
    public Transform cordinha;

    [Header("Som")]
    public AudioSource audioSource;
    public AudioClip somSino;

    [Header("Movimento")]
    [Tooltip("Quanto o sino balança, em graus")]
    public float anguloBalanco = 20f;
    [Tooltip("Quanto tempo o sino fica balançando, em segundos")]
    public float duracaoBalanco = 1.5f;
    [Tooltip("Quanto a cordinha desce ao ser puxada")]
    public float puxaoCordinha = 0.1f;
    [Tooltip("Tempo mínimo entre dois toques, em segundos")]
    public float intervaloMinimo = 0.5f;

    [Header("Ao tocar")]
    [Tooltip("Ações chamadas quando o sino toca (depois: conferir a barraca)")]
    public UnityEvent aoTocar;

    float ultimoToque = -10f;
    Quaternion rotacaoInicialCorpo;
    Vector3 posicaoInicialCordinha;

    void Start()
    {
        if (corpo != null) rotacaoInicialCorpo = corpo.localRotation;
        if (cordinha != null) posicaoInicialCordinha = cordinha.localPosition;
    }

    // Chamado pelo PegarObjeto quando o jogador aciona o sino
    public void Tocar()
    {
        if (Time.time - ultimoToque < intervaloMinimo) return;
        ultimoToque = Time.time;

        if (audioSource != null && somSino != null)
            audioSource.PlayOneShot(somSino);

        StopAllCoroutines();
        if (corpo != null) StartCoroutine(Balancar());
        if (cordinha != null) StartCoroutine(PuxarCordinha());

        aoTocar.Invoke();
    }

    IEnumerator Balancar()
    {
        for (float t = 0f; t < duracaoBalanco; t += Time.deltaTime)
        {
            // Balanço que vai diminuindo até parar
            float amortecimento = 1f - t / duracaoBalanco;
            float angulo = anguloBalanco * amortecimento * Mathf.Sin(t * 12f);
            corpo.localRotation = rotacaoInicialCorpo * Quaternion.Euler(angulo, 0f, 0f);
            yield return null;
        }
        corpo.localRotation = rotacaoInicialCorpo;
    }

    IEnumerator PuxarCordinha()
    {
        Vector3 embaixo = posicaoInicialCordinha + Vector3.down * puxaoCordinha;
        const float tempo = 0.15f;

        for (float t = 0f; t < 1f; t += Time.deltaTime / tempo)
        {
            cordinha.localPosition = Vector3.Lerp(posicaoInicialCordinha, embaixo, t);
            yield return null;
        }
        for (float t = 0f; t < 1f; t += Time.deltaTime / tempo)
        {
            cordinha.localPosition = Vector3.Lerp(embaixo, posicaoInicialCordinha, t);
            yield return null;
        }
        cordinha.localPosition = posicaoInicialCordinha;
    }
}
