using UnityEngine;

// Coloque este script num objeto com Box Collider (Is Trigger) em cima de cada prato.
// Ele avisa a balança quando um peso de ferro ou um abacaxi entra ou sai do prato.
[RequireComponent(typeof(Collider))]
public class SensorPesagem : MonoBehaviour
{
    [Tooltip("A balança deste prato (encontrada automaticamente)")]
    public BalancaPesagem balanca;
    [Tooltip("Marque no sensor do prato esquerdo; deixe desmarcado no direito")]
    public bool ladoEsquerdo;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        balanca = GetComponentInParent<BalancaPesagem>();
    }

    void Awake()
    {
        if (balanca == null) balanca = GetComponentInParent<BalancaPesagem>();
    }

    void OnTriggerEnter(Collider outro)
    {
        MonoBehaviour objeto = ObjetoComPeso(outro);
        if (objeto != null && balanca != null) balanca.Entrou(ladoEsquerdo, objeto);
    }

    void OnTriggerExit(Collider outro)
    {
        MonoBehaviour objeto = ObjetoComPeso(outro);
        if (objeto != null && balanca != null) balanca.Saiu(ladoEsquerdo, objeto);
    }

    // Um abacaxi ou um peso de ferro; qualquer outra coisa é ignorada
    static MonoBehaviour ObjetoComPeso(Collider c)
    {
        Abacaxi abacaxi = c.GetComponentInParent<Abacaxi>();
        if (abacaxi != null) return abacaxi;
        return c.GetComponentInParent<PesoFerro>();
    }
}
