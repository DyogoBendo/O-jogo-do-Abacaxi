using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Coloque este script nos objetos pegáveis em VR (abacaxis e pesos),
// junto com o XR Grab Interactable.
// Ele faz o objeto ficar sempre em pé: não tomba com o giro do controle
// e, ao ser solto, volta a ficar reto (só mantém o giro para os lados).
[RequireComponent(typeof(XRGrabInteractable))]
public class ManterEmPe : MonoBehaviour
{
    [Tooltip("Marcado: o objeto não gira junto com o controle enquanto está na mão")]
    public bool naoGirarNaMao = true;

    XRGrabInteractable pegavel;
    Rigidbody corpo;

    void Awake()
    {
        pegavel = GetComponent<XRGrabInteractable>();
        corpo = GetComponent<Rigidbody>();

        // Desligar "Track Rotation" = o objeto segue a posição da mão, mas não a inclinação
        if (naoGirarNaMao) pegavel.trackRotation = false;
    }

    void OnEnable()
    {
        pegavel.selectEntered.AddListener(AoPegar);
        pegavel.selectExited.AddListener(AoSoltar);
    }

    void OnDisable()
    {
        pegavel.selectEntered.RemoveListener(AoPegar);
        pegavel.selectExited.RemoveListener(AoSoltar);
    }

    // Travas de rotação originais do Rigidbody, devolvidas ao soltar
    RigidbodyConstraints travasOriginais;

    // Se o objeto estava caído quando foi pego, já fica em pé na mão.
    // Enquanto está na mão, trava toda a rotação: assim ele não fica girando
    // quando bate na balança ou no balcão.
    void AoPegar(SelectEnterEventArgs args)
    {
        Endireitar();
        if (corpo != null)
        {
            travasOriginais = corpo.constraints;
            corpo.constraints = travasOriginais | RigidbodyConstraints.FreezeRotation;
        }
    }

    // Ao soltar, garante que cai em pé e devolve as travas originais
    void AoSoltar(SelectExitEventArgs args)
    {
        if (corpo != null) corpo.constraints = travasOriginais;
        Endireitar();
    }

    void Endireitar()
    {
        // Mantém só o giro em Y (para os lados); zera as inclinações em X e Z
        Quaternion emPe = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        transform.rotation = emPe;
        if (corpo != null)
        {
            corpo.rotation = emPe;
            corpo.angularVelocity = Vector3.zero;
        }
    }
}
