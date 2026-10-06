# Spooks & Fuels
## Introdução
Esse é um jogo de sobrevivência 3d, onde o jogador controla um fantasma que dirige uma locomotiva por um cemitério e é atacado por fantasmas malignos. O jogador deve chegar no destino, abastecendo constantemente a locomotiva e combatendo os fatasmas inimigos.

## Mecânicas

### Jogador
- Modelo 3D : pequeno fantasma.
- Visão: Terceira pessoa.
- Ações: 
    - Movimento;
    - Pulo;
    - Pegar objetos;
    - Atacar.

#### Interação com itens do jogo
 - Ao colidir com um obejto, ele é coletado;
 - O item equipado aparece no inventário do jogador;

### Locomotiva
- Modelo 3D: Locomotiva a vapor;
- Movimento linear em um trilho;
- Vida: 10;
- Combustivel: Se move se tiver;
- Dano: Derrota inimigos que ficam no seu caminho;
- Ações (Controlado pelo fantasma jogador):
    - Abastecer (itens coletados pelo mapa e drops dos inimigos);
    - Ligar;
    - Parar.

### Crafting de itens

- Vagões anexados a locomotiva permitem interação para craft de itens;
- Vagões e suas funções:

Vagões | Vagão 1 | Vagão 2 | Vagão 3 | *Vagão 0*
:--- | :--: | :---: | :---: | :---: 
**Craft** | Vassoura | Machado | Picareta | *Locomotiva*
**Primeiro Craft** | - | 5 `ectoplasmas` | 5 `madeiras` & 5 `ectoplasmas` | 10 `ectoplasmas`
**Upgrade 1** | 10 `ectoplasmas` & 10 `madeiras` | 10 `madeiras` & 10 `ectoplasmas` | 10 `pedras` & 10 `ectoplasmas` | 10 `ectoplasmas` & 10 `madeiras`
**Upgrade 2** | 20 `ectoplasmas` & 10 `pedras` | 20 `madeiras` & 10 `pedras` | 20 `pedras` & 10 `madeiras` | 15 `madeiras` & 15 `pedras`
**Upgrade 3** | 30 `ectoplasmas` & 10 `ferros` | 30 `madeiras` & 10 `ferros` | 30 `pedras` & 10 `ferros` | 20 `pedras` & 20 `ferros` 

- Itens e suas melhorias:

Itens | Vassoura | Picareta | Machado | *Locomotiva*
--- | :---: | :---: | :---: | :---:
Primeiro Craft | - | - | - | 2x `velocidade`
Upgrade 1 | +1 `ataque` | -1 `tempo/quebra` | -1 `tempo/quebra` | 2x `combustível`
Upgrade 2 | -1 `tempo/ataque` | +1 `drop` | +1 `drop` | 2x `vida`
Upgrade 3 | +1 `ataque` & -1 `tempo/ataque` | +1 `drop` & -1 `tempo/quebra` | +1 `drop` & -1 `tempo/quebra` | 1.5x `todos atributos` 

### Inimigos
Os inimigos atacam a `locomotiva` ao entrar em contato físico. O jogador deve combatê-los para que a locomotiva não seja destruída.

**Comuns:**
- Modelo 3D: Fantasmas voadores, diversos tipos;
- Vida: 2;
- Spawnam com frequência;
- Velocidade: média;
- Dano: 1;
- Dropam ectoplasma

**Frágeis:**
- Modelo 3D: comuns, mas menores;
- Vida: 1;
- Spawnam com pouca frequência;
- Velocidade: alta;
- Dano: 1;

 **Raros:**
 - Modelo 3D: Versões evoluídas dos fantasmas comuns;
 - Vida: 4;
 - Spawnam com pouca frequência;
 - Velocidade: lenta;
 - Dano: 2;
 - Dropam ectoplasma;

 **Gigantes:**
 - Modelo 3D: Raros, mas bem maiores;
 - Vida: 12;
 - Velocidade: Bem lenta;
 - Dano: 8;
 - Spawnam com pouquíssima frequência;
 - Se divide em dois fantasmas com metade dos seus status ao morrer que se dividem em mais dois cada.

### Objetos quebráveis
- Pedras (rochas, lápides):
    - Bloqueiam os trilhos;
    - Espalhadas pelo cenario;
    - Remove 2 vida da locomotiva se colidir;
    - Deixa a locomotiva lenta
    - Dropam pedras ao serem destruídas que servem para fabricar itens;
    - Destruída se o jogador atacar com `picareta`.
- Madeiras (árvores, barrís, caixas):
    - Espalhadas pelo cenario;
    - Dropam madeira para usar de combustível para a locomotiva e para fabricar itens;
    - Remove 1 vida da locomotiva se colidir;
    - Destruída se o jogador atacar com `machado`.
- Minério de ferro
    - Espalhado pelo mapa;
    - Dropa ferro;
    - Destruída se o jogador atacar com `picareta`.

### Itens (loot & receitas)
- Ectoplasma:
    - Dropado por fantasmas comuns e raros;
    - Meio ineficiênte de abastecer a locomotiva, mas é o mais comum;
    - Jogador deve apertar botão para jogar ectoplasma na locomotiva.

- Super Ectoplasma:
    - Dropado por fantasmas raros;
    - Mesma eficiência do ectoplasma;
    - Aumenta velocidade da locomotiva;
    - Jogador deve apertar botão para jogar ectoplasma na locomotiva.

- Madeira:
    - Meio mais eficiente de abastecer a locomotiva;
    - Jogador deve colocar madeira na locomotiva.

- Pedra:
    - Nenhum efeito especial.

- Ferro:
    - Cura 1 de vida da locomotiva.

### Ferramentas

Todas as ferramentas podem ser achadas em baús pelo cenário.

- Machado:
    - Usado para quebrar caixas/barris;
- Picareta:
    - Usado para quebrar pedras;
- Espada:
    - Usada para dar dano nos fantasmas;


### Interface
- **Jogador:**
    - Inventário na parte inferior central da tela.
- **Locomotiva:**
    - Vida;
    - Combustível.

### Controles

Ação           | Teclado e Mouse   | Controle
:--------------|:----------:| :------:
Movimentação   | WASD       | Analogic
Pulo           | Space      | A
Interagir      | E          | Y
Atacar         | Left Click | Right Trigger

### Interações
- Atacar: 
    - Dano em inimigos; 
    - Quebra de objetos;
    - Uso de itens.
- Interagir:
    - Parar/Ligar locomotiva;
    - Fabricar itens.
- Pulo:
    - Subir na locomotiva.

## Fim de jogo

### Derrota
- A vida da locomotiva chega em zero.

### Vitória
- O jogador chega no destino com a Locomotiva.
    





