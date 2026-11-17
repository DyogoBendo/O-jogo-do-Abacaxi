using UnityEngine;
using TMPro;

// Coloque este script num texto TextMeshPro (3D) que fica na frente de um objeto redondo.
// Ele dobra o texto em volta do corpo, para parecer pintado nele (ex.: o número dos pesos).
// O texto precisa estar virado para fora, a "raio" de distância do eixo do objeto,
// com o alinhamento centralizado.
[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TextoCurvo : MonoBehaviour
{
    [Tooltip("Raio do corpo onde o texto fica, nas unidades do objeto pai")]
    public float raio = 0.505f;

    TMP_Text texto;
    bool curvando;

    void OnEnable()
    {
        texto = GetComponent<TMP_Text>();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(AoMudarTexto);
        Curvar(true);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(AoMudarTexto);
        if (texto != null) texto.ForceMeshUpdate(); // volta ao texto reto
    }

    void OnValidate()
    {
        if (texto != null && isActiveAndEnabled) Curvar(true);
    }

    // O TextMeshPro refaz a malha sempre que o texto muda; aí é preciso dobrar de novo
    void AoMudarTexto(Object obj)
    {
        if (obj == texto) Curvar(false);
    }

    void Curvar(bool refazerMalha)
    {
        if (curvando) return;
        curvando = true;

        if (refazerMalha) texto.ForceMeshUpdate();

        // raio convertido para as unidades do próprio texto
        float escala = Mathf.Abs(transform.localScale.x);
        float r = escala > 0f ? raio / escala : raio;

        TMP_TextInfo info = texto.textInfo;
        for (int c = 0; c < info.characterCount; c++)
        {
            TMP_CharacterInfo letra = info.characterInfo[c];
            if (!letra.isVisible) continue;

            Vector3[] vertices = info.meshInfo[letra.materialReferenceIndex].vertices;
            int primeiro = letra.vertexIndex;

            // Cada letra é um retângulo reto: dobrado no corpo, o meio dele afundaria.
            // Por isso a letra vai um pouco para fora, na medida da sua largura,
            // para o meio ficar exatamente na superfície.
            float meiaLargura = (vertices[primeiro + 2].x - vertices[primeiro].x) * 0.5f;
            float raioDaLetra = r / Mathf.Cos(Mathf.Clamp(meiaLargura / r, 0f, 1.2f));

            for (int i = primeiro; i < primeiro + 4; i++)
            {
                // A distância na horizontal vira um ângulo em volta do corpo;
                // o texto entra para trás (+z) acompanhando a curva
                Vector3 v = vertices[i];
                float angulo = v.x / r;
                vertices[i] = new Vector3(Mathf.Sin(angulo) * raioDaLetra, v.y, r - Mathf.Cos(angulo) * raioDaLetra);
            }
        }
        texto.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

        curvando = false;
    }
}
