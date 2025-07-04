﻿using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GolfTeeGameEngine
{

    public static class Rules
    {
        //             0
        //           1   2
        //         3   4  5
        //        6  7  8  9
        //      10 11 12 13 14

        //  The key of the outer dictionary is the destination of all legal jumps (where empty holes are)
        //  The inner dictionary is keyed by the index of the cell being jumped from, and the value is the index of the cell being jumped over
        public static Dictionary<int, Dictionary<int, int>> LegalJumpsByDestination = new()
        {
            { 0,  new() { { 3, 1 }, { 5, 2 } } },
            { 1,  new() { { 6, 3 }, { 8, 4 } } },
            { 2,  new() { { 7, 4 }, { 9, 5 } } },
            { 3,  new() { { 0, 1 }, { 5, 4 }, { 10, 6 }, { 12, 7 } } },
            { 4,  new() { { 11, 7 }, { 13, 8 } } },
            { 5,  new() { { 0, 2 }, { 3, 4 }, { 12, 8 }, { 14, 9 } } },
            { 6,  new() { { 1, 3 }, { 8, 7 } } },
            { 7,  new() { { 2, 4 }, { 9, 8 } } },
            { 8,  new() { { 1, 4 }, { 6, 7 } } },
            { 9,  new() { { 2, 5 }, { 7, 8 } } },
            { 10, new() { { 3, 6 }, { 12, 11 } } },
            { 11, new() { { 4, 7 }, { 13, 12 } } },
            { 12, new() { { 3, 7 }, { 5, 8 }, { 10, 11 }, { 14, 13 } } },
            { 13, new() { { 4, 8 }, { 11, 12 } } },
            { 14, new() { { 5, 9 }, { 12, 13 } } },
        };

    }
    public class LegalJump
    { 
        public LegalJump()
        {
        }
        public LegalJump(int to, int from)
        {
            To = to;
            From = from;
        }
        public int To { get; set; }
        public int From { get; set; }
    }

    public class Board
    {
        private const int HoleCount = 15;
        public int MoveNum { get; }
        private int PegCount => CountPegsBits();
        
        // New game constructor
        public Board(int emptyPegHole)
        {
            MoveNum = 0;
            Pegs = 0;
            Jumps = new();
            for (int i = 0; i < HoleCount; i++)
            {
                if (i != emptyPegHole)
                    SetPegsBit(i);
            }
        }

        private void SetPegsBit(int n)
        {
            Pegs |= (ushort)(1 << n);
        }

        private void ClearPegsBit(int n)
        {
            Pegs &= (ushort)~(1 << n);
        }

        public bool TestPegsBit(int n)
        {
            return (Pegs & (1 << n)) != 0;
        }

        public int CountPegsBits()
        {
            int i = 0;
            ushort pegs = Pegs;
            while (pegs != 0)
            {
                i++;
                pegs &= (ushort)(pegs - 1);
            }
            return i;
        }

        public bool[] PegsToBoolArray()
        {
            bool[] result = new bool[HoleCount];

            for (int i = 0; i < HoleCount; i++)
            {
                result[i] = TestPegsBit(i);
            }
            return result;
        }

        // Existing state constructor
        public Board(bool[] pegs, List<LegalJump> jumps, int moveNum)
        {
            MoveNum = moveNum;

            Jumps = jumps.ToList();
            for (int i = 0; i < HoleCount  && i < pegs.Length; i++)
            {
                if (pegs[i])
                    SetPegsBit(i);
            }
        }

        // Copy constructor
        public Board(Board other)
        {
            MoveNum = other.MoveNum + 1;
            Jumps = new();
//            Jumps = other.Jumps.ToList(); Not needed for copy constructor cases, and it makes analysis slower.
            Pegs = other.Pegs;
        }

        private bool TryLegalToAndFrom(int to, int from, out int over)
        {
            return Rules.LegalJumpsByDestination[to].TryGetValue(from, out over);
        }

        private bool IsLegalJump(int to, int from, int over)
        {
            if (!TestPegsBit(to) && TestPegsBit(from) && TestPegsBit(over))
            {
                if (Rules.LegalJumpsByDestination[to][from] == over)
                {
                    return true;
                }
            }
            return false;
        }

        public List<LegalJump> LegalJumps()
        {
            var result = new List<LegalJump>();
            foreach (var (to, fromDict) in Rules.LegalJumpsByDestination)
            {
                foreach (var (from, over) in fromDict)
                {
                    if (IsLegalJump(to, from, over))
                    {
                        result.Add(new LegalJump(to, from));
                    }
                }
            }
            return result;
        }

        public bool Jump(int to, int from)
        {
            if (TryLegalToAndFrom(to, from, out int over) && IsLegalJump(to, from, over))
            {
                SetPegsBit(to);
                ClearPegsBit(from);
                ClearPegsBit(over);
                Jumps.Add(new LegalJump(to, from));
                return true;
            }
            return false;
        }

        private static readonly ConcurrentDictionary<ushort, int> _analyzeJumpsMemo = new();

        private int AnalyzeJumps()
        {
            if (_analyzeJumpsMemo.TryGetValue(Pegs, out int cached))
                return cached;

            var result = PegCount;
            var legalJumps = LegalJumps();

            foreach (var legalJump in legalJumps)
            {
                var board = new Board(this);
                if (board.Jump(legalJump.To, legalJump.From))
                {
                    int pegsLeft = board.AnalyzeJumps();
                    if (pegsLeft < result)
                    {
                        result = pegsLeft;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Expected jump from {legalJump.From} to {legalJump.To} to be legal on move {board.MoveNum}");
                }
            }

            _analyzeJumpsMemo[Pegs] = result;
            return result;
        }

        public List<(LegalJump, int)> AnalyzeLegalJumps()
        {
            var result = new List<(LegalJump, int)>();

            var legalJumps = LegalJumps();        
            
            foreach (var legalJump in legalJumps)
            {
                var board = new Board(this);
                if (board.Jump(legalJump.To, legalJump.From))
                {
                    result.Add((legalJump, board.AnalyzeJumps()));
                }
                else
                {
                    throw new InvalidOperationException($"Expected jump from {legalJump.From} to {legalJump.To} to be legal on move {MoveNum}");
                }
                board = this;
            }
            return result;
        }

        public bool Undo()
        {
            if (Jumps.Count > 0)
            {
                var lastJump = Jumps[Jumps.Count - 1];

                if (lastJump != null && TryLegalToAndFrom(lastJump.To, lastJump.From, out int over))
                {
                    ClearPegsBit(lastJump.To);
                    SetPegsBit(lastJump.From);
                    SetPegsBit(over);
                    Jumps.RemoveAt(Jumps.Count -1);
                    return true;
                }
            }
            return false;
        }

        public int BestPossibleResult()
        {
            return AnalyzeJumps();
        }

        public ushort Pegs;
        public List<LegalJump> Jumps;
    }
}
