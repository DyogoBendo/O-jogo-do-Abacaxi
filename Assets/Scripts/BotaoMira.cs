using UnityEngine;
using UnityEngine.UI;

// Coloque este script num Button de um Canvas em World Space.
// Ele cria uma área de clique do tamanho do botão, para o jogador
// poder clicar mirando com o centro da tela (cursor travado).
// Ao ser clicado, dispara o OnClick do botão normalmente.
[RequireComponent(typeof(Button))]
public class BotaoMira : MonoBehaviour
{
    Button botao;

    void Awake()
    {
        botao = GetComponent<Button>();

        // Cria (ou ajusta) a área de clique no tamanho do botão
        RectTransform rect = GetComponent<RectTransform>();
        BoxCollider area = GetComponent<BoxCollider>();
        if (area == null) area = gameObject.AddComponent<BoxCollider>();

        area.isTrigger = true;
        area.size = new Vector3(rect.rect.width, rect.rect.height, 10f);
        area.center = rect.rect.center;
    }

    // Chamado pelo PegarObjeto quando o jogador clica mirando no botão
    public void Clicar()
    {
        if (botao.interactable)
            botao.onClick.Invoke();
    }
}
