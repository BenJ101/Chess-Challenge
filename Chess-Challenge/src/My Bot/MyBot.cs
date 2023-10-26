using System.Runtime.InteropServices;
using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

//- bot stuck in repetition
//- need piece square table for evaluation
//- need alpha beta pruning
//- need sorting of evaluated moves to spee up alpha beta pruning

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    Move thinkmove = Move.NullMove;

    public Move Think(Board board, Timer timer)
    {
        // incorporate basic alpha beta search algorithm using basic evaluation function
        // basic evaluation function can be updated iteratively throughout 
        Move thinkmove = Move.NullMove;
        int eval = Search(3, 0, -10000, 10000, board);
        return thinkmove.IsNull ? board.GetLegalMoves()[0] :thinkmove;
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
        // To reduce number of tokens, evaluation could be done iterating over all pieces
        // This will also help to incorporate the piece square tables
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

    int Search(int depth, int ply, int alpha, int beta, Board board){
        
        if (ply > 0 && board.IsRepeatedPosition()) {
            return 0;
        }

        if (depth == 0) {
            return Evaluate(board);
        }

        Move[] allMoves = board.GetLegalMoves();
        if (allMoves.Length == 0) {
            if (board.IsInCheck()) {
                return -10000;
            }
            return 0;
        }

        int bestEval = -10000;
        Move bestMove = Move.NullMove;
        foreach(Move move in allMoves){
            board.MakeMove(move);
            int eval = -Search(depth - 1, ply + 1, -beta, -alpha, board);
            if (eval > bestEval){
                bestEval = eval;
                bestMove = move;
                if (ply == 0) thinkmove = move;
            }
            board.UndoMove(move);
        }

        return bestEval;
    }

    }
