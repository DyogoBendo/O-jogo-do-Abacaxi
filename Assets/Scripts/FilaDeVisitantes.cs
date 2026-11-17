using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Coloque este script num objeto vazio ("Visitantes") com os bonecos como filhos.
// Ligue o método Liberar no evento "Ao Abrir Feira" do GameManager.
// Quando a feira abre, ele espera o portão abrir e manda os bonecos entrarem, um de cada vez.
public class FilaDeVisitantes : MonoBehaviour
{
    [Tooltip("Os bonecos. Se ficar vazio, usa todos os Visitante filhos deste objeto")]
    public Visitante[] visitantes;

    [Tooltip("Pontos por onde passam para entrar, em ordem (ex.: na frente do portão, do lado de fora; e logo depois dele, do lado de dentro). " +
             "O eixo vermelho (X) de cada ponto é a largura do portão")]
    public Transform[] rotaDeEntrada;
    [Tooltip("Largura do portão por onde eles se espalham ao passar (em metros)")]
    public float larguraDaPassagem = 2f;

    [Tooltip("Lugares da feira para onde vão (e passeiam depois)")]
    public Transform[] pontosNaFeira;

    [Tooltip("Segundos entre a feira abrir e o primeiro boneco andar (tempo do portão abrir)")]
    public float atrasoInicial = 6f;
    [Tooltip("Segundos entre um boneco e o próximo")]
    public float intervalo = 0.7f;

    bool liberou;

    void Awake()
    {
        if (visitantes == null || visitantes.Length == 0)
            visitantes = GetComponentsInChildren<Visitante>();
    }

    public void Liberar()
    {
        if (liberou) return;
        liberou = true;
        StartCoroutine(LiberarEmFila());
    }

    IEnumerator LiberarEmFila()
    {
        yield return new WaitForSeconds(atrasoInicial);

        // Quem está mais perto do portão entra primeiro
        var fila = new List<Visitante>(visitantes);
        if (rotaDeEntrada.Length > 0)
        {
            Vector3 portao = rotaDeEntrada[0].position;
            fila.Sort((a, b) => Vector3.Distance(a.transform.position, portao)
                        .CompareTo(Vector3.Distance(b.transform.position, portao)));
        }

        var destinos = new Vector3[pontosNaFeira.Length];
        for (int i = 0; i < destinos.Length; i++) destinos[i] = pontosNaFeira[i].position;

        int proximoDestino = Random.Range(0, Mathf.Max(1, destinos.Length));
        foreach (var visitante in fila)
        {
            if (visitante == null) continue;

            // Cada um passa por um lugar diferente da largura do portão
            float lado = Random.Range(-larguraDaPassagem, larguraDaPassagem) * 0.5f;
            var rota = new List<Vector3>();
            foreach (var ponto in rotaDeEntrada)
                rota.Add(ponto.position + ponto.right * lado);

            // Destinos diferentes para cada um (vai rodando a lista)
            if (destinos.Length > 0)
            {
                rota.Add(destinos[proximoDestino % destinos.Length]);
                proximoDestino++;
            }

            visitante.Entrar(rota, destinos);
            yield return new WaitForSeconds(intervalo);
        }
    }

    // Mostra a rota e os pontos no editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (rotaDeEntrada != null)
            for (int i = 0; i < rotaDeEntrada.Length; i++)
            {
                if (rotaDeEntrada[i] == null) continue;
                Vector3 p = rotaDeEntrada[i].position;
                Gizmos.DrawLine(p - rotaDeEntrada[i].right * larguraDaPassagem * 0.5f, p + rotaDeEntrada[i].right * larguraDaPassagem * 0.5f);
                if (i > 0 && rotaDeEntrada[i - 1] != null) Gizmos.DrawLine(rotaDeEntrada[i - 1].position, p);
            }
        Gizmos.color = Color.yellow;
        if (pontosNaFeira != null)
            foreach (var p in pontosNaFeira)
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.4f);
    }
}
