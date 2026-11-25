# O Jogo do Abacaxi

Jogo educacional de matemática em Unity, feito em equipe para a disciplina (Trabalho 2, ambiente 3D interativo).
Responda e comente o código em português. O usuário ainda é iniciante em programação: explique passo a passo, uma etapa por vez, e prefira scripts com campos configuráveis pelo Inspector.

## Ambiente

- Unity **6000.6.1f1** (Unity 6), pipeline **URP**. Todos da equipe usam exatamente essa versão.
- Input System novo (`UnityEngine.InputSystem`), não o `Input` antigo.
- Repositório no GitHub. **Tudo o que as cenas do build usam está versionado** (conferido pelas dependências das cinco cenas). Ficam de fora só arquivos não usados dos pacotes da Asset Store (cenas de demonstração etc., pelo `.gitignore`), então não presuma que tudo em `Assets/` está no Git. Os pacotes do Unity vêm pelo `Packages/manifest.json`.
- A versão entregue do trabalho está na tag `entrega-trabalho2`. O trabalho novo é feito no branch `vr`. Confira o branch ativo antes de mudar algo.
- O MCP for Unity (CoplayDev) está instalado: com o Unity aberto e o servidor ligado (Window → MCP for Unity), dá para mexer na cena pelas ferramentas do UnityMCP. Faça uma etapa de cada vez e peça para o usuário conferir no editor.

## A ideia nova do jogo

O avô está cansado e pede ajuda ao neto (o jogador). Há **quatro barracas** (A, B, C e D), cada uma com uma expressão; o valor de x é o peso dos abacaxis daquela barraca. No centro, uma **balança antiga de dois pratos**: o jogador coloca **pesos de ferro de 1 a 5 kg** de um lado e testa abacaxis do outro até nivelar. Leva os abacaxis certos até o balcão da barraca, e **a barraca confere sozinha** quando eles ficam soltos e parados ali (não há sino).

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
- **Vídeo de abertura** narrado: só a voz do avô, câmera passeando pela feira (portão, quatro barracas, balança e pesos, balcão de uma barraca, portão). Gravado no Unity com o `PasseioCamera` e o Unity Recorder, numa cena `GravacaoVideo` (cópia da Feira), e exibido num telão dentro do cenário (para evitar enjoo em VR).
- **Versão VR (feita no branch `vr`):** veja a seção "Versão VR" abaixo. Óculos: **Meta Quest 3 / 3S** (APK Android). Sem teleporte.

### Roteiro do vídeo (~1 min 10 s; tempos aproximados, ajustar à narração gravada)

| Tempo | Câmera | Falas do avô |
| --- | --- | --- |
| 0:00–0:10 | Portão fechado com a placa "FEIRA FECHADA" | Chama o neto: os joelhos já não aguentam, precisa de ajuda. |
| 0:10–0:30 | Passa pelas barracas A, B, C e D, aproximando de cada placa | Cada barraca tem uma conta; o x é o peso dos abacaxis daquela barraca. |
| 0:30–0:45 | Balança antiga e os pesos de 1 a 5 kg | Os abacaxis estão misturados; peso de um lado, abacaxi do outro; se ficar reta, o peso é igual. |
| 0:45–0:52 | Placa da barraca D | Na última, talvez precise juntar dois pesos. |
| 0:52–1:02 | Balcão de uma barraca | Dois abacaxis por barraca; é só deixá-los no balcão que a barraca confere sozinha. |

> O roteiro original falava em puxar a cordinha de um sino, mas o sino foi tirado do jogo. Se a narração já gravada ainda falar do sino, esse trecho precisa ser regravado.
| 1:02–1:10 | Volta ao portão e escurece | Com as quatro prontas, a feira abre. |

## Versão VR

Pacotes: **XR Interaction Toolkit 3.7.0** (samples Starter Assets e XR Device Simulator importados) e **OpenXR 1.19.0** (Windows e Android, Meta Quest Support, perfis Meta Quest Touch Plus e Oculus Touch). Plataforma de build: **Android** (IL2CPP, ARM64).

