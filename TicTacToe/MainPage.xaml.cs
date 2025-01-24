using System;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui;
using System.Collections.Generic;
using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;

namespace TicTacToe
{
    public partial class MainPage : ContentPage
    {
        private string currentPlayer = "X";
        private string[,] board = new string[3, 3];
        private int scoreX = 0;
        private int scoreO = 0;
        private const int WinningScore = 5;
        private bool isComputerMode = false;
        private enum Difficulty { Easy, Medium, Hard }
        private Difficulty currentDifficulty = Difficulty.Medium;
        private Random random = new Random();

        private readonly IAudioManager audioManager;
        private IAudioPlayer backgroundMusicPlayer;

        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            ResetRound();
            _ = PlayBackgroundMusicAsync(); // Corrigido para usar o novo nome do método
        }

        private async Task PlayBackgroundMusicAsync()
        {
            try
            {
                var audioFile = await FileSystem.OpenAppPackageFileAsync("relaxante.mp3");
                backgroundMusicPlayer = audioManager.CreatePlayer(audioFile);
                backgroundMusicPlayer.Loop = true;
                backgroundMusicPlayer.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao tocar música de fundo: {ex.Message}");
            }
        }




        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            backgroundMusicPlayer?.Dispose();
        }




        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.Opacity = 0;
            await this.FadeTo(1, 1000);
            await Task.Delay(1000); // Adicione um atraso de 1 segundo
            await PlayBackgroundMusicAsync();
        }



        private async Task RestartGame()
        {
            scoreX = 0;
            scoreO = 0;
            currentPlayer = "X";
            board = new string[3, 3];

            ScoreLabel.Text = "X: 0 | O: 0";

            foreach (var child in GameGrid.Children)
            {
                if (child is Button button)
                {
                    button.Text = string.Empty;
                    await button.FadeTo(0, 200);
                    await button.FadeTo(1, 200);
                }
            }

            await GameGrid.ScaleTo(0.8, 200, Easing.CubicOut);
            await GameGrid.ScaleTo(1, 200, Easing.CubicIn);
        }

        private async void OnRestartButtonClicked(object sender, EventArgs e)
        {
            await RestartGame();
        }

        private async void OnToggleModeClicked(object sender, EventArgs e)
        {
            isComputerMode = !isComputerMode;
            await RestartGame();
            UpdateModeButton();
        }

        private void UpdateModeButton()
        {
            if (isComputerMode)
            {
                modeButton.Text = "Vs PC";
                ((FontImageSource)modeButton.ImageSource).Glyph = "\ue30a"; // Ícone de computador
            }
            else
            {
                modeButton.Text = "2 Jogadores";
                ((FontImageSource)modeButton.ImageSource).Glyph = "\ue7ef"; // Ícone de pessoas
            }

            // Atualizar o texto da dificuldade se estiver no modo computador
            if (isComputerMode)
            {
                DifficultyButton.Text = currentDifficulty.ToString();
            }
            else
            {
                DifficultyButton.Text = "Dificuldade";
            }
        }


        private async void OnDifficultyButtonClicked(object sender, EventArgs e)
        {
            if (!DropdownFrame.IsVisible)
            {
                DropdownFrame.TranslationY = DropdownFrame.Height;
                DropdownFrame.IsVisible = true;
                await DropdownFrame.TranslateTo(0, 0, 250, Easing.CubicOut);
            }
            else
            {
                await DropdownFrame.TranslateTo(0, DropdownFrame.Height, 250, Easing.CubicIn);
                DropdownFrame.IsVisible = false;
            }
        }


        private void OnDifficultySelected(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                currentDifficulty = (Difficulty)int.Parse(button.ClassId);
                DifficultyButton.Text = button.Text.ToUpper();
                DropdownFrame.IsVisible = false;
            }
        }





        private async void OnButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && string.IsNullOrEmpty(button.Text))
            {
                button.Text = currentPlayer;
                var row = Grid.GetRow(button);
                var col = Grid.GetColumn(button);
                board[row, col] = currentPlayer;

                if (CheckWinner(row, col))
                {
                    UpdateScore();
                    await DisplayAlert("Fim da Rodada", $"{currentPlayer} venceu esta rodada!", "OK");
                    await ResetRound();
                }
                else if (IsBoardFull())
                {
                    await DisplayAlert("Fim da Rodada", "Empate!", "OK");
                    await ResetRound();
                }
                else
                {
                    currentPlayer = currentPlayer == "X" ? "O" : "X";

                    if (isComputerMode && currentPlayer == "O")
                    {
                        await Task.Delay(500); // Simular tempo de "pensamento" do computador
                        MakeComputerMove();
                    }
                }
            }
        }

        private void MakeComputerMove()
        {
            switch (currentDifficulty)
            {
                case Difficulty.Easy:
                    MakeRandomMove();
                    break;
                case Difficulty.Medium:
                    MakeMediumMove();
                    break;
                case Difficulty.Hard:
                    MakeHardMove();
                    break;
            }
        }

        private void MakeRandomMove()
        {
            var emptyCells = new List<(int, int)>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(board[i, j]))
                    {
                        emptyCells.Add((i, j));
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                var move = emptyCells[random.Next(emptyCells.Count)];
                ExecuteMove(move.Item1, move.Item2);
            }
        }
        private void MakeMediumMove()
        {
            // 70% chance de fazer uma jogada inteligente, 30% de fazer uma jogada aleatória
            if (random.Next(100) < 70)
            {
                MakeHardMove();
            }
            else
            {
                MakeRandomMove();
            }
        }

        private void MakeHardMove()
        {
            var bestMove = Minimax(board, true).move;
            if (bestMove != (-1, -1))
            {
                ExecuteMove(bestMove.row, bestMove.col);
            }
        }

        private void ExecuteMove(int row, int col)
        {
            Button button = GetButtonByPosition(row, col);
            button.Text = currentPlayer;
            board[row, col] = currentPlayer;

            if (CheckWinner(row, col))
            {
                UpdateScore();
                DisplayAlert("Fim da Rodada", $"{currentPlayer} venceu esta rodada!", "OK");
                ResetRound();
            }
            else if (IsBoardFull())
            {
                DisplayAlert("Fim da Rodada", "Empate!", "OK");
                ResetRound();
            }
            else
            {
                currentPlayer = currentPlayer == "X" ? "O" : "X";
            }
        }

        private (int score, (int row, int col) move) Minimax(string[,] board, bool isMaximizing)
        {
            if (IsWinner("O")) return (1, (-1, -1));
            if (IsWinner("X")) return (-1, (-1, -1));
            if (IsBoardFull()) return (0, (-1, -1));

            int bestScore = isMaximizing ? int.MinValue : int.MaxValue;
            (int row, int col) bestMove = (-1, -1);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(board[i, j]))
                    {
                        board[i, j] = isMaximizing ? "O" : "X";
                        int score = Minimax(board, !isMaximizing).score;
                        board[i, j] = null;

                        if ((isMaximizing && score > bestScore) || (!isMaximizing && score < bestScore))
                        {
                            bestScore = score;
                            bestMove = (i, j);
                        }
                    }
                }
            }

            return (bestScore, bestMove);
        }

        private bool IsWinner(string player)
        {
            for (int i = 0; i < 3; i++)
            {
                if ((board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) ||
                    (board[0, i] == player && board[1, i] == player && board[2, i] == player))
                    return true;
            }

            if ((board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) ||
                (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player))
                return true;

            return false;
        }

        private bool CheckWinner(int row, int col)
        {
            return IsWinner(currentPlayer);
        }

        private Button GetButtonByPosition(int row, int col)
        {
            return GameGrid.Children.Cast<Button>().First(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == col);
        }

        private bool IsBoardFull()
        {
            foreach (var cell in board)
            {
                if (string.IsNullOrEmpty(cell))
                    return false;
            }
            return true;
        }

        private void UpdateScore()
        {
            if (currentPlayer == "X")
                scoreX++;
            else
                scoreO++;

            ScoreLabel.Text = $"X: {scoreX} | O: {scoreO}";
        }

        private async Task ResetRound()
        {
            foreach (var child in GameGrid.Children)
            {
                if (child is Button button)
                {
                    button.Text = string.Empty;
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await button.FadeTo(0, 200);
                        await button.FadeTo(1, 200);
                    });
                }
            }

            board = new string[3, 3];
            currentPlayer = "X";
        }

    }
}