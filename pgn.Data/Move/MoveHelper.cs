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

        public static Move ValidateKnightMove(Move move, BoardSetup board)
        {
            var rank = move.TargetSquare.Rank - 1;
            var ifile = (int)move.TargetSquare.File - 1;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            var targetPieceNotKing = board[move.TargetSquare]?.PieceType != PieceType.King;
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

        public static Move ValidateBishopMove(Move move, BoardSetup board, 
            PieceType piece = PieceType.Bishop)
        {
            var rank = move.TargetSquare.Rank - 1;
            var ifile = (int)move.TargetSquare.File - 1;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            var targetPieceNotKing = board[move.TargetSquare]?.PieceType != PieceType.King;
            Square original = null;
            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    if (board[f, r]?.PieceType == piece
                        && board[f, r]?.Color == color
                        && Math.Abs(ifile - f) == Math.Abs(rank - r) 
                        && targetPieceNotKing
                        && (move.OriginRank == null || move.OriginRank == (r + 1))
                        && (move.OriginFile == null || move.OriginFile == (File)(f + 1)))
                    {
                        int pathLenght = Math.Abs(ifile - f);
                        int df = Math.Sign(ifile - f);
                        int dr = Math.Sign(rank - r);
                        bool freePath = true;
                        for (int i=1; i<pathLenght; i++)
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
            throw new InvalidOperationException("No piece can make this move");
        }
    }
}
