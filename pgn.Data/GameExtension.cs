using ilf.pgn.Data;
using System;
using System.Linq;
using System.Text;

namespace ilf.pgn.Data
{
    public static class GameExtension
    {
        public static Position GoToMove(this Game game, int halfmove = int.MaxValue)
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
                    sbMoves.Append(" ");
                }
                board.IsWhiteMove = !board.IsWhiteMove;
                if (cnt >= halfmove)
                    break;
            }

            return new Position
            {
                Board = board,
                UciMoves = sbMoves.ToString(),
                HalfMove = cnt
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

        public static WorthInPawn CalcWorthInPawns(this BoardSetup board)
        {
            var res = new WorthInPawn();
            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    var p = board[f, r];
                    if (p == null)
                        continue;
                    if (p.Color == Color.White)
                        res.White += p.PieceType.WorthInPawns();
                    else
                        res.Black += p.PieceType.WorthInPawns();
                }
            }
            return res;
        }

        public static int WorthInPawns(this PieceType piece)
        {
            switch (piece)
            {
                case PieceType.Pawn:
                    return 1;
                case PieceType.Bishop:
                case PieceType.Knight:
                    return 3;
                case PieceType.Rook:
                    return 5;
                case PieceType.Queen:
                    return 9;
                case PieceType.King:
                    return 0;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public class Position
    {
        public BoardSetup Board { get; set; }
        public string UciMoves { get; set; }
        public int HalfMove { get; set; }
    }

    public class WorthInPawn
    {
        public int White { get; set; }
        public int Black { get; set; }
    }
}
