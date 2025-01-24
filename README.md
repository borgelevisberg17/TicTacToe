Tic Tac Toe - Jogo da Velha

Este é um projeto de Jogo da Velha desenvolvido em .NET MAUI, com funcionalidades para 2 jogadores ou um modo contra o computador. O jogo possui animações, sistema de pontuação e lógica para determinar vencedores ou empates.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


📋 Funcionalidades

🎮 Modo de Jogo:

-- 2 Jogadores no mesmo dispositivo.

-- Contra o Computador (com uma lógica inteligente de jogadas usando estratégias).


⭐ Pontuação:

Cada vitória incrementa o placar do jogador vencedor.

O jogo termina quando um dos jogadores atinge uma pontuação definida como "meta de vitórias".


🎨 Animações e Interface:

-- Botões e elementos visuais animados para maior interatividade.

-- Feedback visual ao reiniciar rodadas ou reiniciar o jogo completo.


🧠 Modo Computador:

O computador tenta fazer jogadas estratégicas para vencer ou bloquear o jogador humano.




🛠️ Tecnologias Utilizadas

.NET MAUI: Framework para o desenvolvimento de aplicativos multiplataforma.

C#: Linguagem principal para a lógica do jogo.


🚀 Como Executar o Projeto

1. Certifique-se de que você possui o .NET 7 instalado.
Caso não tenha, instale a SDK do .NET aqui.


2. Clone o repositório:

git clone https://github.com/seu-usuario/tic-tac-toe.git
cd tic-tac-toe


3. Execute o projeto no seu ambiente de desenvolvimento:

dotnet build
dotnet run


4. Caso esteja em um ambiente de IDE como Visual Studio, abra o arquivo .sln e execute o projeto.


📷 Imagens do Projeto

Como:

1. Tela inicial.

2. Jogo em andamento.

3. Modo contra o computador.

📚 Como Jogar

1. Escolha o modo:

- Modo 2 Jogadores: Dois jogadores alternam jogadas no mesmo dispositivo.

- Modo Computador: O jogador humano joga como "X", enquanto o computador joga como
"O". E possuí 3 níveis, o fácil o algoritmo basea-se em escolhas aleatórias,
enquanto o médio usa 70% de escolhas baseadas no minimax já as 30% são
aleatórias também, enquanto o difícil usa 100% de jogadas baseadas no minimax.



2. Toque em um espaço vazio para fazer sua jogada.


3. O vencedor da rodada será anunciado e o placar atualizado.


4. O jogo termina quando um jogador atingir a pontuação máxima definida no código.


📈 Planejamentos Futuros

- Melhorar a inteligência artificial do computador usando o algoritmo Minimax.

- Adicionar suporte a multiplayer online.

- Personalização de interface, como custom temas.

💻 Contribuições

Contribuições são sempre bem-vindas! Se você deseja melhorar este projeto:

1. Faça um fork do repositório.


2. Crie uma nova branch:

git checkout -b minha-feature


3. Faça suas alterações e realize o commit:

- git commit -m "Adicionei nova feature"


4. Envie para o repositório remoto:

- git push origin minha-feature


5. Abra um Pull Request.

🏗️ Autor

Desenvolvido por @borge.levisberg.

Se gostou do projeto, deixe uma estrela ⭐ no repositório!
