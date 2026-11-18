using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

// Ajudante (não vai em nenhum objeto): funções para o jogador de VR (XR Origin).
// Usado pelo GameManager e pela TelaFinal.
public static class JogadorVR
{
    // Travar = o jogador para de andar e girar com os analógicos
    // (as mãos continuam funcionando, para clicar nos botões).
    public static void Travar(GameObject jogador, bool travar)
    {
        if (jogador == null) return;
        foreach (var provedor in jogador.GetComponentsInChildren<LocomotionProvider>(true))
            provedor.enabled = !travar;
    }

    // A câmera (cabeça) do jogador VR, ou null se não for um jogador VR
    public static Transform Cabeca(GameObject jogador)
    {
        if (jogador == null) return null;
        var origem = jogador.GetComponent<Unity.XR.CoreUtils.XROrigin>();
        return origem != null && origem.Camera != null ? origem.Camera.transform : null;
    }

    // Coloca um painel (Canvas em World Space) na frente do jogador,
    // na altura dos olhos e virado para ele
    public static void ColocarNaFrente(Transform painel, Transform cabeca, float distancia)
    {
        if (painel == null || cabeca == null) return;
        Vector3 frente = cabeca.forward; frente.y = 0f;
        if (frente.sqrMagnitude < 0.001f) frente = Vector3.forward;
        frente.Normalize();
        painel.position = cabeca.position + frente * distancia;
        painel.rotation = Quaternion.LookRotation(frente); // o Canvas é lido de trás para frente
    }
}
