using UnityEngine;

public class ChessSquare {
	public piece pieceSquare;
	public side sideSquare;
	public GameObject obj;
	public GameObject square;
	public bool moved;
	
	public ChessSquare(){
		pieceSquare = piece.None;
		sideSquare = side.None;
		moved = true;
	}
	
	public void None(){
		pieceSquare = piece.None;
		sideSquare = side.None;
	}
	
	public void OutOfBoard(){
		pieceSquare = piece.OutOfBoard;
		sideSquare = side.OutOfBoard;
	}
	
	public ChessSquare Clone(){
		ChessSquare c = new ChessSquare ();
		
		c.pieceSquare = pieceSquare;
		c.sideSquare = sideSquare;
		c.obj = obj;
		c.square = square;
		c.moved = moved;
		
		return c;
	}
}