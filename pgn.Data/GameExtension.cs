using ilf.pgn.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace pgn.Data
{
    public static class GameExtension
    {
        public static BoardSetup GoToMove(this Game game, int halfmove)
        {
            var board = game.BoardSetup.Clone();
            foreach (var move in game.MoveText.GetMoves())
            {
               
            }

            return board;
        }
    }
}
