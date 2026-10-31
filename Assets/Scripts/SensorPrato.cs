using UnityEngine;

// Coloque este script num objeto com Box Collider (Is Trigger marcado)
// em cima do prato onde os abacaxis serão colocados.
[RequireComponent(typeof(Collider))]
public class SensorPrato : MonoBehaviour
{
    [Tooltip("A balança à qual este prato pertence (encontrada automaticamente)")]
    public BalancaController balanca;

    // Chamado ao adicionar o componente: já deixa o collider como Trigger
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        balanca = GetComponentInParent<BalancaController>();
    }

    void Awake()
    {
        if (balanca == null)
            balanca = GetComponentInParent<BalancaController>();
    }

    void OnTriggerEnter(Collider outro)
    {
        Abacaxi abacaxi = outro.GetComponentInParent<Abacaxi>();
        if (abacaxi != null && balanca != null)
            balanca.AbacaxiEntrou(abacaxi);
    }

    void OnTriggerExit(Collider outro)
    {
        Abacaxi abacaxi = outro.GetComponentInParent<Abacaxi>();
        if (abacaxi != null && balanca != null)
            balanca.AbacaxiSaiu(abacaxi);
    }
}
