using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GolfTeeGameEngine;
using GolfTeeGameWebApp.Models;

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

        // TargetPeg is set when the user clicks an empty legal hole.
        [BindProperty]
        public int? TargetPeg { get; set; }

        // The index of the move when chosen from the move list
        [BindProperty]
        public int? ListMove { get; set; }

        [BindProperty]
        public bool ShowMoves { get; set; }

        [BindProperty]
        public bool ShowHints { get; set; }

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
            ListMove = null;
            UpdateGameState(board);
            return Page();
        }

        public IActionResult OnPostShowHints()
        {
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            UpdateGameState(board);
            return Page();
        }

        public IActionResult OnPostShowMoves()
        {
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            UpdateGameState(board);
            return Page();
        }

        // Handler for when the user clicks on a filled legal peg to select it as the source.
        public IActionResult OnPostSelectFrom(int from)
        {
            // The entire game state is bound via the hidden fields,
            // so we just record the chosen source peg.
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            MoveFrom = from;
            UpdateGameState(board);
            return Page();
        }

        // Handler for executing a move.
        public IActionResult OnPostMakeMove()
        {
            // Both the source peg (MoveFrom) and the destination (TargetPeg) must be provided.
            if (!MoveFrom.HasValue || !TargetPeg.HasValue)
            {
                // Optionally, display an error message.
                return Page();
            }

            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);
            board.Jump(TargetPeg.Value, MoveFrom.Value);

            // Clear transient move data after the move.
            MoveFrom = null;
            TargetPeg = null;

            UpdateGameState(board);
            return Page();
        }

        public IActionResult OnPostMoveFromList()
        {
            int sourcePeg;
            int destPeg;
            
            var board = new Board(Game.PegState.ToArray(), Game.History, Game.MoveNumber);

            var legalJumps = board.LegalJumps();

            // If a button from the move list was clicked, use the hint index to get the move
            if (ListMove.HasValue && ListMove.Value < legalJumps.Count)
            {
                sourcePeg = legalJumps[ListMove.Value].From;
                destPeg = legalJumps[ListMove.Value].To;
            }
            else
            {
                return Page();
            }


            board.Jump(destPeg, sourcePeg);

            // Clear transient move data after the move.
            MoveFrom = null;
            TargetPeg = null;
            ListMove = null;

            UpdateGameState(board);
            return Page();
        }



        // Creates a new game by instantiating a new Board.
        private void StartNewGame()
        {
            var random = new Random();
            int emptyPegHole = random.Next(15);
            var board = new Board(emptyPegHole);

            // Clear any move selection.
            MoveFrom = null;
            TargetPeg = null;
            ListMove = null;

            UpdateGameState(board);
        }

        // Updates the Game state in the model from the Board object.
        private void UpdateGameState(Board board)
        {
            // Get the legal moves and, optionally, move hints.
            var legalJumps = (!ShowHints)
                ? board.LegalJumps().ToList()
                : board.AnalyzeLegalJumps().Select(t => t.Item1).ToList();
            var hints = (!ShowHints)
                ? new System.Collections.Generic.List<int>()
                : board.AnalyzeLegalJumps().Select(t => t.Item2).ToList();

            Game = new GameModel
            {
                PegState = board.PegsToBoolArray().ToList(),
                History = board.Jumps.ToList(),
                MoveNumber = board.MoveNum,
                PossibleMoves = legalJumps,
                Hints = hints,
                BestResult = board.BestPossibleResult()
            };
        }
    }
}