using GolfTeeGameEngine;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace GolfTeeGameWebApp.Models
{
    public class GameModel
    {
        public List<(LegalJump, int)> PossibleMovesWithHints { get; set; }
        public List<LegalJump> PossibleMoves { get; set; }
        public List<LegalJump> History { get; set; }
        public List<bool> PegState { get; set; }
        public List<int> Hints { get; set; }
        public int MoveNumber { get; set; }
        public GameModel()
        {
            PossibleMovesWithHints = new();
            PossibleMoves = new();
            History = new();
            PegState = new();
            Hints = new();
            MoveNumber = 0;
        }
    }
}
