using UnityEngine;
using TMPro;

// Coloque este script em cada abacaxi.
// Ele guarda o peso (usado pela balança e pela barraca).
// A etiqueta 3D com o peso é opcional: no jogo ela fica escondida (mostrarEtiqueta desmarcado),
// e o jogador descobre o peso usando a balança.
// [ExecuteAlways] faz a etiqueta se posicionar também no editor, fora do Play.
[ExecuteAlways]
public class Abacaxi : MonoBehaviour
{
    [Tooltip("Peso do abacaxi em kg")]
    public int peso = 1;

    [Tooltip("Texto 3D (TextMeshPro) que mostra o peso")]
    public TMP_Text etiqueta;

    [Tooltip("Desmarcado = o peso fica escondido e o jogador precisa usar a balança")]
    public bool mostrarEtiqueta = false;

    [Tooltip("Altura da etiqueta acima do centro do abacaxi")]
    public float alturaEtiqueta = 0.5f;

    // Fica verdadeiro quando o abacaxi é entregue numa barraca concluída (não pode mais ser pego)
    [HideInInspector] public bool travado;

    Renderer modelo;

    void Start()
    {
        AtualizarEtiqueta();
    }

    // Atualiza o texto no editor sempre que o peso é alterado no Inspector
    void OnValidate()
    {
        AtualizarEtiqueta();
    }

    void AtualizarEtiqueta()
    {
        if (etiqueta == null) return;

        etiqueta.text = peso + " kg";
    }

    // Centro visual da fruta (funciona mesmo se a origem do modelo estiver deslocada)
    Vector3 CentroDoAbacaxi()
    {
        if (modelo == null)
        {
            // Procura o modelo da fruta, ignorando o renderer da própria etiqueta
            foreach (var r in GetComponentsInChildren<MeshRenderer>())
            {
                if (etiqueta != null && r.transform.IsChildOf(etiqueta.transform))
                    continue;
                modelo = r;
                break;
            }
        }

        return modelo != null ? modelo.bounds.center : transform.position;
    }

    // Mantém a etiqueta acima do abacaxi e virada para a câmera,
    // mesmo se o abacaxi rolar ou for carregado
    void LateUpdate()
    {
        if (etiqueta == null) return;

        // Liga ou desliga a etiqueta conforme a opção do Inspector.
        // O peso continua guardado no script: a balança e a barraca ainda o usam.
        if (etiqueta.gameObject.activeSelf != mostrarEtiqueta)
            etiqueta.gameObject.SetActive(mostrarEtiqueta);

        if (!mostrarEtiqueta) return;

        Vector3 posicao = CentroDoAbacaxi() + Vector3.up * alturaEtiqueta;
        if ((etiqueta.transform.position - posicao).sqrMagnitude > 0.000001f)
            etiqueta.transform.position = posicao;

        Camera cam = Camera.main;
        if (cam != null)
            etiqueta.transform.rotation = Quaternion.LookRotation(
                etiqueta.transform.position - cam.transform.position);
    }
}