Fluxo: `Menu` → `GravacaoVideo` (vídeo no telão) → `Feira` → `Feira 1` → `Feira 2`. As três Feiras são **fases** e todas são VR.

- **Jogador:** prefab `Assets/Prefabs/JogadorVR.prefab` (XR Origin) nas três fases e no vídeo; o `PlayerCapsule`, a `PlayerFollowCamera` e a `MainCamera` antigas ficam **desligadas** (não apagadas). O Menu tem um XR Origin próprio, parado (Locomotion desligado).
  - Altura **fixa de 1,70 m** para todos (Tracking Origin Mode = Device, Camera Y Offset 1,7 e `Camera Offset` em Y 1,7).
  - Analógico esquerdo anda (Move Speed 2,5), analógico direito gira suave (60°/s). Sem teleporte.
  - **Gatilho faz tudo**: pega/solta (Select em modo Toggle: um clique pega, outro solta) e aperta botões. A ação Select foi trocada de grip para gatilho no `XRI Default Input Actions` (se o sample for reimportado, refazer).
  - Objeto segurado fica no `PontoDeSegurar` de cada mão (1 m à frente; Far Attach Mode = Near e manipulação pelo analógico desligada).
  - Sem instruções de controles na tela (foram tiradas: o jogo usa só analógicos e gatilho).
- **Objetos pegáveis** (abacaxis e pesos): XR Grab Interactable (Velocity Tracking, para colidir com balança e balcão; Use Dynamic Attach; Interpolate) + `ManterEmPe`.
- **Painéis:** `CanvasFinal` e o Canvas do Menu em **World Space** com Tracked Device Graphic Raycaster; EventSystem com **XR UI Input Module**. O Canvas de controles do PC e a mira foram desligados/apagados.
- **Voltar ao menu:** Esc (PC) ou segurar o botão de menu do controle esquerdo por 1 s.
- **Vídeo:** a `CameraVideo` filma para `Assets/Texturas/TelaoVideo.renderTexture`, mostrada num telão na `SalaTelao` (50 m abaixo do chão), onde o `JogadorVR` assiste parado. Gatilho pula. Para **gravar** com o Recorder no PC, esvazie o Target Texture da `CameraVideo`.
- **Testar no PC:** o XR Device Simulator é criado sozinho no Play (só no editor). T/Y escolhem a mão, mouse move, clique esquerdo = gatilho. Para testar com o Quest pelo Link, desmarque "Use XR Device Simulator in scenes" em Project Settings → XR Plug-in Management → XR Interaction Toolkit.

## Estado anterior (desktop, primeira pessoa)

Cenas no Build Profiles: `Menu` (índice 0) e `Feira` (índice 1).

Jogador: Starter Assets – First Person (`PlayerCapsule`, `PlayerCameraRoot`, `MainCamera`, `PlayerFollowCamera`). Interação por raycast do centro da tela: tecla **E** ou **clique esquerdo** pega/solta abacaxis e pesos, aciona botões de UI e toca sinos. **Esc** volta ao menu.

### Scripts (`Assets/Scripts`)

