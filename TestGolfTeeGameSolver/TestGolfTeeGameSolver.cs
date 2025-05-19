using GolfTeeGameEngine;

namespace TestGolfTeeGameSolver
{
    public class TestGolfTeeGame
    {
        [Theory]
        [InlineData(0, 2)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [InlineData(3 ,4)]
        [InlineData(4, 2)]
        [InlineData(5, 4)]
        [InlineData(6, 2)]
        [InlineData(7, 2)]
        [InlineData(8, 2)]
        [InlineData(9, 2)]
        [InlineData(10, 2)]
        [InlineData(11, 2)]
        [InlineData(12, 4)]
        [InlineData(13, 2)]
        [InlineData(14, 2)]
        public void TestAnalyzeGame(int emptyHole, int resultCount)
        {
            var b = new Board(emptyHole);

            var j = b.AnalyzeLegalJumps();

            Assert.Equal(j.Count, resultCount);
        }

        [Fact]
        public void NewGame_HasCorrectPegCount_AndEmptyHole()
        {
            int emptyHole = 0;
            var board = new Board(emptyHole);
            Assert.False(board.Pegs[emptyHole]);
            Assert.Equal(14, board.Pegs.Cast<bool>().Count(b => b));
        }

        [Fact]
        public void LegalJumps_ReturnsExpectedJumps_AtStart()
        {
            var board = new Board(0);
            var jumps = board.LegalJumps();
            // At start, only two legal jumps into hole 0: from 3 and 5
            Assert.Contains(jumps, j => j.To == 0 && j.From == 3);
            Assert.Contains(jumps, j => j.To == 0 && j.From == 5);
            Assert.Equal(2, jumps.Count);
        }

        [Fact]
        public void Jump_ValidMove_UpdatesBoardState()
        {
            var board = new Board(0);
            bool result = board.Jump(0, 3); // Legal jump at start
            Assert.True(result);
            Assert.True(board.Pegs[0]);
            Assert.False(board.Pegs[3]);
            Assert.False(board.Pegs[1]); // Peg at 1 should be removed (jumped over)
            Assert.Single(board.Jumps);
            Assert.Equal(0, board.Jumps[0].To);
            Assert.Equal(3, board.Jumps[0].From);
        }

        [Fact]
        public void Jump_InvalidMove_ReturnsFalse_AndNoChange()
        {
            var board = new Board(0);
            bool result = board.Jump(1, 2); // Not a legal jump at start
            Assert.False(result);
            // Board state unchanged
            Assert.False(board.Pegs[0]);
            Assert.True(board.Pegs[1]);
            Assert.True(board.Pegs[2]);
            Assert.Empty(board.Jumps);
        }

        [Fact]
        public void AnalyzeLegalJumps_ReturnsExpectedResults()
        {
            var board = new Board(0);
            var analysis = board.AnalyzeLegalJumps();
            // There are two possible moves at start, each should return a tuple (LegalJump, int)
            Assert.Equal(2, analysis.Count);
            foreach (var (jump, pegsLeft) in analysis)
            {
                Assert.IsType<LegalJump>(jump);
                Assert.InRange(pegsLeft, 1, 14); // Should be between 1 and 14 pegs left
            }
        }

        [Fact]
        public void Board_CopyConstructor_CreatesDeepCopy()
        {
            var board1 = new Board(0);
            board1.Jump(0, 3);
            var board2 = new Board(board1);
            Assert.NotSame(board1.Pegs, board2.Pegs);
            Assert.Equal(board1.Pegs.Cast<bool>(), board2.Pegs.Cast<bool>());
            Assert.Equal(board1.Jumps.Count, board2.Jumps.Count);
            Assert.Equal(board1.MoveNum + 1, board2.MoveNum);
        }

        [Fact]
        public void Board_ExistingStateConstructor_SetsStateCorrectly()
        {
            bool[] pegs = Enumerable.Repeat(true, 15).ToArray();
            pegs[5] = false;
            var jumps = new List<LegalJump> { new LegalJump(5, 2) };
            var board = new Board(pegs, jumps, 3);
            Assert.False(board.Pegs[5]);
            Assert.True(board.Pegs[2]);
            Assert.Single(board.Jumps);
            Assert.Equal(3, board.MoveNum);
        }
    }

}
