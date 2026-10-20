# King of the Kill

Jogo de ação 2D com mira independente do movimento (*twin-stick shooter*), desenvolvido em Unity para estudar programação, combate e construção de arenas. O jogador participa de uma competição mortal transmitida como um programa de televisão: precisa sobreviver, explorar os setores do estúdio e conquistar acesso à grande final.

**Estado:** protótipo em desenvolvimento. **Plataforma prevista:** navegador, via itch.io. **Resolução de referência:** 640 × 480.

- Jogar no itch.io: [King of the Kill](https://weirdessen.itch.io/king-of-the-kill)
- Nome completo do aluno: **Gabriel Moreira Alves da Silva**

Este README reúne as informações de jogabilidade, os controles, as configurações de dificuldade e os créditos do projeto. As instruções refletem as atualizações de pausa, preparação das salas e composição de inimigos de 19/10/2026. Consolidação documental: **20/10/2026**.

- [Como jogar](#como-jogar)
- [Dificuldades](#dificuldades)
- [Controle](#controle)
- [Créditos e fontes dos assets](#créditos-e-licenças)

## Como jogar

Elimine todos os inimigos da sala para abrir as portas. Escolha o próximo caminho, recolha munição e procure as credenciais **A** e **B**. As duas são necessárias para entrar na arena do **Executor da Audiência**. Derrotar o chefe e seus acompanhantes encerra a partida em vitória.

O jogador tem três pontos de vida. A partida termina quando a vida chega a zero ou quando acaba o prazo de uma sala em combate. O cronômetro de combate para depois da limpeza; explorar salas já concluídas não consome esse prazo.

| Ação | Teclado e mouse | Controle |
|---|---|---|
| Mover | WASD ou setas | Analógico esquerdo |
| Mirar | Mouse sobre a arena | Analógico direito |
| Atirar | Segurar o botão esquerdo | Segurar RT ou RB |
| Pausar / retornar | Esc | Start (Menu) |
| Cancelar confirmação de reinício/saída | Esc ou Cancelar | B ou Cancelar |
| Voltar à tela inicial | Pausa → Tela inicial → Sim, sair | Pausa → Tela inicial → Sim, sair |
| Trocar de sala | Entrar em uma porta aberta | Entrar em uma porta aberta |
| Navegar nos menus | Mouse ou teclado | Direcional/esquerdo; A confirma, B volta |

## Mecânicas

- Doze arenas conectadas, com 6–9 inimigos por sala no Fácil, 8–12 no Normal e 10–15 no Difícil. Caminhos alternativos e salas opcionais.
- Portas bloqueadas durante o combate e acessos que exigem credenciais.
- Minimapa com conexões reais, sala atual, salas concluídas e acessos bloqueados.
- Soldados lentos, robôs rápidos e hitmen estacionários com rajadas nas doze salas. Robôs comuns e blindados estão 10% mais lentos que na revisão anterior.
- Variantes resistentes e chefe que intensifica os ataques ao perder metade da vida.
- Coberturas que bloqueiam tiros e perseguidores que contornam obstáculos.
- Pontos por derrotar inimigos, recolher dinheiro, joias e credenciais.
- Invulnerabilidade breve com piscadas após receber dano.
- Derrota por vida ou tempo, explosões e resultado após o som de morte.
- Menu, história, instruções, confirmação visual e transições.
- Pausa com Start/Esc, retorno ao jogo, reinício e saída para a tela inicial; as duas últimas opções pedem confirmação. Congela combate, prazo e áudio.
- Preparação de 2 segundos com Get Ready! antes dos combates, sem gastar o prazo da sala.
- Balas com sprites direcionais e mira do controle com zona morta única e suavização angular.
- Chefe com explosão maior e prolongada, aplausos e espera antes do painel de vitória.

Munição e cura não concedem pontos. A cura recupera uma vida e fica no chão se a vida já estiver cheia. Cada pacote de munição entrega oito tiros; a partida começa com 28, 24 ou 20 tiros, conforme a dificuldade. Com até dois tiros e nenhum pacote alcançável, o jogo procura um ponto livre para repor munição. Ficar 2,5 segundos com zero tiros concede três tiros de emergência, mesmo se um pacote existente estiver cercado. O prazo do combate continua correndo.

A final tem um chefe e 6/8/10 acompanhantes no Fácil/Normal/Difícil. Os pacotes de cada sala fornecem pelo menos 50% de tiros extras em relação à vida total dos inimigos do Difícil, sem contar munição trazida de outra sala nem reposição de emergência.

As salas preservam seu estado durante a partida: voltar não recria inimigos nem recompensas coletadas. Cada sala concluída aumenta em 5% a velocidade de movimento dos próximos inimigos, até o limite de 25%.

## Dificuldades

Depois de **Jogar**, o flash de confirmação e o narrador acontecem juntos. A seleção de dificuldade abre ao término da confirmação e da voz. Clique em **Fácil**, **Normal** ou **Difícil**, ou confirme com A/Enter, para iniciar diretamente; navegar apenas mostra a descrição. O botão **Voltar** permanece. A escolha fica salva para a próxima partida e para Reiniciar; o padrão inicial é **Normal**.

| Modificação | Fácil | Normal | Difícil |
| --- | ---: | ---: | ---: |
| Inimigos por sala | 6–9 | 8–12 | 10–15 |
| Inimigos no circuito completo | 92 | 122 | 155 |
| Tempo de cada sala | Base × 1,30, arredondado para cima | Base | Base × 0,90, arredondado para cima |
| Movimento dos inimigos | 90% | 100% | 115% |
| Velocidade dos projéteis inimigos | 85% | 100% | 110% |
| Pausa entre rajadas inimigas | 120% | 100% | 85% |
| Munição inicial | 28 | 24 | 20 |
| Proteção após receber dano | 1,2 s | 1 s | 1 s |

A vida permanece em três danos. Não há multiplicador oculto da vida dos inimigos: cada variante usa a vida definida em seu prefab. O movimento também recebe +5% por sala concluída, até +25%. Os robôs comuns e blindados tiveram a velocidade base reduzida em 10% antes desses multiplicadores: `Speed Multiplier` passou de 3,05 para 2,745 e de 3,30 para 2,97, respectivamente.

O intervalo entre tiros de uma mesma rajada e o aviso visual antes de disparar foram mantidos. Após o **Get Ready!** de 2 segundos, os atiradores reiniciam uma espera aleatória de 1 a 2 segundos antes de preparar o primeiro ataque. A preparação não consome o prazo da sala e também congela durante a pausa.

Os totais do circuito consideram todas as doze salas, inclusive as opcionais. A contagem da final inclui exatamente um chefe em cada dificuldade.

## Estrutura e edição

| Caminho | Conteúdo |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Menu, história e instruções |
| `Assets/Scenes/Arena01Estudio.unity` | Cena que hospeda todo o circuito |
| `Assets/Data/Arenas/` | Configurações das doze salas |
| `Assets/Prefabs/` | Jogador, inimigos, projéteis e itens |
| `Assets/Scripts/` | Lógica do jogo e da interface |
| `Assets/Editor/ProjectValidation.cs` | Verificação pelo menu do Editor |
| `Assets/Art/` | Sprites, interface e efeitos visuais |
| `Assets/Audio/KenneyFeedback/` | Novos sons curtos e licenças |
| `README.md` | Jogabilidade, controles, dificuldades, créditos e fontes dos assets |
| `Tests/validate_difficulty_layouts.py` | Auditoria geométrica e de recursos, fora do Unity |

Para editar uma arena, selecione seu asset em `Assets/Data/Arenas/`: nome, anúncio, prazo, cores, posições, inimigos, itens, coberturas e portas são configuráveis. `mapPosition` define a coordenada no minimapa e `mapLabel` a sigla. Cada porta deve apontar para uma sala vizinha com porta de retorno; inclua novas salas na lista `Arenas` do `WaveManager` da cena.

Vida, velocidade, pontos e rajadas ficam nos prefabs dos inimigos. Sons de coleta ficam no componente `Pickup`; o som de sala concluída fica no `WaveManager`. O `MenuActions` usa a gravação atribuída a `Title Voice`; o projeto contém `Assets/Audio/SFX/Title Voice.mp3`. A voz começa junto da confirmação de Jogar, antes da dificuldade. A espera fixa de `Title Delay` foi removida.

### Onde ajustar as configurações

- `Assets/Data/Arenas/*.asset`: `Minimum Difficulty` define em quais modos cada inimigo aparece. Fácil aparece em todos; Normal acrescenta reforços ao Normal/Difícil; Difícil aparece apenas nesse modo. `Time Limit` é o prazo base da sala. `Duration` no `WaveManager` é somente o valor inicial de segurança.
- `Assets/Scripts/GameDifficulty.cs`: multiplicadores, munição inicial, proteção e descrições. Ao mudar um valor, atualize também o texto exibido.
- `Assets/Scripts/ControllerSettings.cs`: preferências e resposta do analógico.
- `Assets/Scripts/MatchSetupMenu.cs`: seleção de dificuldade e ajustes de controle, montados com os recursos existentes.
- `Assets/Scripts/GameController.cs`: `RoomPreparationDuration` controla os 2 segundos de preparação.
- `Assets/Scripts/PauseMenu.cs`: opções e confirmações da pausa.

## Controle

Os comandos básicos de teclado, mouse e controle estão em [Como jogar](#como-jogar). No menu **Controle**, ajuste a mira do Xbox conforme sua preferência. As configurações são salvas localmente.

O analógico esquerdo move o personagem com velocidade proporcional à inclinação, sem ganho extra na diagonal. O analógico direito mira de forma independente e mantém a última direção ao ser solto. Um filtro reduz mudanças involuntárias quando o analógico retorna ao centro.

Segure **RT ou RB** para atirar; **X** também funciona como alternativa. O gatilho ativa o disparo a partir de 15% do curso e o libera abaixo de 10%.

| Ajuste | Opções e funcionamento |
| --- | --- |
| Zona morta | 12%, **16% (padrão)**, 20% ou 24%. Aumente se a mira se mover sozinha com o analógico em repouso; diminua para responder a movimentos menores. |
| Resposta da mira | **Direta:** sem suavização. **Leve (padrão):** resposta rápida com suavização breve. **Suave:** amortecimento maior. A entrada inicial e viradas fortes respondem imediatamente. |
| Assistência de mira | Leve, opcional e ligada por padrão. Considera inimigos visíveis em um cone de 7° e até 8 unidades, com correção máxima de 2,5°. Não atravessa paredes nem dispara automaticamente. |

A mira do mouse continua direta e não recebe assistência. Nos menus, use o direcional ou o analógico esquerdo para navegar, **A** para confirmar e **B** para voltar. O analógico direito fica reservado à mira durante a partida.

Desconectar o controle em uso pausa a partida; reconectá-lo não retoma o jogo automaticamente. Ao retornar, solte o botão ou gatilho de tiro antes de disparar novamente.

## Exportar para itch.io

1. Em **File → Build Profiles**, selecione **Web**.
2. Mantenha `MainMenu` e `Arena01Estudio`, nessa ordem, na lista de cenas do perfil ativo.
3. Confira a referência de **640 × 480**. O projeto usa **Gzip** e **Decompression Fallback**.
4. Use **Build and Run** e teste no servidor local aberto pelo Unity.
5. Compacte o conteúdo da pasta de build em um **ZIP**, com `index.html` na raiz.
6. No itch.io, escolha um projeto HTML, envie esse ZIP e configure a área para 640 × 480.
7. Teste áudio após a primeira interação, mira, controle, troca de salas, derrota, vitória e reinício no navegador.

O projeto-fonte não é o build para upload. Consulte [jogos HTML5 no itch.io](https://itch.io/docs/creators/html5) e [distribuição Web no Unity](https://docs.unity3d.com/6000.4/Documentation/Manual/webgl-deploying.html).

## Página do itch.io e critérios da avaliação

Os quatro itens devem aparecer na página publicada. Campos entre colchetes são pendências, não dados prontos para publicação.

| Critério | Onde preencher | Verificação |
|---|---|---|
| Imagem de capa | **Edit game → Cover image** | A miniatura do projeto mostra o jogo e o título |
| Descrição | **Short description** e descrição principal | Explica proposta, objetivo e controles |
| Nome do aluno | Na descrição principal, perto do início | Nome completo visível; não depender apenas do apelido da conta |
| Link do repositório | Na descrição principal | Link clicável para este projeto; abre para o avaliador |

**Descrição curta sugerida:** Sobreviva a uma competição mortal de TV: explore arenas, reúna credenciais e enfrente o chefe em um shooter 2D com mira independente.

**Texto-base para a descrição principal** — substitua os dois campos antes de publicar:

> **King of the Kill** é um jogo de ação 2D com mira independente do movimento, ambientado em uma competição mortal transmitida pela televisão.
>
> Elimine os inimigos antes que o tempo acabe, abra as portas e explore doze arenas conectadas. Enfrente soldados, robôs e hitmen, recolha munição e cura e reúna as credenciais A e B para chegar à grande final. Dinheiro e joias aumentam sua pontuação, mas o jogador só tem três pontos de vida.
>
> **Controles:** WASD ou setas para mover, mouse para mirar e botão esquerdo para atirar. No controle, use o analógico esquerdo para mover, o direito para mirar e o gatilho direito para atirar.
>
> **Aluno e desenvolvedor:** [PREENCHER NOME COMPLETO].
>
> **Código-fonte e documentação:** [PREENCHER URL DO REPOSITÓRIO].
>
> Projeto acadêmico desenvolvido em Unity. Versão para navegador de computador.

No editor de texto do itch.io, selecione “Código-fonte e documentação”, clique no botão de link e insira a URL real. Inclua abaixo os créditos da seção [Créditos e licenças](#créditos-e-licenças) deste README, completando as entradas ainda pendentes.

### Capa com os assets existentes

1. Rode a versão final do jogo e capture uma imagem real do menu ou de uma arena, sem janelas do Editor, mensagens de erro ou cursor cobrindo o título.
2. Prepare uma imagem **630 × 500 px**, mantendo a proporção dos sprites e da captura. Essa proporção é recomendada pelo itch.io; a resolução da capa é independente dos 640 × 480 do jogo.
3. Use o título **KING OF THE KILL** em tamanho legível na miniatura. Aproveite os personagens e a identidade visual já presentes; evite uma imagem que prometa conteúdo ausente do jogo.
4. Exporte PNG/JPG e envie em **Cover image**. A imagem de fundo da página e o fundo do menu dentro do jogo não preenchem esse campo.
5. Acrescente de três a cinco capturas reais: combate, outra arena e encontro com o chefe, por exemplo. Preserve os créditos dos assets utilizados.

A consolidação desta documentação não gerou imagens nem áudios. Use capturas da versão que será publicada para preparar ou atualizar a capa.

### Conferência da página publicada

- [ ] Capa enviada no campo correto e legível em miniatura.
- [ ] Descrição curta e principal preenchidas, sem campos entre colchetes.
- [ ] Nome completo do aluno visível na própria página.
- [ ] Link do repositório correto e acessível ao avaliador.
- [ ] Créditos externos completos.
- [ ] Build **Web em ZIP**, com `index.html` na raiz, selecionado para jogar no navegador.
- [ ] Área de jogo em **640 × 480**, com início por clique para permitir a interação inicial com áudio.
- [ ] Página, link do repositório e partida conferidos em uma janela anônima.

Fontes: [primeira página no itch.io](https://itch.io/docs/creators/getting-started), [envio de jogos HTML5](https://itch.io/docs/creators/html5).

## Tratamento de dead-end e softlock

Um beco sem saída opcional com porta de retorno não é, por si só, um softlock. A derrota normal pelo prazo também encerra a partida: o problema seria perder a possibilidade de avançar **e** de chegar a um encerramento/reinício válido por falha do jogo.

- **Mapa:** as quinze conexões são bidirecionais. As credenciais não dependem uma da outra; ambas podem ser obtidas antes da final.
- **Revisita:** salas limpas conservam os itens não recolhidos e não reiniciam o combate. É possível sair sem a credencial e voltar para buscá-la.
- **Credencial perdida:** a sala concluída recoloca uma recompensa ausente ou inacessível. Se o prefab/espaço impedir isso, entrega a credencial diretamente, sem pontuação de coleta.
- **Munição:** a reposição consulta posições conectadas ao jogador, com uma reserva de três tiros após 2,5 segundos sem munição, inclusive quando um pacote está cercado. O tempo continua sendo um risco legítimo.
- **Transição:** erros no comando de troca são registrados no Console e a sobreposição deixa de bloquear a interface. A conclusão/cancelamento libera os controles e permite tentar novamente. Essa proteção não repara automaticamente uma cena ou prefab configurado incorretamente.
- **Contagem de inimigos:** o contador das portas é reconciliado com as instâncias realmente vivas. Destruir ou desativar um inimigo fora do fluxo normal não deixa uma contagem fantasma nem concede pontos pela simples remoção.
- **Inimigo fora da arena:** após alguns segundos fora da área navegável, é reposicionado em um ponto livre, conectado e longe do jogador, preservando a vida. Um spawn inválido também procura uma alternativa antes de criar o inimigo.
- **Encerramento:** morte, tempo esgotado e vitória oferecem reinício/retorno; a pausa permite voltar à tela inicial com confirmação. Esses fluxos devem ser testados no Editor e no build.

## Créditos e licenças

Esta seção incorpora os créditos do TXT original, incluindo os acréscimos de publicação de 17/10/2026. As informações necessárias para atribuição e as pendências ficam reunidas aqui. Os arquivos de licença incluídos nas pastas dos assets devem ser preservados.

### Arte, interface, sprites e tipografia

| Uso | Autor e pacote | Fonte e licença |
| --- | --- | --- |
| Personagens, pisos, portas, coberturas e joias | Kenney — Top-down Shooter | [Página do pacote](https://kenney.nl/assets/top-down-shooter) · [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) |
| Interface do menu e HUD | Kenney — UI Pack: Sci-Fi | [Página do pacote](https://kenney.nl/assets/ui-pack-sci-fi) · CC0 1.0 |
| Ícones de teclado, mouse e controle | Kenney — Input Prompts | [Página do pacote](https://kenney.nl/assets/input-prompts) · CC0 1.0 |
| Chaves e dinheiro | Kenney — Generic Items | [OpenGameArt](https://opengameart.org/content/generic-items) · [Autor](https://kenney.nl/) · CC0 1.0 |
| Sprites de balas | Kenney — Shooting Gallery | [Fonte oficial](https://kenney.nl/assets/shooting-gallery) · [Pacote no OpenGameArt](https://opengameart.org/content/shooting-gallery) · CC0 1.0 |
| Fundo do menu | hassekf / Ask4Asset — Sci-Fi Background | [OpenGameArt](https://opengameart.org/content/sci-fi-background) · CC0 1.0 indicada pelo autor |
| Fonte Liberation Sans | Autores identificados no arquivo OFL distribuído com a fonte | `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt` |

Arquivos e adaptações documentados:

- **Generic Items:** pacote original `kenney_genericItems_updatedCross.zip`; `PNG/Colored/genericItem_color_155.png` → `Assets/Art/Kenney/GenericItems/Key.png`; `genericItem_color_158.png` → `Assets/Art/Kenney/GenericItems/Money.png`. Pixels copiados sem modificação; tamanho e cor aplicados pela importação Unity e pelo SpriteRenderer. Licença em `Assets/Art/Kenney/GenericItems/License.txt`.
- **Shooting Gallery:** pacote original `shooting-gallery-pack.zip`; `PNG/HUD/icon_bullet_gold_short.png` e `icon_bullet_silver_short.png` → `Assets/Art/Kenney/ShootingGallery/`. Pixels preservados; escala, rotação e cor das balas inimigas aplicadas por SpriteRenderer/Transform. O `License.txt` original acompanha os PNGs.
- **Fundo:** `sci_fi_bg1.jpg` → `Assets/Art/Menu/sci_fi_bg1.jpg`. Pixels preservados; enquadramento feito na interface do Unity.
- Os arquivos de licença de Top-down Shooter, UI Pack: Sci-Fi e Input Prompts foram mantidos nas respectivas pastas do projeto.

### Sons de interface, coleta, sala limpa e aplausos

| Evento/estado | Autor e pacote | Original → arquivo do projeto | Duração aproximada | Fonte e licença |
| --- | --- | --- | ---: | --- |
| Credenciais, dinheiro e joias | Kenney — Interface Sounds 1.0 | `Audio/confirmation_001.ogg` → `Assets/Audio/KenneyFeedback/KeyPickup.ogg` | 0,290 s | [Interface Sounds](https://kenney.nl/assets/interface-sounds) · CC0 1.0 |
| Sala concluída | Kenney — Interface Sounds 1.0 | `Audio/confirmation_004.ogg` → `Assets/Audio/KenneyFeedback/RoomClear.ogg` | 0,490 s | [Interface Sounds](https://kenney.nl/assets/interface-sounds) · CC0 1.0 |
| Kit de cura | Kenney — Digital Audio | `Audio/powerUp7.ogg` → `Assets/Audio/KenneyFeedback/HealthPickup.ogg` | 0,522 s | [Digital Audio](https://kenney.nl/assets/digital-audio) · CC0 1.0 |
| Confirmação de menu/pausa | Joth — 7 Assorted Sound Effects (Menu, Level Up) | `Menu Confirm.mp3` → `Assets/Audio/JothUI/Confirm.mp3` | — | [Pacote](https://opengameart.org/content/7-assorted-sound-effects-menu-level-up) · CC0 1.0 |
| Coleta de munição | Joth — mesmo pacote | `Item Pickup.mp3` → `Assets/Audio/JothUI/Ammo.mp3` | — | [Pacote](https://opengameart.org/content/7-assorted-sound-effects-menu-level-up) · CC0 1.0 |
| Antigo som de cura; sem uso nas coletas atuais | Joth — mesmo pacote | `Ability Learn.mp3` → `Assets/Audio/JothUI/Heal.mp3` | — | [Pacote](https://opengameart.org/content/7-assorted-sound-effects-menu-level-up) · CC0 1.0 |
| Antigo som de recompensa; substituído nos eventos atuais | Joth — mesmo pacote | `Level Up.mp3` → `Assets/Audio/JothUI/Reward.mp3` | — | [Pacote](https://opengameart.org/content/7-assorted-sound-effects-menu-level-up) · CC0 1.0 |
| Chefe derrotado | Blender Foundation — Yo Frankie!; recorte por LeeZH | `applause.wav` → `Assets/Audio/BlenderFoundation/BossApplause.wav` | 7,65 s | [Applause](https://opengameart.org/content/applause) · [Original Endgame](https://opengameart.org/content/endgame) · [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/) |

Os três sons curtos do Kenney foram copiados integralmente, sem cortes, síntese ou geração. Licenças originais: `Assets/Audio/KenneyFeedback/Interface-License.txt` e `Digital-License.txt`. Os arquivos de Joth vieram de `UISoundEffects.zip`, sem edição ou geração; a licença acompanha `Assets/Audio/JothUI/LICENSE.txt`.

**Atribuição dos aplausos:** Blender Foundation, do jogo *Yo Frankie!*, recorte da seção de aplausos por LeeZH — [Applause](https://opengameart.org/content/applause), sob [Creative Commons Attribution 3.0 Unported](https://creativecommons.org/licenses/by/3.0/). O WAV do recorte foi copiado sem edição adicional; somente o volume é controlado no AudioSource. Não há endosso dos autores. Registro local em `Assets/Audio/BlenderFoundation/CREDITS.txt`.

### Músicas, vozes e demais áudios

Os usos abaixo foram conferidos nas referências das cenas e dos prefabs. Para os áudios de terceiros, as fontes identificam as obras, autores e páginas de obtenção disponíveis. A ferramenta de criação do anúncio do título foi informada pelo desenvolvedor.

| Uso e arquivo | Obra/autoria ou origem informada | Fonte e situação da identificação |
| --- | --- | --- |
| Música do menu — `Assets/Audio/Music/Menu.wav` | Tema de menu de **One Must Fall: 2097**; composição de **Kenny Chou**, também creditado como Kenny Chow; jogo da Diversions Entertainment / Epic MegaGames | [Registro da trilha e créditos — VGMPF](https://www.vgmpf.com/Wiki/index.php/One_Must_Fall%3A_2097_%28DOS%29). O link exato de obtenção/versão do arquivo usado no projeto não foi documentado. |
| Música da arena — `Assets/Audio/Music/Android.wav` | **Android / Super Android**, de **Epic Pinball**; composição de **Robert A. Allen**; jogo da Digital Extremes / Epic MegaGames. O título interno do WAV é `Adrenaline` | [Faixa Android e créditos — VGMPF](https://www.vgmpf.com/Wiki/index.php?title=Epic_Pinball_%28DOS%29). Falta registrar o módulo/arquivo de origem exato usado para exportar este WAV. |
| Morte dos soldados — `Assets/Audio/SFX/ytmp3free.cc_metal-slug-rebel-scream-7-youtubemp3free.org.mp3` | Grito de rebelde associado a **Metal Slug**, conforme origem informada no projeto | [Referência localizada: Metal Slug rebel scream #7, publicada por Lyndione](https://www.youtube.com/watch?v=sJveGEWq69w). O título corresponde ao nome do arquivo; a cadeia de extração e a autoria individual da amostra ainda não estão documentadas. |
| Morte dos hitmen — `Assets/Audio/SFX/doom-guy-death-scream_PSfmYjq.mp3` | Grito de morte associado a **Doom (1993)** / id Software, conforme origem informada; o nome do arquivo não comprova qual personagem o emitiu | [Doom Guy Death Scream — Myinstants, publicação de snooflmua](https://www.myinstants.com/en/instant/doom-guy-death-scream-91043/). O download dessa página aponta para o mesmo nome de MP3 do projeto; o publicador não é identificado como autor da gravação original. |
| Derrota do jogador — `Assets/Audio/SFX/male ko.wav` | Voz de **Street Fighter II** / **Capcom**, proveniente de **Capcom Classics Collection Reloaded**, conforme a descrição da fonte; pacote divulgado por **SnixTheDog** | [Street Fighter II - HQ Voice Clips — YouTube](https://www.youtube.com/watch?v=G-tl3o8nDAs). [SFII_HQ_Voice_Clips.zip — link indicado na descrição](https://www.mediafire.com/file/ku1kkjk2g5i3cr8/SFII_HQ_Voice_Clips.zip/file). Fonte do som usado no projeto informada pelo desenvolvedor. |
| Morte dos robôs e do chefe — `Assets/Audio/SFX/mechanical_explosion.wav` | **Mechanical Explosion**, atribuição provável a **Spring Spring** | [Publicação do autor no OpenGameArt](https://opengameart.org/content/mechanical-explosion), indicada como CC0. Associação por nome e tamanho compatível (arquivo local de 522.402 bytes; página indica 522,4 KB); a igualdade das amostras não foi comparada. |
| Anúncio “King of the Kill” — `Assets/Audio/SFX/Title Voice.mp3` | Anúncio criado para o projeto usando **Audacity**, conforme informado pelo desenvolvedor | Ferramenta de criação: [Audacity](https://www.audacityteam.org/). O arquivo é usado em MainMenu para anunciar o título antes da seleção de dificuldade. |

Os WAVs `Menu.wav` e `Android.wav` registram **OpenMPT 1.32.11.00** como software de exportação. Isso identifica a ferramenta usada, não altera a autoria das músicas. Os MP3s `One Must Fall 2097 - Soundtrack Main Theme (Good sound quality).mp3` e `Epic Pinball Super Android OST.mp3` permanecem em `Assets/Audio/Music/` como arquivos de referência, sem ligação direta às cenas atuais; a reprodução usa os WAVs.

Para as músicas e amostras comerciais acima, não há licença específica de reutilização registrada nos materiais recebidos. **A licença CC0 de Kenney e Joth não se estende a esses arquivos.** As referências de música e morte foram preservadas; esta consolidação não baixou nem substituiu áudios.

### Outros arquivos com origem pendente

Os seguintes sprites já estavam no projeto; faltam autor, pacote, fonte e licença específicos:

- `Assets/Art/Pickups/ammo-pistol 32px.png`;
- `Assets/Art/Pickups/health-red 32px.png`;
- `Assets/Art/VFX/1.png`;
- `Assets/Art/VFX/explosion 2.png`.

### Texto-base de créditos para a página do itch.io

> **Arte e interface:** Kenney — [Top-down Shooter](https://kenney.nl/assets/top-down-shooter), [UI Pack: Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi), [Input Prompts](https://kenney.nl/assets/input-prompts), [Generic Items](https://opengameart.org/content/generic-items) e [Shooting Gallery](https://kenney.nl/assets/shooting-gallery), sob CC0 1.0.
>
> **Coletas e sala limpa:** Kenney — [Interface Sounds](https://kenney.nl/assets/interface-sounds) e [Digital Audio](https://kenney.nl/assets/digital-audio), sob CC0 1.0.
>
> **Menu e munição:** Joth — [7 Assorted Sound Effects (Menu, Level Up)](https://opengameart.org/content/7-assorted-sound-effects-menu-level-up), sob CC0 1.0.
>
> **Fundo do menu:** hassekf / Ask4Asset — [Sci-Fi Background](https://opengameart.org/content/sci-fi-background), sob CC0 1.0.
>
> **Aplausos:** Blender Foundation, do jogo *Yo Frankie!*, recorte por LeeZH — [Applause](https://opengameart.org/content/applause), sob [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/), sem edição adicional. Sem endosso dos autores.
>
> **Música do menu:** tema de *One Must Fall: 2097*, de Kenny Chou. **Música da arena:** Android/Super Android, de *Epic Pinball*, composta por Robert A. Allen. As fontes e os arquivos utilizados estão detalhados neste README.
>
> **Sons de morte dos inimigos:** amostras associadas a *Metal Slug* e *Doom*, com procedência e pendências detalhadas acima. **Explosão mecânica:** provável *Mechanical Explosion*, de Spring Spring — [fonte](https://opengameart.org/content/mechanical-explosion); confirmar a correspondência do arquivo.
>
> **Derrota do jogador:** voz de *Street Fighter II* / Capcom, proveniente de *Capcom Classics Collection Reloaded*. Pacote divulgado por SnixTheDog: [Street Fighter II - HQ Voice Clips](https://www.youtube.com/watch?v=G-tl3o8nDAs), com [ZIP dos áudios indicado na descrição](https://www.mediafire.com/file/ku1kkjk2g5i3cr8/SFII_HQ_Voice_Clips.zip/file).
>
> **Anúncio do título:** criado para King of the Kill usando [Audacity](https://www.audacityteam.org/).
>