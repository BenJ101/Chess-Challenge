﻿using ChessChallenge.API;
using System;


// Completed - bot stuck in repetition (thinkmove was only being augmented in local space)
//              still some 50 move rules endings being achieved
// Completed - alpha beta pruning (using negmax, achieve near double depth)
// Completed - sorting of evaluated moves to speed up alpha beta pruning
// Completed - Quiescence search to avoid horizon effect

//-  piece square table for evaluation
    //- to encode piece square tables to reduce tokens used
    //- to decode the the encoded piece square tables and return values
//- transpoistion table to help speed up evaluation of positions and reduce repetition
//- iterative deepening
    //  - ordering of moves needed to ensure first move evaluated at next depth is best move from previous
    //    this eliminates the wrong best move being returned if the search is stopped early
//- end game evaluations to reweight material


public class MyBot : IChessBot
{
    public Move thinkmove = Move.NullMove;

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 }; // could be combined into piece square tables but only a small save in variables

    // https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
    // Piece square tables for evaluation - need to be encoded into sub arrays to reduce tokens used
    // encode & decode can be implemnted after the piece square tables are implemented in evaluation
    static int[] mg_pawn_table = {
      0,   0,   0,   0,   0,   0,  0,   0,
     98, 134,  61,  95,  68, 126, 34, -11,
     -6,   7,  26,  31,  65,  56, 25, -20,
    -14,  13,   6,  21,  23,  12, 17, -23,
    -27,  -2,  -5,  12,  17,   6, 10, -25,
    -26,  -4,  -4, -10,   3,   3, 33, -12,
    -35,  -1, -20, -23, -15,  24, 38, -22,
      0,   0,   0,   0,   0,   0,  0,   0,
    };

    static int[] eg_pawn_table = {
        0,   0,   0,   0,   0,   0,   0,   0,
        178, 173, 158, 134, 147, 132, 165, 187,
        94, 100,  85,  67,  56,  53,  82,  84,
        32,  24,  13,   5,  -2,   4,  17,  17,
        13,   9,  -3,  -7,  -7,  -8,   3,  -1,
        4,   7,  -6,   1,   0,  -5,  -1,  -8,
        13,   8,   8,  10,  13,   0,   2,  -7,
        0,   0,   0,   0,   0,   0,   0,   0,
    };

    static int[] mg_knight_table = {
        -167, -89, -34, -49,  61, -97, -15, -107,
        -73, -41,  72,  36,  23,  62,   7,  -17,
        -47,  60,  37,  65,  84, 129,  73,   44,
        -9,  17,  19,  53,  37,  69,  18,   22,
        -13,   4,  16,  13,  28,  19,  21,   -8,
        -23,  -9,  12,  10,  19,  17,  25,  -16,
        -29, -53, -12,  -3,  -1,  18, -14,  -19,
        -105, -21, -58, -33, -17, -28, -19,  -23,
    };

    static int[] eg_knight_table = {
        -58, -38, -13, -28, -31, -27, -63, -99,
        -25,  -8, -25,  -2,  -9, -25, -24, -52,
        -24, -20,  10,   9,  -1,  -9, -19, -41,
        -17,   3,  22,  22,  22,  11,   8, -18,
        -18,  -6,  16,  25,  16,  17,   4, -18,
        -23,  -3,  -1,  15,  10,  -3, -20, -22,
        -42, -20, -10,  -5,  -2, -20, -23, -44,
        -29, -51, -23, -15, -22, -18, -50, -64,
    };

    static int[] mg_bishop_table = {
        -29,   4, -82, -37, -25, -42,   7,  -8,
        -26,  16, -18, -13,  30,  59,  18, -47,
        -16,  37,  43,  40,  35,  50,  37,  -2,
        -4,   5,  19,  50,  37,  37,   7,  -2,
        -6,  13,  13,  26,  34,  12,  10,   4,
        0,  15,  15,  15,  14,  27,  18,  10,
        4,  15,  16,   0,   7,  21,  33,   1,
        -33,  -3, -14, -21, -13, -12, -39, -21,
    };

    static int[] eg_bishop_table = {
        -14, -21, -11,  -8, -7,  -9, -17, -24,
        -8,  -4,   7, -12, -3, -13,  -4, -14,
        2,  -8,   0,  -1, -2,   6,   0,   4,
        -3,   9,  12,   9, 14,  10,   3,   2,
        -6,   3,  13,  19,  7,  10,  -3,  -9,
        -12,  -3,   8,  10, 13,   3,  -7, -15,
        -14, -18,  -7,  -1,  4,  -9, -15, -27,
        -23,  -9, -23,  -5, -9, -16,  -5, -17,
    };

    static int[] mg_rook_table = {
        32,  42,  32,  51, 63,  9,  31,  43,
        27,  32,  58,  62, 80, 67,  26,  44,
        -5,  19,  26,  36, 17, 45,  61,  16,
        -24, -11,   7,  26, 24, 35,  -8, -20,
        -36, -26, -12,  -1,  9, -7,   6, -23,
        -45, -25, -16, -17,  3,  0,  -5, -33,
        -44, -16, -20,  -9, -1, 11,  -6, -71,
        -19, -13,   1,  17, 16,  7, -37, -26,
    };

    static int[] eg_rook_table = {
        13, 10, 18, 15, 12,  12,   8,   5,
        11, 13, 13, 11, -3,   3,   8,   3,
        7,  7,  7,  5,  4,  -3,  -5,  -3,
        4,  3, 13,  1,  2,   1,  -1,   2,
        3,  5,  8,  4, -5,  -6,  -8, -11,
        -4,  0, -5, -1, -7, -12,  -8, -16,
        -6, -6,  0,  2, -9,  -9, -11,  -3,
        -9,  2,  3, -1, -5, -13,   4, -20,
    };

    static int[] mg_queen_table = {
        -28,   0,  29,  12,  59,  44,  43,  45,
        -24, -39,  -5,   1, -16,  57,  28,  54,
        -13, -17,   7,   8,  29,  56,  47,  57,
        -27, -27, -16, -16,  -1,  17,  -2,   1,
        -9, -26,  -9, -10,  -2,  -4,   3,  -3,
        -14,   2, -11,  -2,  -5,   2,  14,   5,
        -35,  -8,  11,   2,   8,  15,  -3,   1,
        -1, -18,  -9,  10, -15, -25, -31, -50,
    };

    static int[] eg_queen_table = {
        -9,  22,  22,  27,  27,  19,  10,  20,
        -17,  20,  32,  41,  58,  25,  30,   0,
        -20,   6,   9,  49,  47,  35,  19,   9,
        3,  22,  24,  45,  57,  40,  57,  36,
        -18,  28,  19,  47,  31,  34,  39,  23,
        -16, -27,  15,   6,   9,  17,  10,   5,
        -22, -23, -30, -16, -16, -23, -36, -32,
        -33, -28, -22, -43,  -5, -32, -20, -41,
    };

    static int[] mg_king_table = {
        -65,  23,  16, -15, -56, -34,   2,  13,
        29,  -1, -20,  -7,  -8,  -4, -38, -29,
        -9,  24,   2, -16, -20,   6,  22, -22,
        -17, -20, -12, -27, -30, -25, -14, -36,
        -49,  -1, -27, -39, -46, -44, -33, -51,
        -14, -14, -22, -46, -44, -30, -15, -27,
        1,   7,  -8, -64, -43, -16,   9,   8,
        -15,  36,  12, -54,   8, -28,  24,  14,
    };

    static int[] eg_king_table = {
        -74, -35, -18, -18, -11,  15,   4, -17,
        -12,  17,  14,  17,  17,  38,  23,  11,
        10,  17,  23,  15,  20,  45,  44,  13,
        -8,  22,  24,  27,  26,  33,  26,   3,
        -18,  -4,  21,  24,  27,  23,   9, -11,
        -19,  -3,  11,  21,  23,  16,   7,  -9,
        -27, -11,   4,  13,  14,   4,  -5, -17,
        -53, -34, -21, -11, -28, -14, -24, -43
    }; 

    // list of all mg tables
    int[][] mg_tables = { mg_pawn_table, mg_knight_table, mg_bishop_table, mg_rook_table, mg_queen_table, mg_king_table };
    // list of all eg tables
    int[][] eg_tables = { eg_pawn_table, eg_knight_table, eg_bishop_table, eg_rook_table, eg_queen_table, eg_king_table };

    int[] piece_phase = { 0, 0, 1, 1, 2, 4, 0 }; // 0 for king, 1 for pawn, 2 for knight and bishop, 4 for rook, 0 for queen
    

    int Evaluate(Board board) {
        // To reduce number of tokens, evaluation could be done iterating over all pieces
        // This will also help to incorporate the piece square tables
        
        int mg_eval = 0;
        int eg_eval = 0;
        
        int gamePhase = 0;

        // iterate over white and black pieces
        foreach (bool colour in new[] {true, false}) {
            // iterate over all pieces
            for (int i = 1; i <= 6; i++) {
                
                ulong evalPieces = board.GetPieceBitboard((PieceType)i, colour);
                
                // iterate over all eval pieces
                while (evalPieces != 0) {
                    int square = BitboardHelper.ClearAndGetIndexOfLSB(ref evalPieces);
                    // convert square to correct index for piece square table where 63 is 7, 0 is 56 etc for white
                    // and 63 is 56, 0 is 7 etc for black
                    // currently only needed as piece square tables are not encoded and in wrong order
                    if (colour)
                        square = ((7-((int)Math.Floor((double)square/8)))*8) + (square%8);
                    else
                        square = ((int)Math.Floor((double)square/8)*8) + (7-(square%8));


                    mg_eval += pieceValues[i] + mg_tables[i-1][square];
                    eg_eval += pieceValues[i] + eg_tables[i-1][square];
                    gamePhase += piece_phase[i];
                    
                }            

            }
            mg_eval = -mg_eval; 
            eg_eval = -eg_eval;
        }
        return (mg_eval * gamePhase + eg_eval * (24 - gamePhase)) / 24 * (board.IsWhiteToMove ? 1 : -1);

    }


    public int Search(int depth, int ply, int alpha, int beta, Board board, ref Move thinkmove, Timer timer){
        bool Quiescence_search = depth <= 0;
        int bestEval = -50000;

        // if draw by repetition
        if (ply > 0 && board.IsRepeatedPosition()) {
            return 0;
        }

        int stand_pat = Evaluate(board);

        if(Quiescence_search) {
            bestEval = stand_pat;
            if(bestEval >= beta) return bestEval;
            alpha = Math.Max(alpha, bestEval);
        }

        Move[] moves = board.GetLegalMoves(Quiescence_search);      


        // if bottom of search
        // if (depth == 0) {
        //     return Evaluate(board);  
        // }
        

        Move bestMove = Move.NullMove;

        // Score moves to improve pruning
        int[] scores = new int[moves.Length];
        for(int i = 0; i<moves.Length; i++){
            Move move = moves[i];
            if(move.IsCapture){
                scores[i] = 100*(int)move.CapturePieceType - (int)move.MovePieceType;
            }
        }
        
        for(int i = 0; i < moves.Length; i++){
            // return bad eval to cancel search if time is running out
            // if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 30000;    
            
            // sort moves by captures to promote pruning and reduce the time spent evaluating potentially worse positions
            for(int j = i + 1; j < moves.Length; j++) {
                if(scores[j] > scores[i])
                    (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
            }

            Move move = moves[i];
            board.MakeMove(move);
            int eval = -Search(depth - 1, ply + 1, -beta, -alpha, board, ref thinkmove, timer);
            board.UndoMove(move);
            
            if (eval > bestEval){
                bestEval = eval;
                bestMove = move;
                if (ply == 0) {
                    thinkmove = move;
                }
            }

            alpha = Math.Max(alpha, eval);

            if (alpha >= beta) {
                return beta;
            }

        }
        
        // if checkmate or stalemate
        if (moves.Length == 0 && !Quiescence_search) {
            if (board.IsInCheck()) {
                return -50000 + ply;
            }
            return 0;
        }

        return bestEval;
        
    }

    public Move Think(Board board, Timer timer){ 
        Move thinkmove = Move.NullMove;


        int eval = Search(6, 0, -50000, 50000, board, ref thinkmove, timer);
            

        return thinkmove.IsNull ? board.GetLegalMoves()[0]:thinkmove;
    }

}
