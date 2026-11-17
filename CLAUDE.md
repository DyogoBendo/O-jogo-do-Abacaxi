# O Jogo do Abacaxi

Jogo educacional de matemática em Unity, feito em equipe para a disciplina (Trabalho 2, ambiente 3D interativo).
Responda e comente o código em português. O usuário ainda é iniciante em programação: explique passo a passo, uma etapa por vez, e prefira scripts com campos configuráveis pelo Inspector.

## Ambiente

- Unity **6000.6.1f1** (Unity 6), pipeline **URP**. Todos da equipe usam exatamente essa versão.
- Input System novo (`UnityEngine.InputSystem`), não o `Input` antigo.
- Repositório no GitHub. **Os assets grandes não estão no Git** (foram passados à parte), então não presuma que tudo em `Assets/` está versionado.
- A versão entregue do trabalho está na tag `entrega-trabalho2`. O trabalho novo é feito no branch `vr`. Confira o branch ativo antes de mudar algo.
- O MCP for Unity (CoplayDev) está instalado: com o Unity aberto e o servidor ligado (Window → MCP for Unity), dá para mexer na cena pelas ferramentas do UnityMCP. Faça uma etapa de cada vez e peça para o usuário conferir no editor.

## A ideia nova do jogo

O avô está cansado e pede ajuda ao neto (o jogador). Há **quatro barracas** (A, B, C e D), cada uma com uma expressão; o valor de x é o peso dos abacaxis daquela barraca. No centro, uma **balança antiga de dois pratos**: o jogador coloca **pesos de ferro de 1 a 5 kg** de um lado e testa abacaxis do outro até nivelar. Leva os abacaxis certos até a barraca e **puxa a cordinha do sino** para conferir.

| Barraca | Expressão | x |
| --- | --- | --- |
| A | x + 4 = 7 | 3 kg |
| B | 3x − 2 = 4 | 2 kg |
| C | 2x + 2 = 12 | 5 kg |
| D (desafio) | 2x − 4 = 10 | 7 kg (combina 5 + 2 na balança) |

Decisões:
- Abacaxis **sem etiqueta de peso** e com tamanhos parecidos, todos misturados num monte, com 3 ou 4 abacaxis "errados" (1 kg e 4 kg).
- **2 abacaxis por barraca.** Barracas em **qualquer ordem**.
- Ao concluir uma barraca, os abacaxis dela ficam presos e não podem mais ser pegos.
- Com as quatro barracas completas: o jogador é virado para o portão, a porta abre, a música da feira troca pela música de fim e, depois de alguns segundos, aparece a tela de parabéns (Recomeçar / Sair).
- **Vídeo de abertura** narrado: só a voz do avô, câmera passeando pela feira (portão, quatro barracas, balança e pesos, sino, portão). Gravado no Unity com o `PasseioCamera` e o Unity Recorder, numa cena `GravacaoVideo` (cópia da Feira), e exibido num telão dentro do cenário (para evitar enjoo em VR).
- **Versão VR (ainda não feita):** o jogador vira **XR Origin** com **XR Interaction Toolkit** (pegar com grip, teleporte). Mira, textos fixos na tela e menus em Screen Space precisam virar painéis em World Space. Óculos de VR ainda **a definir**.

### Roteiro do vídeo (~1 min 10 s; tempos aproximados, ajustar à narração gravada)

| Tempo | Câmera | Falas do avô |
| --- | --- | --- |
| 0:00–0:10 | Portão fechado com a placa "FEIRA FECHADA" | Chama o neto: os joelhos já não aguentam, precisa de ajuda. |
| 0:10–0:30 | Passa pelas barracas A, B, C e D, aproximando de cada placa | Cada barraca tem uma conta; o x é o peso dos abacaxis daquela barraca. |
| 0:30–0:45 | Balança antiga e os pesos de 1 a 5 kg | Os abacaxis estão misturados; peso de um lado, abacaxi do outro; se ficar reta, o peso é igual. |
| 0:45–0:52 | Placa da barraca D | Na última, talvez precise juntar dois pesos. |
| 0:52–1:02 | Sino de uma barraca e a cordinha | Dois abacaxis por barraca; ao terminar, puxar a cordinha do sino. |
| 1:02–1:10 | Volta ao portão e escurece | Com as quatro prontas, a feira abre. |

