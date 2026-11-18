using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Versão VR da sombra que o PegarObjeto fazia no desktop.
// Coloque este script no JogadorVR (o XR Origin).
// Para cada mão que está segurando algo, desenha uma sombra redonda
// no que estiver logo abaixo (balcão, chão, prato da balança...).
// Assim o jogador sabe se o abacaxi já está em cima do balcão antes de soltar.
public class SombraObjetoSegurado : MonoBehaviour
{
    [Tooltip("Material da sombra (Assets/Materiais/SombraAbacaxi)")]
    public Material materialSombra;
    [Tooltip("Tamanho da sombra em relação à largura do objeto")]
    public float tamanhoSombra = 1.3f;
    [Tooltip("Até que distância para baixo a sombra aparece")]
    public float alcanceSombra = 5f;

    // Uma mão (interactor) e a sombra dela
    class Mao
    {
        public NearFarInteractor interactor;
        public Transform sombra;
    }

    readonly List<Mao> maos = new List<Mao>();
    Collider[] collidersDoJogador;

    void Start()
    {
        if (materialSombra == null)
        {
            Debug.LogWarning("SombraObjetoSegurado: arraste o material SombraAbacaxi no Inspector.");
            enabled = false;
            return;
        }

        // Partes do próprio jogador (corpo, mãos) não recebem sombra
        collidersDoJogador = GetComponentsInChildren<Collider>(true);

        // Cria uma sombra para cada mão
        foreach (var interactor in GetComponentsInChildren<NearFarInteractor>(true))
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Sombra da mão " + interactor.transform.parent.name;
            Destroy(quad.GetComponent<Collider>()); // a sombra não pode ter colisão
            var r = quad.GetComponent<MeshRenderer>();
            r.sharedMaterial = materialSombra;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            quad.SetActive(false);

            maos.Add(new Mao { interactor = interactor, sombra = quad.transform });
        }
    }

    void LateUpdate()
    {
        foreach (var mao in maos)
            AtualizarSombra(mao);
    }

    void AtualizarSombra(Mao mao)
    {
        // A mão está segurando um objeto pegável?
        XRGrabInteractable segurado = null;
        if (mao.interactor != null && mao.interactor.hasSelection)
            segurado = mao.interactor.interactablesSelected[0] as XRGrabInteractable;

        if (segurado == null)
        {
            if (mao.sombra.gameObject.activeSelf) mao.sombra.gameObject.SetActive(false);
            return;
        }

        // Centro do objeto (o pivô do abacaxi é deslocado, então usamos o centro dos colliders)
        Bounds b = new Bounds(segurado.transform.position, Vector3.zero);
        bool primeiro = true;
        foreach (var c in segurado.colliders)
        {
            if (c == null || c.isTrigger) continue;
            if (primeiro) { b = c.bounds; primeiro = false; }
            else b.Encapsulate(c.bounds);
        }

        // Procura o que está logo abaixo, ignorando o próprio objeto e o jogador
        RaycastHit[] acertos = Physics.RaycastAll(b.center, Vector3.down, alcanceSombra, ~0, QueryTriggerInteraction.Ignore);
        bool achou = false;
        RaycastHit chao = default;
        foreach (var acerto in acertos)
        {
            if (segurado.colliders.Contains(acerto.collider)) continue;
            if (System.Array.IndexOf(collidersDoJogador, acerto.collider) >= 0) continue;
            if (!achou || acerto.distance < chao.distance) { chao = acerto; achou = true; }
        }

        if (!achou)
        {
            mao.sombra.gameObject.SetActive(false);
            return;
        }

        float largura = Mathf.Max(b.size.x, b.size.z) * tamanhoSombra;
        mao.sombra.gameObject.SetActive(true);
        mao.sombra.position = chao.point + chao.normal * 0.01f; // um pouquinho acima para não piscar
        mao.sombra.rotation = Quaternion.LookRotation(-chao.normal);
        mao.sombra.localScale = new Vector3(largura, largura, 1f);
    }
}
