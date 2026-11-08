# Auto Walk Around

**[English](#english) · [Português](#português)**

---

## English

Tired of auto-run getting stuck on every tree, rock and fence, so you have to strafe left and right by hand? With this mod, auto-run steers around whatever is in the way and then carries on in the same direction. It also sprints for you: it sprints while you have stamina, rests until stamina is full, and repeats.

### Features

- **Client-side only.** The server and other players don't need the mod.
- **Steers around obstacles:** trees, rocks, fences, walls and buildings. Once past, you go back to the direction you were heading.
- **Turns at the last moment:** by default it keeps running until you are right at the obstacle, then goes along it until the way ahead is clear. Rocks and stone blocks you can walk up are not obstacles.
- **Gets unstuck:** if you stop moving against something, it jumps over it when it's low (a fallen log), turns harder, and only then tries the other side.
- **Sprint cycle:** while auto-running, it sprints when your stamina is full, stops when it runs out, waits for it to fill up again and repeats.
- **Your sprint key still rules:** press it to stop sprinting (the cycle then waits for you) or to sprint right away.
- Only acts during auto-run. Normal walking is untouched.
- Steering pauses while you swim, ride or sit.
- Works with **Improved Auto-run and Jump**: steering pauses while you strafe by hand.
- Every part can be turned off on its own.

### Requirements

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

### Installation

**r2modman / Thunderstore Mod Manager:** click *Install*. To install a downloaded zip, go to Settings → *Import local mod*.

**Nexus Mods / Vortex:** also on [Nexus Mods](https://www.nexusmods.com/valheim/mods/4892).

**Manual:**
1. Extract the zip.
2. Copy the `plugins/AutoWalkAround` folder into `Valheim/BepInEx/plugins/`.
   Result: `Valheim/BepInEx/plugins/AutoWalkAround/AutoWalkAround.dll`.
3. Launch the game.

To update, delete the old `AutoWalkAround` folder before copying the new one.

### Configuration

Press **F1** in game (requires Configuration Manager) or edit `BepInEx/config/felih.AutoWalkAround.cfg`.

| Option | Default | What it does |
|---|---|---|
| Enabled | on | Steers around obstacles during auto-run. |
| ToggleKey | none | Optional key that turns steering on and off in game. |
| StartTurning | WhenClose | `WhenClose` = keep running until you are right at the obstacle, then go along it. `Early` = start turning LookAhead metres before. |
| LookAhead | 3 m | How far ahead to look for obstacles when StartTurning is `Early`. |
| MaxTurn | 90° | The widest turn used to get around something. |
| AvoidCreatures | off | Also steer around creatures and other players. |
| AutoJump | off | Jump over low obstacles instead of going around them. Only use it if auto-run survives a jump (Improved Auto-run and Jump does that). |
| JumpWhenStuck | on | When stuck on something low (a fallen log), jump over it. Only with Improved Auto-run and Jump installed. |
| PauseWhileStrafing | on | Stop steering while you strafe by hand. |
| AutoSprint | on | The sprint cycle. Turn it off if another mod handles sprinting. |
| StartAtPercent | 100 | Stamina (%) needed to start sprinting again. |
| StopAtPercent | 0 | Stamina (%) at which sprinting stops. |
| DetailedLog | off | Writes details to the BepInEx log (for bug reports). |

### Notes

- Works well with **Gather Tweaks**, **Build Tweaks** and **DigForHoe** (same author).

---

## Português

Cansado da corrida automática travar em toda árvore, pedra e cerca, e ter que andar para os lados na mão? Com este mod, a corrida automática contorna o que estiver no caminho e depois segue na mesma direção. Ele também corre rápido por você: corre enquanto tem stamina, espera ela encher e repete.

### Recursos

- **Só no seu jogo.** O servidor e os outros jogadores não precisam ter o mod.
- **Desvia de obstáculos:** árvores, pedras, cercas, paredes e construções. Depois de passar, você volta para a direção em que estava indo.
- **Vira só na hora certa:** por padrão continua correndo até chegar bem perto do obstáculo e então vai ao longo dele até o caminho ficar livre. Pedras e blocos de pedra em que dá para subir não contam como obstáculo.
- **Destrava sozinho:** se você parar encostado em algo, ele pula por cima se for baixo (um tronco caído), vira mais forte e só então tenta pelo outro lado.
- **Ciclo de corrida rápida:** durante a corrida automática, corre rápido com a stamina cheia, para quando ela acaba, espera encher de novo e repete.
- **Sua tecla de correr manda:** aperte para parar de correr rápido (o ciclo espera você) ou para correr rápido na hora.
- Só age na corrida automática. Andar normal não muda nada.
- O desvio pausa enquanto você nada, está montado ou sentado.
- Funciona com o **Improved Auto-run and Jump**: o desvio pausa enquanto você anda para os lados na mão.
- Cada parte pode ser desligada separadamente.

### Requisitos

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

### Instalação

**r2modman / Thunderstore Mod Manager:** clique em *Install*. Para instalar um zip baixado, use Configurações → *Import local mod*.

**Nexus Mods / Vortex:** também está no [Nexus Mods](https://www.nexusmods.com/valheim/mods/4892).

**À mão:**
1. Extraia o zip.
2. Copie a pasta `plugins/AutoWalkAround` para dentro de `Valheim/BepInEx/plugins/`.
   Resultado: `Valheim/BepInEx/plugins/AutoWalkAround/AutoWalkAround.dll`.
3. Abra o jogo.

Para atualizar, apague a pasta `AutoWalkAround` antiga antes de copiar a nova.

### Configuração

Aperte **F1** no jogo (precisa do Configuration Manager) ou edite `BepInEx/config/felih.AutoWalkAround.cfg`.

| Opção | Padrão | O que faz |
|---|---|---|
| Enabled | ligado | Desvia de obstáculos na corrida automática. |
| ToggleKey | nenhuma | Tecla opcional para ligar e desligar o desvio no jogo. |
| StartTurning | WhenClose | `WhenClose` = continua correndo até chegar bem perto do obstáculo e então vai ao longo dele. `Early` = começa a virar LookAhead metros antes. |
| LookAhead | 3 m | Distância à frente em que ele procura obstáculos quando StartTurning é `Early`. |
| MaxTurn | 90° | A maior curva usada para contornar algo. |
| AvoidCreatures | desligado | Desvia também de criaturas e outros jogadores. |
| AutoJump | desligado | Pula obstáculos baixos em vez de contornar. Só use se a corrida automática continuar depois do pulo (o Improved Auto-run and Jump faz isso). |
| JumpWhenStuck | ligado | Quando trava em algo baixo (um tronco caído), pula por cima. Só com o Improved Auto-run and Jump instalado. |
| PauseWhileStrafing | ligado | Para de desviar enquanto você anda para os lados na mão. |
| AutoSprint | ligado | O ciclo de corrida rápida. Desligue se outro mod já cuida da corrida rápida. |
| StartAtPercent | 100 | Stamina (%) necessária para voltar a correr rápido. |
| StopAtPercent | 0 | Stamina (%) em que a corrida rápida para. |
| DetailedLog | desligado | Escreve detalhes no log do BepInEx (para relatar bugs). |

### Observações

- Combina com o **Gather Tweaks**, o **Build Tweaks** e o **DigForHoe** (mesmo autor).
