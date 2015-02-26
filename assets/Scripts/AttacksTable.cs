using UnityEngine;
using System.Collections;

public class AttacksTable {
	
	private ChessSquare[,] board;
	
	public piece[,,,] attacks; /* attacks[a,b,c,d] -> posicion a,b atacada desde c,d */

	public AttacksTable(ChessSquare[,] b, bool white){
		board = b;
		if (white)
			generateTable (side.White);
		else
			generateTable (side.Black);
	}
	
	public int numAttacking(Vector2 pos){
		return numAttacking (Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
	}
	
	public bool isAttacked(Vector2 pos){
		return isAttacked (Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
	}
	
	public bool isAttacked(int i, int j){
		for (int k = 2; k < 10; k++)
			for (int l = 2; l < 10; l++)
				if(attacks[i,j,k,l] != piece.None)
					return true;
		return false;
	}
	
	public int numAttacking(int i, int j){
		int ret = 0;
		for (int k = 2; k < 10; k++)
			for (int l = 2; l < 10; l++)
				if(attacks[i,j,k,l] != piece.None)
					ret++;
		return ret;
	}

	void generateTable(side attacker){
		attacks = new piece[12,12,12,12];
		for (int i = 0; i < 12; i++) {
			for (int j = 0; j < 12; j++) {
				for (int k = 0; k < 12; k++) {
					for (int l = 0; l < 12; l++) {
						attacks [i, j, k, l] = piece.None;
					}
				}
			}
		}
		for (int i = 0; i < 12; i++) {
			for (int j = 0; j < 12; j++) {
				for (int k = 0; k < 12; k++) {
					attacks [i, j, k, 0] = piece.OutOfBoard;
					attacks [i, j, k, 1] = piece.OutOfBoard;
					attacks [i, j, k, 10] = piece.OutOfBoard;
					attacks [i, j, k, 11] = piece.OutOfBoard;
					attacks [i, j, 0, k] = piece.OutOfBoard;
					attacks [i, j, 1, k] = piece.OutOfBoard;
					attacks [i, j, 10, k] = piece.OutOfBoard;
					attacks [i, j, 11, k] = piece.OutOfBoard;
				}
			}
		}
		
		for (int i = 2; i < 10; i++) {
			for (int j = 2; j < 10; j++) {
				if(board [i, j].sideSquare != attacker)
					continue;
				
				switch(board [i, j].pieceSquare){
				case piece.Bishop:
					generateBishop (i, j, false);
					break;
					
				case piece.King:
					generateKing (i, j);
					break;
					
				case piece.Rook:
					generateRook (i, j, false);
					break;
					
				case piece.Queen:
					generateRook (i, j, true);
					generateBishop (i, j, true);
					break;
					
				case piece.Knight:
					generateKnight (i, j);
					break;
					
				case piece.Pawn:
					generatePawn (i, j);
					break;
					
				default:
					break;
				}				   
			}
		}
	}
	
	void generateBishop(int i, int j, bool Queen){
		piece p = piece.Bishop;
		if (Queen)
			p = piece.Queen;

		for (int x = 1;; x++) {
			if(board [i+x, j+x].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i+x, j+x, i, j] = p;
			if(board [i+x, j+x].pieceSquare != piece.None)
				break;
		}
		for (int x = 1;; x++) {
			if(board [i-x, j+x].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i-x, j+x, i, j] = p;
			if(board [i-x, j+x].pieceSquare != piece.None)
				break;
		}
		for (int x = 1;; x++) {
			if(board [i+x, j-x].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i+x, j-x, i, j] = p;
			if(board [i+x, j-x].pieceSquare != piece.None)
				break;
		}
		for (int x = 1;; x++) {
			if(board [i-x, j-x].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i-x, j-x, i, j] = p;
			if(board [i-x, j-x].pieceSquare != piece.None)
				break;
		}
	}
	
	void generateRook(int i, int j, bool Queen){
		piece p = piece.Rook;
		if (Queen)
			p = piece.Queen;

		for (int x = 1;; x++) {
			if(board [i+x, j].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i+x, j, i, j] = p;
			if(board [i+x, j].pieceSquare != piece.None)
				break;
		}
		for (int x = 1;; x++) {
			if(board [i, j+x].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i, j+x, i, j] = p;
			if(board [i, j+x].pieceSquare != piece.None)
				break;
		}
		for (int x = 1;; x++) {
			if(board [i, j-x].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i, j-x, i, j] = p;
			if(board [i, j-x].pieceSquare != piece.None)
				break;
		}
		for (int x = 1;; x++) {
			if(board [i-x, j].pieceSquare == piece.OutOfBoard)
				break;
			attacks [i-x, j, i, j] = p;
			if(board [i-x, j].pieceSquare != piece.None)
				break;
		}
	}
	
	void generateKing(int i, int j){
		if(board [i+1, j].pieceSquare != piece.OutOfBoard)
			attacks [i+1, j, i, j] = piece.King;
		if(board [i+1, j+1].pieceSquare != piece.OutOfBoard)
			attacks [i+1, j+1, i, j] = piece.King;
		if(board [i+1, j-1].pieceSquare != piece.OutOfBoard)
			attacks [i+1, j-1, i, j] = piece.King;
		
		if(board [i-1, j].pieceSquare != piece.OutOfBoard)
			attacks [i-1, j, i, j] = piece.King;
		if(board [i-1, j-1].pieceSquare != piece.OutOfBoard)
			attacks [i-1, j-1, i, j] = piece.King;
		if(board [i-1, j+1].pieceSquare != piece.OutOfBoard)
			attacks [i-1, j+1, i, j] = piece.King;
		
		if(board [i, j+1].pieceSquare != piece.OutOfBoard)
			attacks [i, j+1, i, j] = piece.King;
		if(board [i, j-1].pieceSquare != piece.OutOfBoard)
			attacks [i, j-1, i, j] = piece.King;
	}
	
	void generateKnight(int i, int j){
		if(board [i+2, j+1].pieceSquare != piece.OutOfBoard)
			attacks [i+2, j+1, i, j] = piece.Knight;
		if(board [i+2, j-1].pieceSquare != piece.OutOfBoard)
			attacks [i+2, j-1, i, j] = piece.Knight;
		
		if(board [i-2, j+1].pieceSquare != piece.OutOfBoard)
			attacks [i-2, j+1, i, j] = piece.Knight;
		if(board [i-2, j-1].pieceSquare != piece.OutOfBoard)
			attacks [i-2, j-1, i, j] = piece.Knight;
		
		if(board [i+1, j+2].pieceSquare != piece.OutOfBoard)
			attacks [i+1, j+2, i, j] = piece.Knight;
		if(board [i-1, j+2].pieceSquare != piece.OutOfBoard)
			attacks [i-1, j+2, i, j] = piece.Knight;
		
		if(board [i+1, j-2].pieceSquare != piece.OutOfBoard)
			attacks [i+1, j-2, i, j] = piece.Knight;
		if(board [i-1, j-2].pieceSquare != piece.OutOfBoard)
			attacks [i-1, j-2, i, j] = piece.Knight;
	}
	
	void generatePawn(int i, int j){
		int mov = 1;
		if (board [i, j].sideSquare == side.Black)
			mov = -1;
		if(board [i+mov, j+1].pieceSquare != piece.OutOfBoard)
			attacks [i+mov, j+1, i, j] = piece.Pawn;
		if(board [i+mov, j-1].pieceSquare != piece.OutOfBoard)
			attacks [i+mov, j-1, i, j] = piece.Pawn;
	}
}
