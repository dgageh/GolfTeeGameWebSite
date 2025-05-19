using GolfTeeGameEngine;

namespace GolfTeeGameWebApp.Models
{
    
    using System.Collections.Generic;

    public class GameModel
    {
        public Board CurrentBoard { get; set; }
        public List<(LegalJump, int)> PossibleMoves { get; set; } = new();

        public GameModel()
        {
            CurrentBoard = new Board(emptyPegHole: 0); // Initialize board
            PossibleMoves = CurrentBoard.AnalyzeLegalJumps();
        }

        public void MakeMove(int moveFrom, int moveTo)
        {
            if (CurrentBoard.Jump(moveTo, moveFrom))
            {
                PossibleMoves = CurrentBoard.AnalyzeLegalJumps();
            }
        }
    }
}
