using UnityEngine;
using System.Collections;

class AIMove{
	public Vector2 initialPos;
	public Vector2 finalPos;
	public int movedValue;
	public int eatenValue;
	public int risk;
}

public class ChessAI : MonoBehaviour {
	
	private ChessSquare[,] board;
	private Chess chess;

	private AIMove selectedMove;

	private int state; /* 0 = nothing, 1 = thinking, 2 = selecting, 3 = moving */

	private float cooldown;
	private bool initial;

	private ArrayList checkBreakers;
	private ArrayList attackMoves;
	private ArrayList randomMoves;

	private bool waitingPromotion;

	void Start() {
		state = 0;
		chess = GameObject.FindObjectOfType<Chess> ();
		initial = true;
		waitingPromotion = false;
		cooldown = Time.time - 2;
	}

	public void go(ChessSquare[,] b){
		selectedMove = null;
		board = b;
		state = 1;

		side s = side.White;
		AttacksTable t = chess.attacksBlack;
		if (!chess.isWhiteTurn()){
			s = side.Black;
			t = chess.attacksWhite;
		}

		if (chess.checkCheck (b, s, t)) {
			selectedMove = selectCheckBreaker(s);
		}else{
			selectedMove = selectAttack (s, t);
			if(selectedMove == null)
				selectedMove = selectRandom (s);
		}
		if(selectedMove == null){
			chess.theEnd();
			state = 0;
		}

		cooldown = Time.time;
	}

	public void Break(){
		chess.lightAttackers(selectedMove.finalPos, false);
		chess.Deselect();
		state = 0;
	}
	
	ChessSquare[,] simulateMove(AIMove m){
		ChessSquare[,] newBoard = (ChessSquare[,])board.Clone ();
		
		newBoard [Mathf.RoundToInt(m.finalPos.x), Mathf.RoundToInt(m.finalPos.y)] = new ChessSquare ();
		newBoard [Mathf.RoundToInt(m.finalPos.x), Mathf.RoundToInt(m.finalPos.y)].pieceSquare = newBoard [Mathf.RoundToInt(m.initialPos.x), Mathf.RoundToInt(m.initialPos.y)].pieceSquare;
		newBoard [Mathf.RoundToInt(m.finalPos.x), Mathf.RoundToInt(m.finalPos.y)].sideSquare = newBoard [Mathf.RoundToInt(m.initialPos.x), Mathf.RoundToInt(m.initialPos.y)].sideSquare;
		
		newBoard [Mathf.RoundToInt(m.initialPos.x), Mathf.RoundToInt(m.initialPos.y)] = new ChessSquare ();
		newBoard [Mathf.RoundToInt(m.initialPos.x), Mathf.RoundToInt(m.initialPos.y)].pieceSquare = piece.None;
		newBoard [Mathf.RoundToInt(m.initialPos.x), Mathf.RoundToInt(m.initialPos.y)].sideSquare = side.None;

		return newBoard;
	}

	AIMove selectCheckBreaker(side s){
		if(initial){
			ArrayList moves = gimmeMoves (s);
			checkBreakers = new ArrayList();
		
			foreach(AIMove m in moves){
				ChessSquare[,] simulatedBoard = simulateMove(m);
				AttacksTable t = new AttacksTable(simulatedBoard, (s == side.Black));

				if(!chess.checkCheck(simulatedBoard,s,t))
					checkBreakers.Add(m);
			}
		}
		
		if (checkBreakers.Count == 0)
			return null;

		return (AIMove)checkBreakers [Random.Range (0, checkBreakers.Count - 1)];
	}
	
	AIMove selectAttack(side s, AttacksTable t){
		if (initial) {
			ArrayList moves = gimmeMoves (s);
			attackMoves = new ArrayList ();
			foreach (AIMove m in moves)
				if (m.eatenValue != 0)
					attackMoves.Add (m);
		}

		AIMove move = selectMoveAttack(selectMovesOfRisk(attackMoves, 5));
		if(move == null)
			move = selectMoveAttack(selectAttacksDefensive(selectMovesOfRisk(attackMoves, 4)));
		if(move == null)
			move = selectMoveAttack(selectAttacksDefensive(selectMovesOfRisk(attackMoves, 3)));

		return move;
	}
	