| Script | Onde fica | Função |
| --- | --- | --- |
| `Abacaxi` | cada abacaxi | `peso` (int, kg) e etiqueta TextMeshPro opcional (no jogo novo, sem etiqueta). `[ExecuteAlways]`; usa o centro do `MeshRenderer` porque o pivô do modelo é deslocado. `travado` = já entregue numa barraca concluída. |
| `PesoFerro` | cada peso de ferro | `peso` (int, kg) e texto do número gravado. |
| `BalancaPesagem` | `Scale` da balança central | Soma o peso de cada prato (abacaxis e pesos), inclina o `Beam` e move `ChainsOriginLeft/Right`. Pratos com **Rigidbody cinemático**, movidos em `FixedUpdate` com `MovePosition`. `indicadorEquilibrio` opcional. |
| `SensorPesagem` | trigger acima de cada prato | Avisa a balança quando um abacaxi ou peso entra/sai (`ladoEsquerdo` define o prato). |
| `Barraca` | área trigger sobre o balcão de cada barraca | `pesoCerto` e `quantidadeNecessaria` (2). **Confere sozinha** no `Update`: quando os abacaxis da área estão soltos (fora da mão) e quase parados por `tempoParaConferir` segundos, chama `Conferir()`, que verifica quantidade e pesos, mostra dicas, toca som de acerto/erro, solta confete, trava os abacaxis e avisa o `GameManager`. Só confere de novo se os abacaxis na área mudarem. |
| `Sino` | (não usado) | Sobra da versão antiga, em que o sino chamava `Barraca.Conferir`. Não há sino em nenhuma cena; o `PegarObjeto` (desktop) ainda referencia o script. |
| `PegarObjeto` | `PlayerCapsule` | Raycast do centro da tela; pega/solta `Abacaxi` e `PesoFerro`; aciona `BotaoMira` e `Sino`. Não pega abacaxi `travado`. |
| `BotaoMira` | botão de UI em World Space | BoxCollider do tamanho do botão; dispara o `onClick` quando o jogador mira e aperta. |
| `GameManager` | objeto `GameManager` | Lista `barracas` (qualquer ordem), `BarracaConcluida()`, texto de progresso, abre a feira (vira o jogador, abre a porta, troca a música, mostra o painel de parabéns com atraso). |
| `Portao` | a porta (cubo) | Anima até `posicaoAberta`/`rotacaoAberta`; menu de contexto para gravar a posição e testar. |
| `TelaFinal` | painel de parabéns | Só age depois de a feira abrir: desliga os scripts do jogador, libera o cursor, Recomeçar/Sair. |
| `MenuInicial` | cena Menu | Painéis de menu e instruções, carrega `Feira` e trava o cursor ao começar. |
| `VoltarAoMenu` | GameManager | Esc carrega a cena `Menu`. |
| `PasseioCamera` | câmera do vídeo | Move a câmera por pontos (`Transform` + segundo de chegada) e toca a narração. |
| `MoverObjeto` | (não usado) | Desloca um objeto suavemente. |
| `JogadorVR` | (ajudante estático) | Trava/destrava a locomoção do XR Origin, acha a cabeça, põe um painel na frente do jogador. |
| `ObjetoNaMao` | (ajudante estático) | Diz se um abacaxi/peso está na mão (VR ou PC); usado pela balança e pela barraca. |
| `ManterEmPe` | abacaxis e pesos (VR) | Não gira com o controle, trava a rotação enquanto está na mão e endireita ao pegar/soltar. |
| `SombraObjetoSegurado` | `JogadorVR` | Sombra redonda embaixo do objeto segurado (versão VR da sombra do `PegarObjeto`). |
| `PontoDoLaser` | `JogadorVR` e XR Origin do Menu | Bolinha vermelha na ponta do raio quando ele está num objeto pegável ou num botão de UI (`mostrarNosBotoes`). |
| `TelaControles` | `Game Manager` das três fases (`Feira`, `Feira 1`, `Feira 2`) | Botão Y do controle esquerdo (F1 no PC) abre/fecha o `CanvasControles` na frente do jogador; trava só os modos de locomoção que estavam ligados e religa os mesmos ao fechar. Com a tela fechada mostra o aviso `CanvasDicaControles` ("Y - TELA DE CONTROLES"), preso à câmera no Play (`posicaoDica`). Os textos usam `Materiais/TextoTelaControles` porque o material padrão da LiberationSans tem cor de face preta. |

Removidos: `BalancaController` e `SensorPrato` (mecânica antiga com peso alvo e botão Verificar). Se ainda existirem no projeto, devem ser apagados.

### Particularidades do projeto

- Vários pacotes da Asset Store eram Built-in: materiais foram convertidos à mão para `Universal Render Pipeline/Lit`. Os móveis usam uma textura de **paleta de cores** (`market-stalls-colors`).
- O modelo do abacaxi tem escala ~10 e pivô fora do centro; por isso o código usa centro de massa/centro do renderer em vez de `transform.position`.
- Pesos e abacaxis: Rigidbody com Freeze Rotation X/Z, Collision Detection Continuous Speculative. Os pesos usam Mesh Collider convexo.
- Textos filhos de objetos com escala grande precisam de escala compensada.
