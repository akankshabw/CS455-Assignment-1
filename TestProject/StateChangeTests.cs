using Bunit;
using Xunit;
using MoleProject.Pages;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq.Protected;
using Newtonsoft.Json;

namespace WhackEmAllTests
{
    public class WhackEmAllTests : TestContext
    {
        public static string GenerateRandomName() {
            return $"TestPlayer_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
        }
        [Fact]
        public void StartGame_ShouldInitializeGameCorrectly()
        {
            var gameService = new Game();
            gameService.CurrentPlayer.Name = GenerateRandomName();
            gameService.StartGame();


            Assert.Equal(0, gameService.CurrentPlayer.Score);
            Assert.Equal(750, gameService.Config.GameSpeed);
            Assert.Equal(16, gameService.CurrCellManager.Cells.Count);
            Assert.True(gameService.State.IsGameStarted);
            Assert.True(string.IsNullOrEmpty(gameService.State.Message));
            Assert.False(gameService.State.ShowGameOverModal);
            Assert.NotNull(gameService.Config.GameLoopTimer);
            Assert.NotNull(gameService.Config.GameTimeTimer);
            Assert.Equal(60, gameService.State.CurrentTime);
            Assert.True(gameService.State.IsGameRunning);
        }

        [Fact]
        public async Task EndGame_ShouldTerminateGameCorrectly()
        {
            // Arrange
            var mockGame = new Mock<Game>
            {
                CallBase = true // Ensures the actual logic is used except for mocked methods
            };
            mockGame.Object.CurrentPlayer.Name = GenerateRandomName();
            mockGame.Object.StartGame();
            mockGame.Setup(x => x.SendScoreToServer(It.IsAny<int>())).Returns(Task.CompletedTask); // Mock the method

            // Act
            await mockGame.Object.EndGame();

            // Assert
            Assert.False(mockGame.Object.State.IsGameRunning);
            Assert.True(mockGame.Object.State.ShowGameOverModal);
            mockGame.Verify(x => x.SendScoreToServer(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void SetNextAppearance_ShouldSetNextMoleAppearanceCorrectly()
        {
            var gameService = new Game();
            gameService.CurrentPlayer.Name = GenerateRandomName();

            gameService.StartGame();

            gameService.setNextAppearance();
            int currentMoleIndex = gameService.State.HitPosition;
            Assert.True(gameService.CurrCellManager.Cells[currentMoleIndex].IsShown);

            gameService.setNextAppearance();
            int newMoleIndex = gameService.State.HitPosition;

            Assert.False(gameService.CurrCellManager.Cells[currentMoleIndex].IsShown);
            Assert.True(gameService.CurrCellManager.Cells[newMoleIndex].IsShown);
        }

        [Fact]
        public void GameTimeAsync_ShouldEndGameCorrectly()
        {
            // Arrange
            var _gameComponent = new Mock<Game>() { CallBase = true };
            _gameComponent.Object.State.IsGameRunning = true; 
            _gameComponent.Object.State.CurrentTime = 0; 

            _gameComponent.Setup(m => m.EndGame()).Verifiable();

            // Act
            _gameComponent.Object.GameTimeAsync(new PeriodicTimer(TimeSpan.FromSeconds(1)));

            // Assert
            _gameComponent.Verify(m => m.EndGame(), Times.Once); 
            Assert.False(_gameComponent.Object.State.IsGameRunning);
            Assert.True(_gameComponent.Object.State.ShowGameOverModal); 
        }

        [Fact]
        public void RestartGame_ResetsGameState()
        {
            var cut = RenderComponent<Game>();
            cut.Instance.StartGame();
            cut.Instance.CurrentPlayer.Name = GenerateRandomName();
            cut.Instance.EndGame();

            cut.Instance.RestartGame();

            Assert.True(cut.Instance.State.IsGameStarted);
            Assert.True(cut.Instance.State.IsGameRunning);
            Assert.Equal(0, cut.Instance.CurrentPlayer.Score);
            Assert.Equal(60, cut.Instance.State.CurrentTime);
            Assert.False(cut.Instance.State.ShowGameOverModal);
        }

        [Fact]
        public void ReturnToStartMenu_SetsCorrectState()
        {
            var cut = RenderComponent<Game>();
            cut.Instance.StartGame();
            cut.Instance.CurrentPlayer.Name = GenerateRandomName();
            cut.Instance.EndGame();

            cut.Instance.ReturnToStartMenu();

            Assert.False(cut.Instance.State.IsGameStarted);
            Assert.False(cut.Instance.State.ShowGameOverModal);
        }

    }
}

