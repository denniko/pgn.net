using System;
using System.Collections.Generic;
using System.Text;

namespace ilf.pgn.Data
{
    public static class MoveHelper
    {
        static bool CheckPawnMoveAndApply(Move move, BoardSetup board)
        {
            var file = move.TargetSquare.File;
            var fileInt = (int)file;
            var rank = move.TargetSquare.Rank;
            Color color = board.IsWhiteMove ? Color.White : Color.Black;
            int direction = color == Color.White ? 1 : -1;
            int pawnLine = color == Color.White ? 2 : 7;
            var targetPiece = board[move.TargetSquare];
            var ppc = move.PromotedPiece == null || (move.PromotedPiece != PieceType.King
                && move.PromotedPiece != PieceType.Pawn);
            var isCorrectPawnArea = color == Color.White && rank >= 2 && rank <= 8
                || color == Color.Black && rank >= 1 && rank <= 7;
            if (move.Type == MoveType.Simple)
            {              
                bool isCorrect = targetPiece == null && ppc && isCorrectPawnArea
                    && (board[file, rank - direction]?.PieceType == PieceType.Pawn
                    && board[file, rank - direction]?.Color == color)
                    || (board[file, rank - direction] == null
                    && board[file, rank - 2 * direction]?.PieceType == PieceType.Pawn
                    && board[file, rank - 2 * direction]?.Color == color
                    && rank - 2 * direction == pawnLine);
            } 
            else if (move.Type == MoveType.Capture)
            {
                return ppc && targetPiece != null && color != targetPiece.Color
                    && isCorrectPawnArea
                    && (fileInt > 1 && board[fileInt - 1, rank - 1]?.Color == color 
                    && board[fileInt - 1, rank - 1]?.PieceType == PieceType.Pawn                   
                    || fileInt < 8 && board[fileInt + 1, rank - 1]?.Color == color
                    && board[fileInt + 1, rank - 1]?.PieceType == PieceType.Pawn);
            } else if (move.Type == MoveType.CaptureEnPassant)
            {
                throw new NotImplementedException();
            }
            throw new InvalidOperationException();
        }
    }
}
