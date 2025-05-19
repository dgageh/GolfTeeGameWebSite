using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GolfTeeGameEngine;

using GolfTeeGameWebApp.Models;

namespace GolfTeeGameWebApp.Pages
{
    public class GamePageModel : PageModel
    {
        // Bind the game state to the page so it can be received in POST requests.
        [BindProperty]
        public GameModel Game { get; set; }

        /* 
         * The initial GET loads a new game.
         * This action resets your game state to the starting conditions.
         */
        public void OnGet()
        {
            StartNewGame();
        }

        public IActionResult OnPostNewGame()
        {
            StartNewGame();
            return Page();
        }


        public IActionResult OnPostMakeMove(int to, int from)
        {
            // Reconstruct the Board from the client-supplied state.
            Board board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);

            board.Jump(to, from);

            // Update the game state from the board.
            UpdateGameState(board, false);
            return Page();
        }

        public IActionResult OnPostUndo()
        {
            Board board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            board.Undo();
            UpdateGameState(board, false);
            return Page();
        }

        public IActionResult OnPostShowHints()
        {
            Board board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            board.AnalyzeLegalJumps(); 
            UpdateGameState(board, true);
            return Page();
        }

        private void StartNewGame()
        {
            var random = new Random();
            int emptyPegHole = random.Next(16);
            var board = new Board(emptyPegHole); 

            UpdateGameState(board, false);
        }

        private void UpdateGameState(Board board, bool includeHints)
        {
            List<LegalJump> legalJumps = new();
            List<int> hints = new();
            if (!includeHints)
            {
                legalJumps = board.LegalJumps().ToList();
            }
            else
            {
                var movesWithHints = board.AnalyzeLegalJumps();
                legalJumps = movesWithHints.Select(t => t.Item1).ToList();
                hints = movesWithHints.Select(t => t.Item2).ToList();
            }

            Game = new GameModel
            {
                PegState = board.Pegs.Cast<bool>().ToList(),
                History = board.Jumps.ToList(),
                MoveNumber = board.MoveNum,
                PossibleMoves = legalJumps,
                Hints = hints,
            };
        }
    }
}
