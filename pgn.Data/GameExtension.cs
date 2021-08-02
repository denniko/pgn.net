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
            bool chess960 = true;
            if (game.BoardSetup == null && game.Tags.ContainsKey("FEN") == false)
            {
                chess960 = false;
                game.BoardSetup = BoardSetup.CreateDefault();
            }
            var board = game.BoardSetup.Clone();
            int cnt = 0;
            var sbMoves = new StringBuilder();
            foreach (var move in game.MoveText.GetMoves())
            {
                cnt++;
                var vmoves = board.ValidateMove(move);
                if (vmoves != null)
                {
                    if (move.Type == MoveType.CastleKingSide || move.Type == MoveType.CastleQueenSide)
                    {
                        var p0 = board[vmoves[0].OriginSquare];
                        var p1 = board[vmoves[1].OriginSquare];
                        board[vmoves[0].OriginSquare] = null;
                        board[vmoves[1].OriginSquare] = null;
                        board[vmoves[0].TargetSquare] = p0;
                        board[vmoves[1].TargetSquare] = p1;
                        if (chess960)
                        {
                            var king = vmoves.Single(vm => vm.Piece == PieceType.King);
                            var rook = vmoves.Single(vm => vm.Piece == PieceType.Rook);
                            sbMoves.Append(king.OriginSquare.ToString() + rook.OriginSquare.ToString());
                        } 
                        else
                        {
                            string from = board.IsWhiteMove ? "e1" : "e8";
                            string to;
                            if (move.Type == MoveType.CastleKingSide)
                                to = board.IsWhiteMove ? "g1" : "g8";
                            else
                                to = board.IsWhiteMove ? "c1" : "c8";
                            sbMoves.Append(from + to);
                        }
                    }
                    else
                    {
                        foreach (var vmove in vmoves)
                        {
                            if (vmove.TargetSquare != null)
                            {
                                board[vmove.TargetSquare] = board[vmove.OriginSquare];
                            }
                            if (vmove.TargetSquare != vmove.OriginSquare)
                            {
                                board[vmove.OriginSquare] = null;
                            }
                        }
                        sbMoves.Append(vmoves.First().ToUciString());
                    }
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

        public static string ConvertUciMoveToPgn(this Game game, string move)
        {
            var pos = game.GoToMove();
            var from = new Square((File)Enum.Parse(typeof(File), move[0].ToString(), true), 
                Convert.ToInt32(move[1].ToString()));
            var to = new Square((File)Enum.Parse(typeof(File), move[2].ToString(), true), 
                Convert.ToInt32(move[3].ToString()));
            var pieceFrom = pos.Board[from];
            if (pieceFrom.PieceType == PieceType.Pawn)
            {
                if (from.File == to.File) return to.ToString();
                return from.File.ToString().ToLower() + "x" + to.ToString();
            }
            else
            {
                return ((char)pieceFrom.PieceType).ToString() + from.ToString() + to.ToString();
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