	AIMove selectRandom(side s){
		ArrayList aux;
		AIMove ret;

		if(initial)
			randomMoves = gimmeNoKingMoves (s);

		aux = selectMovesDefensive (randomMoves);
		ret = selectMoveByRisk (aux);
		if (ret == null)
			ret = selectMoveByRisk (randomMoves);

		return ret;
	}
	
	void OnGUI(){
		if(state == 1)
			GUI.Box (new Rect (160, 10, 200, 22), "Shitty AI selecting piece...");
		if(state > 1)
			GUI.Box (new Rect (160, 10, 200, 22), "Shitty AI thinking...");
		if(state == 0 && Time.time - cooldown < 1)
			GUI.Box (new Rect (160, 10, 200, 22), "Shitty AI moved!");
	}
	
	void Update(){
		if (state == 1 && Time.time - cooldown > 1) {
			chess.select(Mathf.RoundToInt(selectedMove.initialPos.x), Mathf.RoundToInt(selectedMove.initialPos.y));
			cooldown = Time.time;
			state = 2;
		}
		
		if (state == 2 && Time.time - cooldown > 1) {
			chess.lightAttackers(Mathf.RoundToInt(selectedMove.finalPos.x), Mathf.RoundToInt(selectedMove.finalPos.y), true);
			cooldown = Time.time;
			state = 3;
		}
		
		if (state == 3 && Time.time - cooldown > 1) {
			chess.lightAttackers(Mathf.RoundToInt(selectedMove.finalPos.x), Mathf.RoundToInt(selectedMove.finalPos.y), false);
			if(!chess.move(Mathf.RoundToInt(selectedMove.finalPos.x), Mathf.RoundToInt(selectedMove.finalPos.y))){
				initial = false;
				go (board);
			}else{
				cooldown = Time.time;
				state = 0;
				initial = true;
				waitingPromotion = true;
			}
		}
		if (state == 0 && Time.time - cooldown > 1)
			waitingPromotion = false;

		if (chess.getPromotion() && waitingPromotion)
			chess.promoteQueen ();
	}

	ArrayList pawnMoves(int i,int j){
		ArrayList moves = new ArrayList ();

		int mov = 1;
		if (board [i, j].sideSquare == side.Black)
			mov = -1;
		if(board [i+mov, j+1].sideSquare != board[i,j].sideSquare &&
		   board [i+mov, j+1].sideSquare != side.OutOfBoard &&
		   board [i+mov, j+1].sideSquare != side.None){
			moves.Add(gimmeMove(i, j, i+mov, j+1));
		}

		if(board [i+mov, j-1].sideSquare != board[i,j].sideSquare &&
		   board [i+mov, j-1].sideSquare != side.OutOfBoard &&
		   board [i+mov, j-1].sideSquare != side.None){
			moves.Add(gimmeMove(i, j, i+mov, j-1));
		}
		
		if(board [i+mov, j].sideSquare == side.None){
			moves.Add(gimmeMove(i, j, i+mov, j));
		}

		if((i==3 || i==8) && board [i+mov*2, j].sideSquare == side.None && board [i+mov, j].pieceSquare == piece.None){
			moves.Add(gimmeMove(i, j, i+mov*2, j));
		}

		return moves;
	}
	
	ArrayList knightMoves(int i, int j){
		ArrayList moves = new ArrayList ();
		
		if(board [i+2, j+1].sideSquare != board[i,j].sideSquare && board [i+2, j+1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+2, j+1));
		}
		
