using System;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui;
using System.Collections.Generic;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Plugin.Maui.Audio;
using Microsoft.Maui.Graphics;

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
        private IAudioPlayer? backgroundMusicPlayer;

        // 1. ADICIONE ESTE CONSTRUTOR VAZIO (Necessário para o gerador do XAML)
        public MainPage()
        {
            InitializeComponent();
            this.audioManager = Plugin.Maui.Audio.AudioManager.Current;
            _ = ResetRound();
            _ = PlayBackgroundMusicAsync();
        }

        // 2. MANTENHA O SEU CONSTRUTOR COM PARÂMETROS
        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            _ = ResetRound();
            _ = PlayBackgroundMusicAsync();
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
            await this.FadeToAsync(1, 1000);
            await Task.Delay(1000); // Adicione um atraso de 1 segundo
            await PlayBackgroundMusicAsync();

            CheckFirstTimeLaunch();
        }

        private void CheckFirstTimeLaunch()
        {
            bool hasShownTutorial = Preferences.Default.Get("HasShownTutorial", false);
            if (!hasShownTutorial)
            {
                ShowTutorial();
            }
        }

        private async void ShowTutorial()
        {
            TutorialOverlay.IsVisible = true;
            TutorialOverlay.Opacity = 0;
            await TutorialOverlay.FadeToAsync(1, 500, Easing.CubicOut);
        }

        private async void OnCloseTutorialClicked(object? sender, EventArgs e)
        {
            Preferences.Default.Set("HasShownTutorial", true);
            await TutorialOverlay.FadeToAsync(0, 300, Easing.CubicIn);
            TutorialOverlay.IsVisible = false;
        }



        private async Task RestartGame()
        {
            scoreX = 0;
            scoreO = 0;
            currentPlayer = "X";
            board = new string[3, 3];

            UpdateScoreDisplay();
            UpdateTurnIndicator();

            foreach (var child in GameGrid.Children)
            {
                if (child is Button button)
                {
                    button.Text = string.Empty;
                    await button.FadeToAsync(0, 200);
                    await button.FadeToAsync(1, 200);
                }
            }

            await GameGrid.ScaleToAsync(0.8, 200, Easing.CubicOut);
            await GameGrid.ScaleToAsync(1, 200, Easing.CubicIn);
        }

        private async void OnRestartButtonClicked(object? sender, EventArgs e)
        {
            await RestartGame();
        }

        private async void OnToggleModeClicked(object? sender, EventArgs e)
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


        private async void OnDifficultyButtonClicked(object? sender, EventArgs e)
        {
            if (!DropdownFrame.IsVisible)
            {
                DropdownFrame.TranslationY = DropdownFrame.Height;
                DropdownFrame.IsVisible = true;
                await DropdownFrame.TranslateToAsync(0, 0, 250, Easing.CubicOut);
            }
            else
            {
                await DropdownFrame.TranslateToAsync(0, DropdownFrame.Height, 250, Easing.CubicIn);
                DropdownFrame.IsVisible = false;
            }
        }


        private void OnDifficultySelected(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                currentDifficulty = (Difficulty)int.Parse(button.ClassId);
                DifficultyButton.Text = button.Text.ToUpper();
                DropdownFrame.IsVisible = false;
            }
        }





        private async void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && string.IsNullOrEmpty(button.Text))
            {
                button.Text = currentPlayer;
                button.TextColor = currentPlayer == "X" ? (Color)Resources["PrimaryColor"] : (Color)Resources["AccentColor"];
                await AnimateMove(button);

                var row = Grid.GetRow(button);
                var col = Grid.GetColumn(button);
                board[row, col] = currentPlayer;

                if (IsWinner(currentPlayer, out var winningCells))
                {
                    UpdateScore();
                    await HighlightWinningCells(winningCells);
                    ShowGameOverOverlay($"{currentPlayer} venceu!", true);
                }
                else if (IsBoardFull())
                {
                    ShowGameOverOverlay("Empate!", false);
                }
                else
                {
                    currentPlayer = currentPlayer == "X" ? "O" : "X";
                    UpdateTurnIndicator();

                    if (isComputerMode && currentPlayer == "O")
                    {
                        await Task.Delay(800); // Simular tempo de "pensamento" do computador
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

        private async void ExecuteMove(int row, int col)
        {
            Button button = GetButtonByPosition(row, col);
            button.Text = currentPlayer;
            button.TextColor = currentPlayer == "X" ? (Color)Resources["PrimaryColor"] : (Color)Resources["AccentColor"];
            await AnimateMove(button);

            board[row, col] = currentPlayer;

            if (IsWinner(currentPlayer, out var winningCells))
            {
                UpdateScore();
                await HighlightWinningCells(winningCells);
                ShowGameOverOverlay($"{currentPlayer} venceu!", true);
            }
            else if (IsBoardFull())
            {
                ShowGameOverOverlay("Empate!", false);
            }
            else
            {
                currentPlayer = currentPlayer == "X" ? "O" : "X";
                UpdateTurnIndicator();
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

        private bool IsWinner(string player, out List<(int row, int col)> winningCells)
        {
            winningCells = new List<(int, int)>();

            // Rows
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                {
                    winningCells.AddRange(new[] { (i, 0), (i, 1), (i, 2) });
                    return true;
                }
            }

            // Columns
            for (int i = 0; i < 3; i++)
            {
                if (board[0, i] == player && board[1, i] == player && board[2, i] == player)
                {
                    winningCells.AddRange(new[] { (0, i), (1, i), (2, i) });
                    return true;
                }
            }

            // Diagonals
            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
            {
                winningCells.AddRange(new[] { (0, 0), (1, 1), (2, 2) });
                return true;
            }
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
            {
                winningCells.AddRange(new[] { (0, 2), (1, 1), (2, 0) });
                return true;
            }

            return false;
        }

        private bool IsWinner(string player)
        {
            return IsWinner(player, out _);
        }

        private async Task AnimateMove(Button button)
        {
            try
            {
                if (HapticFeedback.Default.IsSupported)
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            }
            catch { }

            button.BackgroundColor = (Color)Resources["SurfaceColor"];
            button.Scale = 0.5;
            button.Opacity = 0;
            await Task.WhenAll(
                button.ScaleToAsync(1, 200, Easing.SpringOut),
                button.FadeToAsync(1, 200)
            );
        }

        private async Task HighlightWinningCells(List<(int row, int col)> cells)
        {
            try
            {
                if (HapticFeedback.Default.IsSupported)
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            }
            catch { }

            var winningColor = currentPlayer == "X" ? (Color)Resources["PrimaryColor"] : (Color)Resources["AccentColor"];
            var tasks = new List<Task>();

            foreach (var cell in cells)
            {
                var button = GetButtonByPosition(cell.row, cell.col);
                tasks.Add(button.ScaleToAsync(1.1, 300, Easing.CubicInOut));
                button.BackgroundColor = winningColor.WithAlpha(0.3f);
                button.TextColor = Colors.White;
            }

            await Task.WhenAll(tasks);
            await Task.Delay(500);
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

            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            ScoreXLabel.Text = scoreX.ToString();
            ScoreOLabel.Text = scoreO.ToString();
        }

        private void UpdateTurnIndicator()
        {
            TurnLabel.Text = $"PLAYER {currentPlayer}";
            var accentColor = (Color)Resources["AccentColor"];
            var primaryColor = (Color)Resources["PrimaryColor"];
            TurnIndicatorFrame.Background = currentPlayer == "X" ? primaryColor : accentColor;
        }

        private async void ShowGameOverOverlay(string message, bool isVictory)
        {
            ResultMessageLabel.Text = message;

            if (isVictory)
            {
                ResultTitleLabel.Text = "VITÓRIA!";
                ResultIconLabel.Text = "\ue801"; // trophy
                ResultIconLabel.TextColor = (Color)Resources["PrimaryColor"];
            }
            else
            {
                ResultTitleLabel.Text = "EMPATE";
                ResultIconLabel.Text = "\ue8d4"; // swap
                ResultIconLabel.TextColor = Colors.Gray;
            }

            GameOverOverlay.IsVisible = true;
            GameOverOverlay.Opacity = 0;
            await GameOverOverlay.FadeToAsync(1, 400, Easing.CubicOut);
        }

        private async void OnPlayAgainClicked(object? sender, EventArgs e)
        {
            await GameOverOverlay.FadeToAsync(0, 200, Easing.CubicIn);
            GameOverOverlay.IsVisible = false;
            await ResetRound();
        }

        private async Task ResetRound()
        {
            var surfaceColor = (Color)Resources["SurfaceColor"];
            foreach (var child in GameGrid.Children)
            {
                if (child is Button button)
                {
                    button.Text = string.Empty;
                    button.BackgroundColor = surfaceColor;
                    button.Scale = 1;
                    button.Opacity = 1;
                }
            }

            board = new string[3, 3];
            currentPlayer = "X";
            UpdateTurnIndicator();
        }

    }
}