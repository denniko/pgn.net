using ilf.pgn.Data;
using System;
using System.Linq;
using System.Text;

namespace pgn.Data
{
    public static class GameExtension
    {
        public static GoToMoveResult GoToMove(this Game game, int halfmove = int.MaxValue)
        {
            var board = game.BoardSetup.Clone();
            int cnt = 0;
            var sbMoves = new StringBuilder();
            foreach (var move in game.MoveText.GetMoves())
            {
                cnt++;
                var vmoves = board.ValidateMove(move);
                if (vmoves != null)
                {
                    foreach (var vmove in vmoves)
                    {
                        if (vmove.TargetSquare != null)
                        {
                            board[vmove.TargetSquare] = board[vmove.OriginSquare];
                        }
                        board[vmove.OriginSquare] = null;
                    }
                    sbMoves.Append(vmoves.First().ToUciString());
                }
                board.IsWhiteMove = !board.IsWhiteMove;
                if (cnt >= halfmove)
                    break;
            }

            return new GoToMoveResult
            {
                Setup = board,
            };
        }

        public static Move[] ValidateMove(this BoardSetup board, Move move)
        {
            if (move.Type == MoveType.CastleKingSide || move.Type == MoveType.CastleQueenSide)
            {
                return MoveHelper.ValidateCastle(move, board);
            }
            else
            {
                switch (move.Piece)
                {
                    case PieceType.Pawn:
                        return new[] { MoveHelper.ValidatePawnMove(move, board) };
                    case PieceType.Knight:
                        return new[] { MoveHelper.ValidateKnightMove(move, board) };
                    case PieceType.Bishop:
                        return new[] { MoveHelper.ValidateBishopMove(move, board) };
                    case PieceType.Rook:
                        return new[] { MoveHelper.ValidateRookMove(move, board) };
                    case PieceType.Queen:
                        return new[] { MoveHelper.ValidateQueenMove(move, board) };
                    case PieceType.King:
                        return new[] { MoveHelper.ValidateKingMove(move, board) };
                    default:
                        return null;
                }
            }
        }
    }

    public class GoToMoveResult
    {
        public BoardSetup Setup { get; set; }
        public string UciMoves { get; set; }
    }
}