		if(board [i-2, j+1].sideSquare != board[i,j].sideSquare && board [i-2, j+1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i-2, j+1));
		}
		
		if(board [i+2, j-1].sideSquare != board[i,j].sideSquare && board [i+2, j-1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+2, j-1));
		}
		if(board [i-2, j-1].sideSquare != board[i,j].sideSquare && board [i-2, j-1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i-2, j-1));
		}
		if(board [i+1, j+2].sideSquare != board[i,j].sideSquare && board [i+1, j+2].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+1, j+2));
		}
		if(board [i+1, j+2].sideSquare != board[i,j].sideSquare && board [i+1, j+2].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+1, j+2));
		}

		if(board [i+1, j-2].sideSquare != board[i,j].sideSquare && board [i+1, j-2].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+1, j-2));
		}
		if(board [i-1, j+2].sideSquare != board[i,j].sideSquare && board [i-1, j+2].sideSquare != side.OutOfBoard){			
			moves.Add(gimmeMove(i, j, i-1, j+2));
		}
		if(board [i-1, j-2].sideSquare != board[i,j].sideSquare && board [i-1, j-2].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i-1, j-2));
		}

		return moves;
	}
	
	ArrayList rookMoves(int i, int j){
		ArrayList moves = new ArrayList ();

		for(int x = 1;; x++){			
			if(board [i+x, j].sideSquare == board[i,j].sideSquare ||
			   board [i+x, j].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i+x, j].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i+x, j));
			}else{
				moves.Add(gimmeMove(i, j, i+x, j));
				break;
			}
		}
		for(int x = 1;; x++){
			if(board [i-x, j].sideSquare == board[i,j].sideSquare ||
			   board [i-x, j].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i-x, j].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i-x, j));
			}else{
				moves.Add(gimmeMove(i, j, i-x, j));
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i, j-x].sideSquare == board[i,j].sideSquare ||
			   board [i, j-x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i, j-x].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i, j-x));
			}else{
				moves.Add(gimmeMove(i, j, i, j-x));
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i, j+x].sideSquare == board[i,j].sideSquare ||
			   board [i, j+x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i, j+x].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i, j+x));
			}else{
				moves.Add(gimmeMove(i, j, i, j+x));
				break;
			}
		}

		return moves;
	}
	
	ArrayList kingMoves(int i, int j){
		ArrayList moves = new ArrayList ();

		if(board [i+1, j+1].sideSquare != board[i,j].sideSquare &&
		   board [i+1, j+1].sideSquare != side.OutOfBoard){			
			moves.Add(gimmeMove(i, j, i+1, j+1));
		}
		if(board [i+1, j-1].sideSquare != board[i,j].sideSquare &&
		   board [i+1, j-1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+1, j-1));
		}
		if(board [i-1, j+1].sideSquare != board[i,j].sideSquare &&
		   board [i-1, j+1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i-1, j+1));
		}
		if(board [i-1, j-1].sideSquare != board[i,j].sideSquare &&
		   board [i-1, j-1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i-1, j-1));
		}
		if(board [i-1, j].sideSquare != board[i,j].sideSquare &&
		   board [i-1, j].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i-1, j));
		}
		if(board [i+1, j].sideSquare != board[i,j].sideSquare &&
		   board [i+1, j].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i+1, j));
		}
		if(board [i, j+1].sideSquare != board[i,j].sideSquare &&
		   board [i, j+1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i, j+1));
		}
		if(board [i, j-1].sideSquare != board[i,j].sideSquare &&
		   board [i, j-1].sideSquare != side.OutOfBoard){
			moves.Add(gimmeMove(i, j, i, j-1));
		}
		
		return moves;
	}
	
	ArrayList bishopMoves(int i, int j){
		ArrayList moves = new ArrayList ();

		for(int x = 1;; x++){			
			if(board [i+x, j+x].sideSquare == board[i,j].sideSquare ||
			   board [i+x, j+x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i+x, j+x].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i+x, j+x));
			}else{
				moves.Add(gimmeMove(i, j, i+x, j+x));
				break;
			}
		}
		for(int x = 1;; x++){
			if(board [i-x, j+x].sideSquare == board[i,j].sideSquare ||
			   board [i-x, j+x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i-x, j+x].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i-x, j+x));
			}else{
				moves.Add(gimmeMove(i, j, i-x, j+x));
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i+x, j-x].sideSquare == board[i,j].sideSquare ||
			   board [i+x, j-x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i+x, j-x].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i+x, j-x));
			}else{
				moves.Add(gimmeMove(i, j, i+x, j-x));
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i-x, j-x].sideSquare == board[i,j].sideSquare ||
			   board [i-x, j-x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i-x, j-x].sideSquare == side.None){
				moves.Add(gimmeMove(i, j, i-x, j-x));
			}else{
				moves.Add(gimmeMove(i, j, i-x, j-x));
				break;
			}
		}

		return moves;
	}

	AIMove gimmeMove(int i, int j, int x, int y){
		AttacksTable t = chess.attacksBlack;
		if (!chess.isWhiteTurn())
			t = chess.attacksWhite;

		AIMove move = new AIMove ();
		move.initialPos = new Vector2 (i, j);
		move.finalPos = new Vector2 (x, y);
		move.movedValue = gimmeValue(board [i, j].pieceSquare);
		move.eatenValue = gimmeValue(board [x, y].pieceSquare);
		move.risk = 5 - t.numAttacking (x, y);
		if(move.risk < 0)
			move.risk = 0;

		return move;
	}
	
	ArrayList gimmeMoves(side s){
		ArrayList moves = new ArrayList ();
		
		for(int i = 2 ; i < 10 ; i++){
			for(int j = 2 ; j < 10 ; j++){
				if(board[i,j].sideSquare == s){
					switch(board [i, j].pieceSquare){
					case piece.Bishop:
						moves.AddRange(bishopMoves (i, j));
						break;
						
					case piece.King:
						moves.AddRange(kingMoves (i, j));
						break;
						
					case piece.Rook:
						moves.AddRange(rookMoves (i, j));
						break;
						
					case piece.Queen:
						moves.AddRange(bishopMoves (i, j));
						moves.AddRange(rookMoves (i, j));
						break;
						
					case piece.Knight:
						moves.AddRange(knightMoves (i, j));
						break;
						
					case piece.Pawn:
						moves.AddRange(pawnMoves (i, j));
						break;
						
					default:
						break;
					}
				}
			}
		}
		
		return moves;
	}
	
	ArrayList gimmeNoKingMoves(side s){
		ArrayList moves = new ArrayList ();
		
		for(int i = 2 ; i < 10 ; i++){
			for(int j = 2 ; j < 10 ; j++){
				if(board[i,j].sideSquare == s){
					switch(board [i, j].pieceSquare){
					case piece.Bishop:
						moves.AddRange(bishopMoves (i, j));
						break;
						
					case piece.Rook:
						moves.AddRange(rookMoves (i, j));
						break;
						
					case piece.Queen:
						moves.AddRange(bishopMoves (i, j));
						moves.AddRange(rookMoves (i, j));
						break;
						
					case piece.Knight:
						moves.AddRange(knightMoves (i, j));
						break;
						
					case piece.Pawn:
						moves.AddRange(pawnMoves (i, j));
						break;
						
					default:
						break;
					}
				}
			}
		}
		
		return moves;
	}
	
	AIMove selectMoveAttack(ArrayList list){
		if(list.Count == 0)
			return null;

		int max = 0;
		foreach (AIMove mov in list)
			max += mov.eatenValue;
		
		int random = Random.Range (0, max);
		
		foreach (AIMove mov in list){
			if(random < mov.eatenValue){
				return mov;
			}else
				random -= mov.eatenValue;
		}
		
		return (AIMove)list [list.Count - 1];
	}
	
	ArrayList selectAttacksDefensive(ArrayList list){
		AttacksTable t = chess.attacksBlack;
		if (chess.isWhiteTurn())
			t = chess.attacksWhite;
		
		ArrayList aux = new ArrayList ();
		foreach (AIMove mov in list)
			if (mov.eatenValue >= mov.movedValue || t.numAttacking(mov.finalPos) > 2)
				aux.Add (mov);
		return aux;
	}
	
	ArrayList selectMovesDefensive(ArrayList list){
		AttacksTable mine = chess.attacksBlack;
		AttacksTable their = chess.attacksWhite;
		if (chess.isWhiteTurn()) {
			mine = chess.attacksWhite;
			their = chess.attacksBlack;
		}
		
		ArrayList aux = new ArrayList ();
		foreach (AIMove mov in list)
			if (their.numAttacking(mov.finalPos) < mine.numAttacking(mov.finalPos))
				aux.Add (mov);
		return aux;
	}
	
	AIMove selectMoveByRisk(ArrayList list){
		if(list.Count == 0)
			return null;

		ArrayList aux = new ArrayList ();
		int max = 0;
		foreach (AIMove mov in list){
			if(mov.risk != 0)
				aux.Add(mov);
			max += mov.risk;
		}
		
		int random = Random.Range (0, max);		
		foreach (AIMove mov in aux){
			if(random < mov.risk){
				return mov;
			}else
				random -= mov.risk;
		}
		
		return (AIMove)aux [list.Count - 1];
	}
	
	ArrayList selectMovesOfRisk(ArrayList list, int risk){
		ArrayList aux = new ArrayList ();
		foreach (AIMove mov in list)
			if(mov.risk == risk)
				aux.Add(mov);
		return aux;
	}

	int gimmeValue(piece p){
		switch (p) {
		case piece.Bishop:
			return 3;

		case piece.Rook:
			return 5;

		case piece.Knight:
			return 3;

		case piece.King:
			return 1000;

		case piece.Pawn:
			return 1;

		case piece.Queen:
			return 9;

		default:
			return 0;
		}
	}
}
