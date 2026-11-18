using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Ajudante (não vai em nenhum objeto): diz se um abacaxi ou peso está na mão do jogador.
// Usado pela balança (não conta o peso de quem está na mão) e pela barraca
// (não confere enquanto o jogador ainda segura o abacaxi).
public static class ObjetoNaMao
{
    public static bool Esta(Component objeto)
    {
        if (objeto == null) return false;

        // VR: o objeto tem XR Grab Interactable e algum controle está segurando
        XRGrabInteractable pegavel = objeto.GetComponentInParent<XRGrabInteractable>();
        if (pegavel != null && pegavel.isSelected) return true;

        // Desktop (PegarObjeto): ele desliga a gravidade de quem está segurando
        Rigidbody corpo = objeto.GetComponentInParent<Rigidbody>();
        return corpo != null && !corpo.isKinematic && !corpo.useGravity;
    }
}
