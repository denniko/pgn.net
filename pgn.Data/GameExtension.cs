using ilf.pgn.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace pgn.Data
{
    public static class GameExtension
    {
        public static BoardSetup GoToMove(this Game game, int halfmove = int.MaxValue)
        {
            var board = game.BoardSetup.Clone();
            int cnt = 0;
            foreach (var move in game.MoveText.GetMoves())
            {
                cnt++;
                var vmove = board.ValidateMove(move);
                if (vmove != null)
                {
                    board[vmove.TargetSquare] = board[vmove.OriginSquare];
                    board[vmove.OriginSquare] = null;
                }
                board.IsWhiteMove = !board.IsWhiteMove;
                if (cnt >= halfmove)
                    break;
            }

            return board;
        }

        public static Move ValidateMove(this BoardSetup board, Move move)
        {
            if (move.Type == MoveType.CastleKingSide || move.Type == MoveType.CastleQueenSide)
            {

            }
            else
            {
                switch (move.Piece)
                {
                    case PieceType.Pawn:
                        return MoveHelper.ValidatePawnMove(move, board);
                    case PieceType.Knight:
                        return MoveHelper.ValidateKnightMove(move, board);
                    case PieceType.Bishop:
                        return MoveHelper.ValidateBishopMove(move, board);
                    case PieceType.Rook:
                        return MoveHelper.ValidateRookMove(move, board);
                    case PieceType.Queen:
                        return MoveHelper.ValidateQueenMove(move, board);
                    case PieceType.King:
                        return MoveHelper.ValidateKingMove(move, board);
                    default:
                        return null;
                }
            }
        }
    }
}
