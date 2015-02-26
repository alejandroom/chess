using UnityEngine;

public class ChessMove {
	public Vector2 initialPos;
	public Vector2 finalPos;
	public piece eatenPiece;
	public side eatenSide;
	public bool eatenMoved;
	
	public piece movingPiece;
	
	public bool castling;
	public Vector2 initialPosRook;
	public Vector2 finalPosRook;
	
	public bool moved;
	
	public override string ToString(){
		string ret = "";
		string ch = "00ABCDEFGH";
		
		if (castling) {
			ret = "Castling at " + ch[Mathf.RoundToInt(finalPos.x)] + Mathf.RoundToInt(finalPos.y);
		}else{
			ret = movingPiece + " to " + ch[Mathf.RoundToInt(finalPos.x)] + Mathf.RoundToInt(finalPos.y);
		}
		
		return ret;
	}
}
