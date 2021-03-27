using System;
using System.Collections.Generic;
using System.Linq;

namespace ilf.pgn.Data
{
    public static class MoveHelper
    {
        public static Move ValidatePawnMove(Move move, BoardSetup board)
        {
            var file = move.TargetSquare.File;
            var rank = move.TargetSquare.Rank;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            int direction = color == Color.White ? 1 : -1;
            int pawnLine = color == Color.White ? 2 : 7;
            var targetPiece = board[move.TargetSquare];
            var ppc = move.PromotedPiece == null || (move.PromotedPiece != PieceType.King
                && move.PromotedPiece != PieceType.Pawn);
            var isCorrectPawnArea = color == Color.White && rank >= 2 && rank <= 8
                || color == Color.Black && rank >= 1 && rank <= 7;
            var result = move.Clone();
            if (move.Type == MoveType.Simple)
            {
                if (targetPiece == null && ppc && isCorrectPawnArea == false)
                    throw new InvalidOperationException();
                if (board[file, rank - direction]?.PieceType == PieceType.Pawn && board[file, rank - direction]?.Color == color)
                {
                    result.OriginFile = file;
                    result.OriginRank = rank - direction;
                    result.OriginSquare = new Square(result.OriginFile.Value, result.OriginRank.Value);
                    return result;
                }
                else if (board[file, rank - direction] == null && board[file, rank - 2 * direction]?.PieceType == PieceType.Pawn
                    && board[file, rank - 2 * direction]?.Color == color && rank - 2 * direction == pawnLine)
                {
                    result.OriginFile = file;
                    result.OriginRank = rank - 2 * direction;
                    result.OriginSquare = new Square(result.OriginFile.Value, result.OriginRank.Value);
                    return result;
                }
            }
            else if (move.Type == MoveType.Capture)
            {
                if (ppc && targetPiece != null && color != targetPiece.Color && isCorrectPawnArea && move.OriginFile.HasValue == false)
                    throw new InvalidOperationException();
                if (board[move.OriginFile.Value, rank - direction]?.Color == color
                    && board[move.OriginFile.Value, rank - direction]?.PieceType == PieceType.Pawn)
                {
                    result.OriginRank = rank - direction;
                    result.OriginSquare = new Square(result.OriginFile.Value, result.OriginRank.Value);
                    return result;
                }
            }
            else if (move.Type == MoveType.CaptureEnPassant)
            {
                throw new NotImplementedException();
            }
            throw new InvalidOperationException("No piece can make this move");
        }

        public static Move[] ValidateCastle(Move move, BoardSetup board)
        {
            // Не проверяются битые поля на пути короля и то что ладья та самая
            bool kingSide = move.Type == MoveType.CastleKingSide;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            if (color == Color.White && (kingSide == true && board.CanWhiteCastleKingSide == false
                || kingSide == false && board.CanWhiteCastleQueenSide == false)
                || color == Color.Black && (kingSide == true && board.CanBlackCastleKingSide == false
                || kingSide == false && board.CanBlackCastleQueenSide == false))
            {
                throw new InvalidOperationException("This castling is no longer possible");
            }
            int df = kingSide ? 1 : -1;
            int rank = board.IsWhiteMove ? 0 : 7;
            for (int f = 0; f < 8; f++)
            {
                if (board[f, rank]?.PieceType == PieceType.King && board[f, rank]?.Color == color)
                {
                    for (int i = 1; i < 6; i++)
                    {
                        int nf = f + i * df;
                        if (nf < 0 || nf > 7)
                            break;
                        if (board[nf, rank]?.PieceType == PieceType.Rook 
                            && board[nf, rank]?.Color == color)
                        {
                            var km = new Move();
                            km.Piece = PieceType.King;
                            km.Type = MoveType.Simple;
                            km.OriginSquare = new Square((File)(f + 1), rank + 1);
                            km.OriginFile = km.OriginSquare.File;
                            km.OriginRank = km.OriginSquare.Rank;
                            km.TargetSquare = kingSide ? new Square(File.G, rank) : new Square(File.C, rank);
                            km.TargetFile = km.TargetSquare.File;
                            var rm = new Move();
                            rm.Piece = PieceType.Rook;
                            rm.Type = MoveType.Simple;
                            rm.OriginSquare = new Square((File)(nf + 1), rank + 1);
                            rm.OriginFile = rm.OriginSquare.File;
                            rm.OriginRank = rm.OriginSquare.Rank;
                            rm.TargetSquare = kingSide ? new Square(File.F, rank) : new Square(File.D, rank);
                            rm.TargetFile = rm.TargetSquare.File;
                            return new[] { km, rm };
                        }
                        else if (board[nf, rank] != null)
                        {
                            throw new InvalidOperationException("A figure interferes with castling");
                        }
                    }
                    break;
                }
            }
            throw new InvalidOperationException("No castling rook found");
        }

