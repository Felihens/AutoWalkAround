# Changelog

## 1.0.4
- Smarter jumps: it measures how tall the obstacle is and jumps only when it's taller than a step and up to `MaxJumpHeight` (1.1 m), there's room above it and ground to land on behind it (not a drop or deep water), and going around would take a wide turn. It never jumps over creatures, and keeps going straight while in the air. New option `MaxJumpHeight`.
- Pulos mais espertos: mede a altura do obstáculo e só pula quando ele é mais alto que um degrau e até `MaxJumpHeight` (1,1 m), há espaço em cima e chão para pousar do outro lado (sem barranco nem água funda), e contornar exigiria uma virada grande. Nunca pula por cima de criaturas e segue reto enquanto está no ar. Nova opção `MaxJumpHeight`.

## 1.0.3
- Runs straight through bushes, saplings (young trees still growing) and berry or crop plants instead of steering around them. New option IgnorePlants (on). / Passa direto por arbustos, mudas (árvores novas ainda crescendo) e plantas de frutinhas ou plantações em vez de desviar delas. Nova opção IgnorePlants (ligada).

## 1.0.2
- New option StartTurning (WhenClose): it keeps running until you are right at the obstacle, then goes along it until the auto-run direction is clear again. Early = the old behaviour, turning LookAhead metres before. / Nova opção StartTurning (WhenClose): continua correndo até chegar bem perto do obstáculo e então vai ao longo dele até a direção da corrida automática ficar livre. Early = o jeito antigo, virando LookAhead metros antes.
- Standing on big rocks or stone blocks no longer makes it steer: only steep surfaces count as obstacles. / Ficar em cima de pedras grandes ou blocos de pedra não faz mais ele desviar: só superfícies íngremes contam como obstáculo.

## 1.0.1
- No more going back and forth along long fallen logs: it keeps a clear line along the obstacle, remembers which side it was going around, and when stuck it jumps (if the obstacle is low), then turns harder, and only then tries the other side. / Não fica mais indo e voltando em troncos compridos: segue uma linha livre ao longo do obstáculo, lembra de que lado estava contornando e, quando trava, pula (se o obstáculo for baixo), depois vira mais forte e só então tenta o outro lado.
- New option JumpWhenStuck (on): jump over low obstacles you get stuck on. Only with Improved Auto-run and Jump installed. / Nova opção JumpWhenStuck (ligada): pula obstáculos baixos quando trava. Só com o Improved Auto-run and Jump instalado.
- Your sprint key now switches sprinting on and off during auto-run. Switched off, the cycle waits until you press it again. / A tecla de correr agora liga e desliga a corrida rápida durante a corrida automática. Desligada, o ciclo espera você apertar de novo.


## 1.0.0
- First release. / Primeira versão.
- Auto-run steers around trees, rocks, fences and walls, then gets back on course. / A corrida automática contorna árvores, pedras, cercas e paredes e depois volta ao rumo.
- Sprint cycle: sprints at full stamina, stops when it runs out, rests until full, repeats. / Ciclo de corrida rápida: corre com a stamina cheia, para quando ela acaba, espera encher e repete.
