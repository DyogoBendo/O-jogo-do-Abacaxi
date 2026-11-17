using UnityEngine;
using TMPro;

// Coloque este script em cada peso de ferro (no objeto que tem o Rigidbody).
// Ele guarda o valor do peso, usado pela balança de pesagem.
public class PesoFerro : MonoBehaviour
{
    [Tooltip("Valor do peso em kg")]
    public int peso = 1;

    [Tooltip("Opcional: o texto gravado no peso, atualizado automaticamente")]
    public TMP_Text numero;

    void OnValidate()
    {
        if (numero != null) numero.text = peso + "KG";
    }
}