## Estado atual (desktop, primeira pessoa)

Cenas no Build Profiles: `Menu` (índice 0) e `Feira` (índice 1).

Jogador: Starter Assets – First Person (`PlayerCapsule`, `PlayerCameraRoot`, `MainCamera`, `PlayerFollowCamera`). Interação por raycast do centro da tela: tecla **E** ou **clique esquerdo** pega/solta abacaxis e pesos, aciona botões de UI e toca sinos. **Esc** volta ao menu.

### Scripts (`Assets/Scripts`)

| Script | Onde fica | Função |
| --- | --- | --- |
| `Abacaxi` | cada abacaxi | `peso` (int, kg) e etiqueta TextMeshPro opcional (no jogo novo, sem etiqueta). `[ExecuteAlways]`; usa o centro do `MeshRenderer` porque o pivô do modelo é deslocado. `travado` = já entregue numa barraca concluída. |
| `PesoFerro` | cada peso de ferro | `peso` (int, kg) e texto do número gravado. |
| `BalancaPesagem` | `Scale` da balança central | Soma o peso de cada prato (abacaxis e pesos), inclina o `Beam` e move `ChainsOriginLeft/Right`. Pratos com **Rigidbody cinemático**, movidos em `FixedUpdate` com `MovePosition`. `indicadorEquilibrio` opcional. |
| `SensorPesagem` | trigger acima de cada prato | Avisa a balança quando um abacaxi ou peso entra/sai (`ladoEsquerdo` define o prato). |
| `Barraca` | área trigger sobre o balcão de cada barraca | `pesoCerto` e `quantidadeNecessaria` (2). `Conferir()` (chamado pelo sino) verifica quantidade e pesos, mostra dicas, trava os abacaxis e avisa o `GameManager`. |
| `Sino` | objeto do sino (`TreeToy_bell`) | `Tocar()`: som + balanço do `corpo` (`Toy_bell`); evento `aoTocar` ligado a `Barraca.Conferir`. Campo `cordinha` deixado vazio (sem puxão). |
| `PegarObjeto` | `PlayerCapsule` | Raycast do centro da tela; pega/solta `Abacaxi` e `PesoFerro`; aciona `BotaoMira` e `Sino`. Não pega abacaxi `travado`. |
| `BotaoMira` | botão de UI em World Space | BoxCollider do tamanho do botão; dispara o `onClick` quando o jogador mira e aperta. |
| `GameManager` | objeto `GameManager` | Lista `barracas` (qualquer ordem), `BarracaConcluida()`, texto de progresso, abre a feira (vira o jogador, abre a porta, troca a música, mostra o painel de parabéns com atraso). |
| `Portao` | a porta (cubo) | Anima até `posicaoAberta`/`rotacaoAberta`; menu de contexto para gravar a posição e testar. |
| `TelaFinal` | painel de parabéns | Só age depois de a feira abrir: desliga os scripts do jogador, libera o cursor, Recomeçar/Sair. |
| `MenuInicial` | cena Menu | Painéis de menu e instruções, carrega `Feira` e trava o cursor ao começar. |
| `VoltarAoMenu` | GameManager | Esc carrega a cena `Menu`. |
| `PasseioCamera` | câmera do vídeo | Move a câmera por pontos (`Transform` + segundo de chegada) e toca a narração. |
| `MoverObjeto` | (não usado) | Desloca um objeto suavemente. |

Removidos: `BalancaController` e `SensorPrato` (mecânica antiga com peso alvo e botão Verificar). Se ainda existirem no projeto, devem ser apagados.

### Particularidades do projeto

- Vários pacotes da Asset Store eram Built-in: materiais foram convertidos à mão para `Universal Render Pipeline/Lit`. Os móveis usam uma textura de **paleta de cores** (`market-stalls-colors`).
- O modelo do abacaxi tem escala ~10 e pivô fora do centro; por isso o código usa centro de massa/centro do renderer em vez de `transform.position`.
- Pesos e abacaxis: Rigidbody com Freeze Rotation X/Z, Collision Detection Continuous Speculative. Os pesos usam Mesh Collider convexo.
- Textos filhos de objetos com escala grande precisam de escala compensada.
