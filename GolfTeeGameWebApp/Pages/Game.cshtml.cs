using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GolfTeeGameEngine;

public class GameModel : PageModel
{
    public Board CurrentBoard { get; set; }
    public List<(LegalJump, int)> PossibleMoves { get; set; } = new();

    [BindProperty]
    public int MoveFrom { get; set; }
    [BindProperty]
    public int MoveTo { get; set; }

    public void OnGet()
    {
        CurrentBoard = new Board(emptyPegHole: 0); // Initialize board
        PossibleMoves = CurrentBoard.AnalyzeLegalJumps();
    }

    public IActionResult OnPost()
    {
        if (CurrentBoard.Jump(MoveTo, MoveFrom))
        {
            PossibleMoves = CurrentBoard.AnalyzeLegalJumps();
        }
        return RedirectToPage("Game"); // Refresh game state
    }
}