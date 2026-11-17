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
        // Só abre com o jogo rodando (vencendo a fase); fora do Play a porta fica fechada
        if (!Application.isPlaying || aberto) return;
        aberto = true;
        StartCoroutine(Animar());
    }

    IEnumerator Animar()
    {
        Vector3 posInicio = transform.localPosition;
        Quaternion rotInicio = transform.localRotation;
        Quaternion rotFim = Quaternion.Euler(rotacaoAberta);

        // Ângulo total do giro em torno do eixo vertical (Y)
        float angulo = Mathf.DeltaAngle(transform.localEulerAngles.y, rotacaoAberta.y);

        if (TentarCalcularDobradica(posInicio, posicaoAberta, angulo, out Vector3 dobradica))
        {
            // Gira a porta em torno da dobradiça (eixo vertical que passa por ela)
            for (float t = 0f; t < 1f; t += Time.deltaTime / duracao)
            {
                float suave = Mathf.SmoothStep(0f, 1f, t);
                Quaternion giro = Quaternion.Euler(0f, angulo * suave, 0f);

                Vector3 pos = dobradica + giro * (posInicio - dobradica);
                pos.y = Mathf.Lerp(posInicio.y, posicaoAberta.y, suave);

                transform.localPosition = pos;
                transform.localRotation = giro * rotInicio;
                yield return null;
            }
        }
        else
        {
            // Sem giro (só deslizar): movimento em linha reta
            for (float t = 0f; t < 1f; t += Time.deltaTime / duracao)
            {
                float suave = Mathf.SmoothStep(0f, 1f, t);
                transform.localPosition = Vector3.Lerp(posInicio, posicaoAberta, suave);
                transform.localRotation = Quaternion.Slerp(rotInicio, rotFim, suave);
                yield return null;
            }
        }

        transform.localPosition = posicaoAberta;
        transform.localRotation = rotFim;
    }

    // Acha o ponto (no plano XZ) em torno do qual girar "angulo" graus leva
    // a porta da posição fechada até a aberta. Esse ponto é a dobradiça.
    static bool TentarCalcularDobradica(Vector3 fechada, Vector3 aberta, float angulo, out Vector3 dobradica)
    {
        dobradica = Vector3.zero;
        if (Mathf.Abs(angulo) < 1f) return false;

        float rad = angulo * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);

        // Giro em Y no Unity: x' = x*c + z*s ; z' = -x*s + z*c
        // Resolve (I - R) * P = aberta - R * fechada
        float rx = aberta.x - (fechada.x * c + fechada.z * s);
        float rz = aberta.z - (-fechada.x * s + fechada.z * c);
        float det = 2f - 2f * c;

        dobradica.x = ((1f - c) * rx + s * rz) / det;
        dobradica.z = (-s * rx + (1f - c) * rz) / det;
        return true;
    }

    // Mostra a dobradiça calculada (linha amarela) quando a porta está selecionada
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && aberto) return;
        float angulo = Mathf.DeltaAngle(transform.localEulerAngles.y, rotacaoAberta.y);
        if (!TentarCalcularDobradica(transform.localPosition, posicaoAberta, angulo, out Vector3 dobradica)) return;

        dobradica.y = transform.localPosition.y;
        Vector3 mundo = transform.parent != null ? transform.parent.TransformPoint(dobradica) : dobradica;
        float altura = transform.lossyScale.y * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(mundo - Vector3.up * altura, mundo + Vector3.up * altura);
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