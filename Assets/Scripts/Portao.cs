using System.Collections;
using UnityEngine;

// Coloque este script na própria porta.
// Ela começa onde estiver na cena (fechada) e, ao abrir, vai suavemente
// até a posição e rotação "abertas" gravadas abaixo.
//
// Para gravar a posição aberta:
//   1. Mova e gire a porta até onde ela deve ficar aberta.
//   2. Clique nos três pontinhos deste componente > "Gravar posição atual como aberta".
//   3. Aperte Ctrl + Z até a porta voltar para a posição fechada.
public class Portao : MonoBehaviour
{
    [Tooltip("Posição da porta aberta (mesmos valores do Position no Inspector)")]
    public Vector3 posicaoAberta;
    [Tooltip("Rotação da porta aberta (mesmos valores do Rotation no Inspector)")]
    public Vector3 rotacaoAberta;
    [Tooltip("Tempo da animação, em segundos")]
    public float duracao = 2f;

    bool aberto;

    public void Abrir()
    {
        if (aberto) return;
        aberto = true;
        StartCoroutine(Animar());
    }

    IEnumerator Animar()
    {
        Vector3 posInicio = transform.localPosition;
        Quaternion rotInicio = transform.localRotation;
        Quaternion rotFim = Quaternion.Euler(rotacaoAberta);

        for (float t = 0f; t < 1f; t += Time.deltaTime / duracao)
        {
            float suave = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(posInicio, posicaoAberta, suave);
            transform.localRotation = Quaternion.Slerp(rotInicio, rotFim, suave);
            yield return null;
        }

        transform.localPosition = posicaoAberta;
        transform.localRotation = rotFim;
    }

    [ContextMenu("Gravar posição atual como aberta")]
    void GravarPosicaoAberta()
    {
        posicaoAberta = transform.localPosition;
        rotacaoAberta = transform.localEulerAngles;
#if UNITY_EDITOR
        // Salva os valores sem entrar no Ctrl + Z, para o Ctrl + Z desfazer só o movimento da porta
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log("Portao: posição aberta gravada. Agora use Ctrl + Z para voltar a porta à posição fechada.");
    }

    [ContextMenu("Testar abertura (só em Play)")]
    void TestarAbertura()
    {
        if (Application.isPlaying) Abrir();
        else Debug.Log("Portao: aperte Play antes de testar a abertura");
    }
}