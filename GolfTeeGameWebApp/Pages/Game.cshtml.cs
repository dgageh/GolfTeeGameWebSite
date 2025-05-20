using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GolfTeeGameEngine;
using GolfTeeGameWebApp.Models;
using System.Linq;

namespace GolfTeeGameWebApp.Pages
{
    public class GamePageModel : PageModel
    {
        // Bound game state – carries the full state from the hidden fields.
        [BindProperty]
        public GameModel Game { get; set; } = new GameModel();

        // Transient properties used for move operations.
        // MoveFrom is set when a user clicks a filled peg to select it.
        [BindProperty]
        public int? MoveFrom { get; set; }

        // Transient properties used for move operations.
        // HintMoveFrom is set when a user clicks a hint to make the move.
        [BindProperty]
        public int? HintMoveFrom { get; set; }

        // TargetPeg is set when the user clicks an empty legal hole.
        [BindProperty]
        public int? TargetPeg { get; set; }

        public void OnGet()
        {
            StartNewGame();
        }

        public IActionResult OnPostNewGame()
        {
            StartNewGame();
            return Page();
        }

        public IActionResult OnPostUndo()
        {
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            board.Undo();
            // Clear any transient move selections.
            MoveFrom = null;
            TargetPeg = null;
            HintMoveFrom = null;
            UpdateGameState(board, includeHints: false);
            return Page();
        }

        public IActionResult OnPostShowHints()
        {
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            board.AnalyzeLegalJumps();
            UpdateGameState(board, includeHints: true);
            return Page();
        }

        // Handler for when the user clicks on a filled legal peg to select it as the source.
        public IActionResult OnPostSelectFrom(int from)
        {
            // The entire game state is bound via the hidden fields,
            // so we just record the chosen source peg.
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            MoveFrom = from;
            UpdateGameState(board, includeHints: false);
            return Page();
        }

        // Handler for executing a move.
        public IActionResult OnPostMakeMove()
        {
            int? sourcePeg = HintMoveFrom ?? MoveFrom;
            // Both the source peg (MoveFrom) and the destination (TargetPeg) must be provided.
            if (!sourcePeg.HasValue || !TargetPeg.HasValue)
            {
                // Optionally, display an error message.
                return Page();
            }

            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            board.Jump(TargetPeg.Value, MoveFrom.Value);

            // Clear transient move data after the move.
            MoveFrom = null;
            TargetPeg = null;
            HintMoveFrom = null;

            UpdateGameState(board, includeHints: false);
            return Page();
        }

        // Creates a new game by instantiating a new Board.
        private void StartNewGame()
        {
            var random = new Random();
            int emptyPegHole = random.Next(16);
            var board = new Board(emptyPegHole);

            // Clear any move selection.
            MoveFrom = null;
            TargetPeg = null;
            UpdateGameState(board, includeHints: false);
        }

        // Updates the Game state in the model from the Board object.
        private void UpdateGameState(Board board, bool includeHints)
        {
            // Get the legal moves and, optionally, move hints.
            var legalJumps = (!includeHints)
                ? board.LegalJumps().ToList()
                : board.AnalyzeLegalJumps().Select(t => t.Item1).ToList();
            var hints = (!includeHints)
                ? new System.Collections.Generic.List<int>()
                : board.AnalyzeLegalJumps().Select(t => t.Item2).ToList();

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