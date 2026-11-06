# Auto Walk Around

**[English](#english) · [Português](#português)**

---

## English

Tired of auto-run getting stuck on every tree, rock and fence, so you have to strafe left and right by hand? With this mod, auto-run steers around whatever is in the way and then carries on in the same direction. It also sprints for you: it sprints while you have stamina, rests until stamina is full, and repeats.

### Features

- **Client-side only.** The server and other players don't need the mod.
- **Steers around obstacles:** trees, rocks, fences, walls and buildings. Once past, you go back to the direction you were heading.
- **Gets unstuck:** if you stop moving against something, it tries the other side.
- **Sprint cycle:** while auto-running, it sprints when your stamina is full, stops when it runs out, waits for it to fill up again and repeats.
- Only acts during auto-run. Normal walking is untouched.
- Steering pauses while you swim, ride or sit.
- Works with **Improved Auto-run and Jump**: steering pauses while you strafe by hand.
- Every part can be turned off on its own.

### Requirements

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

### Installation

**r2modman / Thunderstore Mod Manager:** click *Install*. To install a downloaded zip, go to Settings → *Import local mod*.

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
| LookAhead | 3 m | How far ahead to look for obstacles. |
| MaxTurn | 90° | The widest turn used to get around something. |
| AvoidCreatures | off | Also steer around creatures and other players. |
| AutoJump | off | Jump over low obstacles instead of going around them. Only use it if auto-run survives a jump (Improved Auto-run and Jump does that). |
| PauseWhileStrafing | on | Stop steering while you strafe by hand. |
| AutoSprint | on | The sprint cycle. Turn it off if another mod handles sprinting. |
| StartAtPercent | 100 | Stamina (%) needed to start sprinting again. |
| StopAtPercent | 0 | Stamina (%) at which sprinting stops. |
| DetailedLog | off | Writes details to the BepInEx log (for bug reports). |

### Notes

- Works well with **Bed Recall**, **Gather Tweaks**, **Build Tweaks** and **DigForHoe** (same author).

---

## Português

Cansado da corrida automática travar em toda árvore, pedra e cerca, e ter que andar para os lados na mão? Com este mod, a corrida automática contorna o que estiver no caminho e depois segue na mesma direção. Ele também corre rápido por você: corre enquanto tem stamina, espera ela encher e repete.

### Recursos

- **Só no seu jogo.** O servidor e os outros jogadores não precisam ter o mod.
- **Desvia de obstáculos:** árvores, pedras, cercas, paredes e construções. Depois de passar, você volta para a direção em que estava indo.
- **Destrava sozinho:** se você parar encostado em algo, ele tenta pelo outro lado.
- **Ciclo de corrida rápida:** durante a corrida automática, corre rápido com a stamina cheia, para quando ela acaba, espera encher de novo e repete.
- Só age na corrida automática. Andar normal não muda nada.
- O desvio pausa enquanto você nada, está montado ou sentado.
- Funciona com o **Improved Auto-run and Jump**: o desvio pausa enquanto você anda para os lados na mão.
- Cada parte pode ser desligada separadamente.

### Requisitos

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

### Instalação

**r2modman / Thunderstore Mod Manager:** clique em *Install*. Para instalar um zip baixado, use Configurações → *Import local mod*.

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
| LookAhead | 3 m | Distância à frente em que ele procura obstáculos. |
| MaxTurn | 90° | A maior curva usada para contornar algo. |
| AvoidCreatures | desligado | Desvia também de criaturas e outros jogadores. |
| AutoJump | desligado | Pula obstáculos baixos em vez de contornar. Só use se a corrida automática continuar depois do pulo (o Improved Auto-run and Jump faz isso). |
| PauseWhileStrafing | ligado | Para de desviar enquanto você anda para os lados na mão. |
| AutoSprint | ligado | O ciclo de corrida rápida. Desligue se outro mod já cuida da corrida rápida. |
| StartAtPercent | 100 | Stamina (%) necessária para voltar a correr rápido. |
| StopAtPercent | 0 | Stamina (%) em que a corrida rápida para. |
| DetailedLog | desligado | Escreve detalhes no log do BepInEx (para relatar bugs). |

### Observações

- Combina com o **Bed Recall**, o **Gather Tweaks**, o **Build Tweaks** e o **DigForHoe** (mesmo autor).