        public static Move ValidateKnightMove(Move move, BoardSetup board)
        {
            var rank = move.TargetSquare.Rank - 1;
            var ifile = (int)move.TargetSquare.File - 1;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            var targetPieceNotKing = board[move.TargetSquare]?.PieceType != PieceType.King
                && board[move.TargetSquare]?.Color != color;
            Square original = null;
            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    if (board[f, r]?.PieceType == PieceType.Knight && board[f, r]?.Color == color
                        && Math.Abs(Math.Abs(ifile - f) - Math.Abs(rank - r)) == 1 && targetPieceNotKing
                        && (move.OriginRank == null || move.OriginRank == (r + 1))
                        && (move.OriginFile == null || move.OriginFile == (File)(f + 1)))
                    {
                        if (original == null)
                        {
                            original = new Square((File)(f + 1), r + 1);
                        }
                        else
                        {
                            throw new InvalidOperationException("More than one piece can make a given move");
                        }
                    }
                }
            }
            if (original != null)
            {
                var result = move.Clone();
                result.OriginSquare = original;
                result.OriginFile = original.File;
                result.OriginRank = original.Rank;
                return result;
            }
            throw new InvalidOperationException("No piece can make this move");
        }

        public static Move ValidateQueenMove(Move move, BoardSetup board)
        {
            var asRook = ValidatePieceMove(move, board, PieceType.Queen, PieceType.Rook);
            var asBishop = ValidatePieceMove(move, board, PieceType.Queen, PieceType.Bishop);
            if (asBishop == null && asRook == null)
                throw new InvalidOperationException("No piece can make this move");
            else
                return asRook ?? asBishop;
        }

        public static Move ValidateRookMove(Move move, BoardSetup board)
        {
            var asRook = ValidatePieceMove(move, board, PieceType.Rook, PieceType.Rook);
            if (asRook == null)
                throw new InvalidOperationException("No piece can make this move");
            else
                return asRook;
        }

        public static Move ValidateBishopMove(Move move, BoardSetup board)
        {
            var asBishop = ValidatePieceMove(move, board, PieceType.Bishop, PieceType.Bishop);
            if (asBishop == null)
                throw new InvalidOperationException("No piece can make this move");
            else
                return asBishop;
        }

        public static Move ValidateKingMove(Move move, BoardSetup board)
        {
            var asKing = ValidatePieceMove(move, board, PieceType.King, PieceType.King);
            if (asKing == null)
                throw new InvalidOperationException("No piece can make this move");
            else
                return asKing;
        }

        static Move ValidatePieceMove(Move move, BoardSetup board,
            PieceType piece, PieceType pieceType)
        {
            var rank = move.TargetSquare.Rank - 1;
            var ifile = (int)move.TargetSquare.File - 1;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            var targetPieceNotKing = board[move.TargetSquare]?.PieceType != PieceType.King
                && board[move.TargetSquare]?.Color != color;
            Func<int, int, bool> pieceCondition;
            if (pieceType == PieceType.Bishop)
                pieceCondition = (f, r) => Math.Abs(ifile - f) == Math.Abs(rank - r);
            else if (pieceType == PieceType.Rook)
                pieceCondition = (f, r) => (ifile == f || rank == r);
            else if (pieceType == PieceType.King)
                pieceCondition = (f, r) => Math.Abs(ifile - f) <= 1 && Math.Abs(rank - r) <= 1;
            else
                throw new InvalidOperationException("Invalid piece was passed to func");
            Square original = null;
            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    if (board[f, r]?.PieceType == piece
                        && board[f, r]?.Color == color
                        && pieceCondition(f, r)
                        && targetPieceNotKing
                        && (move.OriginRank == null || move.OriginRank == (r + 1))
                        && (move.OriginFile == null || move.OriginFile == (File)(f + 1)))
                    {
                        int pathLenght = Math.Abs(ifile - f);
                        int df = Math.Sign(ifile - f);
                        int dr = Math.Sign(rank - r);
                        bool freePath = true;
                        for (int i = 1; i < pathLenght; i++)
                        {
                            if (board[f + df * i, r + dr * i] != null)
                            {
                                freePath = false;
                                break;
                            }
                        }
                        if (freePath == true && original == null)
                        {
                            original = new Square((File)(f + 1), r + 1);
                        }
                        else
                        {
                            throw new InvalidOperationException("More than one piece can make a given move");
                        }
                    }
                }
            }
            if (original != null)
            {
                var result = move.Clone();
                result.OriginSquare = original;
                result.OriginFile = original.File;
                result.OriginRank = original.Rank;
                return result;
            }
            return null;
        }
    }
}
