using System.Runtime.InteropServices;
using ChessChallenge.API;
using System;


public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        int eval = Evaluate(board);
        return allMoves[0];

    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    int Evaluate(Board board) {
        int whiteEval = CountMaterial (board, true);
        int blackEval = CountMaterial (board, false);

        int evaluation = whiteEval - blackEval;

        int perspective = board.IsWhiteToMove ? 1 : -1;
        return evaluation * perspective;
    }

    int CountMaterial (Board board, bool colour) {
        int material = 0;
        material += board.GetPieceList(PieceType.Pawn, colour).Count * pieceValues[(int)PieceType.Pawn];
        material += board.GetPieceList(PieceType.Knight, colour).Count * pieceValues[(int)PieceType.Knight];
        material += board.GetPieceList(PieceType.Bishop, colour).Count * pieceValues[(int)PieceType.Bishop];
        material += board.GetPieceList(PieceType.Rook, colour).Count * pieceValues[(int)PieceType.Rook];
        material += board.GetPieceList(PieceType.Queen, colour).Count * pieceValues[(int)PieceType.Queen];

        return material;
    }
}