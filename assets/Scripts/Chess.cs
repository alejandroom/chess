using UnityEngine;
using System.Collections;

public enum piece {King, Queen, Pawn, Rook, Knight, Bishop, None, OutOfBoard};
public enum side {Black, White, None, OutOfBoard};

public class Chess : MonoBehaviour {
	public GameObject Square;
	public GameObject Border;
	public GameObject piecePrefab;
	private Texture2D blackTex;
	private Texture2D whiteTex;

	private ChessSquare[,] board;
	
	private Texture2D WhitePawn;
	private Texture2D WhiteRook;
	private Texture2D WhiteBishop;
	private Texture2D WhiteQueen;
	private Texture2D WhiteKing;
	private Texture2D WhiteKnight;
	
	private Texture2D BlackPawn;
	private Texture2D BlackRook;
	private Texture2D BlackBishop;
	private Texture2D BlackQueen;
	private Texture2D BlackKing;
	private Texture2D BlackKnight;

	private int selectedI;
	private int selectedJ;

	private bool white;
	private bool check;
	private bool promotion;

	private bool leftCastling;
	private bool rightCastling;
	
	public AttacksTable attacksWhite;
	public AttacksTable attacksBlack;

	private bool starting;
	private bool playing;
	private bool fin;

	private float checkedCooldown;
	private ArrayList history;
	
	private bool whiteAI = true;
	private bool blackAI = false;
	private ChessAI enemy;
	private bool enemyPlaying;
	private float turnCD;
	private float timeBetweenTurns = 2;

	void Start () {
		WhitePawn = Resources.Load<Texture2D>("Icons/WhitePawn");
		WhiteRook = Resources.Load<Texture2D>("Icons/WhiteRook");
		WhiteBishop = Resources.Load<Texture2D>("Icons/WhiteBishop");
		WhiteKnight = Resources.Load<Texture2D>("Icons/WhiteKnight");
		WhiteQueen = Resources.Load<Texture2D>("Icons/WhiteQueen");
		WhiteKing = Resources.Load<Texture2D>("Icons/WhiteKing");
		BlackKing = Resources.Load<Texture2D>("Icons/BlackKing");
		BlackQueen = Resources.Load<Texture2D>("Icons/BlackQueen");
		BlackPawn = Resources.Load<Texture2D>("Icons/BlackPawn");
		BlackRook = Resources.Load<Texture2D>("Icons/BlackRook");
		BlackBishop = Resources.Load<Texture2D>("Icons/BlackBishop");
		BlackKnight = Resources.Load<Texture2D>("Icons/BlackKnight");
		
		blackTex = Resources.Load<Texture2D>("BlackSquare");
		whiteTex = Resources.Load<Texture2D>("WhiteSquare");

		newBoard ();
		instantiateBoard ();
		attacksWhite = new AttacksTable (board, true);
		attacksBlack = new AttacksTable (board, false);
		leftCastling = false;
		rightCastling = false;
		check = false;
		promotion = false;

		playing = true;
		fin = false;
		checkedCooldown = Time.time - 4f;

		history = new ArrayList ();

		enemy = GameObject.FindObjectOfType<ChessAI>();
		
		white = true;
		enemyPlaying = false;
		turnCD = Time.time;

		starting = true;
	}

	void newBoard(){
		board = new ChessSquare [12,12];
		for (int i = 0; i < 12; i++)
			for (int j = 0; j < 12; j++)
				board [i,j] = new ChessSquare();
		for (int i = 0; i < 12; i++) {
			board [i,0].OutOfBoard();
			board [i,1].OutOfBoard();
			board [i,10].OutOfBoard();
			board [i,11].OutOfBoard();
			board [0,i].OutOfBoard();
			board [1,i].OutOfBoard();
			board [10,i].OutOfBoard();
			board [11,i].OutOfBoard();
		}
		for (int i = 2; i < 10; i++) {			
			board [3,i].pieceSquare = piece.Pawn;
			board [3,i].sideSquare = side.White;
			board [2,i].sideSquare = side.White;
			board [8,i].pieceSquare = piece.Pawn;
			board [8,i].sideSquare = side.Black;
			board [9,i].sideSquare = side.Black;
			
			board [9,i].moved = false;
			board [8,i].moved = false;
			board [3,i].moved = false;
			board [2,i].moved = false;
		}
		board [2,2].pieceSquare = piece.Rook;
		board [2,3].pieceSquare = piece.Knight;
		board [2,4].pieceSquare = piece.Bishop;
		board [2,5].pieceSquare = piece.Queen;
		board [2,6].pieceSquare = piece.King;
		board [2,7].pieceSquare = piece.Bishop;
		board [2,8].pieceSquare = piece.Knight;
		board [2,9].pieceSquare = piece.Rook;
		
		board [9,2].pieceSquare = piece.Rook;
		board [9,3].pieceSquare = piece.Knight;
		board [9,4].pieceSquare = piece.Bishop;
		board [9,5].pieceSquare = piece.King;
		board [9,6].pieceSquare = piece.Queen;
		board [9,7].pieceSquare = piece.Bishop;
		board [9,8].pieceSquare = piece.Knight;
		board [9,9].pieceSquare = piece.Rook;
	}

	void changeTurn(){
		Deselect ();
		white = !white;
		attacksWhite = new AttacksTable (board, true);
		attacksBlack = new AttacksTable (board, false);
		leftCastling = false;
		rightCastling = false;
		promotion = false;
		
		/* Check detection */
		side s = side.White;
		AttacksTable t = attacksBlack;
		if(!white){
			s = side.Black;
			t = attacksWhite;
		}
		check = checkCheck (board, s, t);

		enemyPlaying = false;
		playing = true;
		turnCD = Time.time;
	}

	public bool checkCheck(ChessSquare[,] b, side s, AttacksTable t){
		for (int i = 2; i < 10; i ++) {
			for (int j = 2; j < 10; j ++) {
				if(b [i,j].pieceSquare == piece.King && b [i,j].sideSquare == s && t.isAttacked(i,j))
					return true;
			}
		}
		return false;
	}

	void instantiateBoard(){
		selectedI = 0;
		selectedJ = 0;
		
		for (int i = 2; i < 10; i++) {
			if(i%2==0){
				GameObject border = (GameObject)Instantiate(Border, new Vector3(1.45f,0,i), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
				border.renderer.material.mainTexture = blackTex;
				border = (GameObject)Instantiate(Border, new Vector3(i,0,1.45f), Quaternion.LookRotation(Vector3.Cross(gameObject.transform.forward, gameObject.transform.up))) as GameObject;
				border.renderer.material.mainTexture = blackTex;
				border = (GameObject)Instantiate(Border, new Vector3(i,0,9.55f), Quaternion.LookRotation(Vector3.Cross(gameObject.transform.forward, gameObject.transform.up))) as GameObject;
				border.renderer.material.mainTexture = whiteTex;
				border = (GameObject)Instantiate(Border, new Vector3(9.55f,0,i), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
				border.renderer.material.mainTexture = whiteTex;
			}else{
				GameObject border = (GameObject)Instantiate(Border, new Vector3(1.45f,0,i), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
				border.renderer.material.mainTexture = whiteTex;
				border = (GameObject)Instantiate(Border, new Vector3(i,0,1.45f), Quaternion.LookRotation(Vector3.Cross(gameObject.transform.forward, gameObject.transform.up))) as GameObject;
				border.renderer.material.mainTexture = whiteTex;
				border = (GameObject)Instantiate(Border, new Vector3(i,0,9.55f), Quaternion.LookRotation(Vector3.Cross(gameObject.transform.forward, gameObject.transform.up))) as GameObject;
				border.renderer.material.mainTexture = blackTex;
				border = (GameObject)Instantiate(Border, new Vector3(9.55f,0,i), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
				border.renderer.material.mainTexture = blackTex;
			}
		}
		for (int i = 2; i < 10; i ++) {
			for (int j = 2; j < 10; j ++) {
				if((i+j)%2 == 0){
					board[i,j].square = (GameObject)Instantiate(Square, new Vector3(i,0,j), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
					board[i,j].square.renderer.material.mainTexture = whiteTex;

				}else{
					board[i,j].square = (GameObject)Instantiate(Square, new Vector3(i,0,j), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
					board[i,j].square.renderer.material.mainTexture = blackTex;
				}

				board[i,j].obj = (GameObject)Instantiate(piecePrefab, new Vector3(i,0.1f,j), Quaternion.LookRotation(gameObject.transform.forward)) as GameObject;
				paintPiece(i, j);

				if(board[i,j].pieceSquare == piece.None)
					hidePiece(i, j);
				else
					showPiece(i, j);
			}
		}
	}

	public bool moveUser(int i, int j){
		if (!playing)
			return false;
		return move (i, j);
	}

	public bool move(int i, int j){
		ChessMove move = new ChessMove ();
		move.castling = false;
		move.initialPos = new Vector2 (selectedI, selectedJ);
		move.finalPos = new Vector2 (i, j);
		move.eatenPiece = board [i, j].pieceSquare;
		move.moved = board [selectedI, selectedJ].moved;
		move.eatenSide = board [i, j].sideSquare;
		move.eatenMoved = board [i, j].moved;
		move.movingPiece = board [selectedI, selectedJ].pieceSquare;

		/* Guardamos los datos de las 2 */
		ChessSquare oldInitial = board [selectedI, selectedJ].Clone();
		ChessSquare oldNext = board [i, j].Clone();

		/* Se copian los datos de la ficha */
		board [i, j].sideSquare = board [selectedI, selectedJ].sideSquare;
		board [i, j].pieceSquare = board [selectedI, selectedJ].pieceSquare;
		board [i, j].moved = true;

		/* Se formatea la antigua celda */
		board [selectedI, selectedJ].sideSquare = side.None;
		board [selectedI, selectedJ].pieceSquare = piece.None;
		board [selectedI, selectedJ].moved = true;

		/* Comprobamos futuro jaque. Restablecemos si es invalido el movimiento. */
		AttacksTable t = new AttacksTable (board, !white);
		if (checkCheck (board, oldInitial.sideSquare, t)) {
			board [i, j].sideSquare = oldNext.sideSquare;
			board [i, j].pieceSquare = oldNext.pieceSquare;
			board [i, j].moved = oldNext.moved;

			board [selectedI, selectedJ].sideSquare = oldInitial.sideSquare;
			board [selectedI, selectedJ].pieceSquare = oldInitial.pieceSquare;
			board [selectedI, selectedJ].moved = oldInitial.moved;

			checkedCooldown = Time.time;
			return false;
		}


		/* Se paran los botecitos y se desplaza la ficha */
		board [i, j].obj.GetComponent<Bouncy>().stopBounce();
		paintPiece (i, j);
		showPiece (i, j);
		hidePiece (selectedI, selectedJ);

		/* Se comprueba coronamiento y se efectua */
		if (board [i, j].pieceSquare == piece.Pawn && (i == 9 || i == 2)) {
			selectedI = i;
			selectedJ = j;

			promotion = true;
			playing = false;
			Deselect();
			board [i, j].square.GetComponent<Square>().range();
		}else{
			changeTurn ();
			if (oldNext.pieceSquare == piece.King)
				theEnd ();
		}

		history.Add (move);
		return true;
	}

	public bool selectUser(int i, int j){
		if(!playing)
			return false;
		return select (i, j);
	}
	
	public bool select(int i, int j){
		Deselect ();

		/* No se seleccionan casillas vacias, inexistentes, o del bando que no juega */
		if (board [i, j].pieceSquare == piece.None || board [i, j].pieceSquare == piece.OutOfBoard)
			return false;
		if (board [i, j].sideSquare == side.Black && white)
			return false;
		if (board [i, j].sideSquare == side.White && !white)
			return false;


		selectedJ = j;
		selectedI = i;
		
		board [i, j].obj.GetComponent<Bouncy>().bounce();

		switch(board [i, j].pieceSquare){
		case piece.Bishop:
			lightBishop (i, j);
			break;
			
		case piece.King:
			lightKing (i, j);
			break;
			
		case piece.Rook:
			lightRook (i, j);
			break;

		case piece.Queen:
			lightRook (i, j);
			lightBishop (i, j);
			break;
			
		case piece.Knight:
			lightKnight (i, j);
			break;

		case piece.Pawn:
			lightPawn (i, j);
			break;
		
		default:
			break;
		}

		/* Comprobacion Castling */
		AttacksTable t = attacksWhite;
		if(board [i, j].sideSquare == side.White)
			t = attacksBlack;
		if (board [i, j].pieceSquare == piece.King && board [i, j].moved == false && !t.isAttacked(i, j)) {
			leftCastling = true;
			if(board [i, 2].pieceSquare != piece.Rook || board [i, 2].moved != false)
				leftCastling = false;
			
			for(int x = 3 ; x < j ; x ++)
				if(board [i, x].pieceSquare != piece.None || t.isAttacked(i, x))
					leftCastling = false;

			rightCastling = true;
			if(board [i, 9].pieceSquare != piece.Rook || board [i, 9].moved != false)
				rightCastling = false;
			
			for(int x = 8 ; x > j ; x --)
				if(board [i, x].pieceSquare != piece.None || t.isAttacked(i, x))
					rightCastling = false;
		}

		return true;
	}
	
	public void undo(int numTimes){
		for (int i = 0; i < numTimes; i++)
			undo ();
	}
	public void undo(){
		if (history.Count == 0)
			return;

		ChessMove move = (ChessMove) history [history.Count - 1];
		history.RemoveAt (history.Count - 1);

		if (move.castling) {
			board [Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y)].moved = false;
			board [Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y)].pieceSquare = board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].pieceSquare;
			board [Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y)].sideSquare = board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].sideSquare;
			
			board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].moved = false;
			board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].pieceSquare = piece.None;
			board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].sideSquare = side.None;
			
			paintPiece(Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y));
			showPiece(Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y));
			hidePiece(Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y));

			board [Mathf.RoundToInt(move.initialPosRook.x), Mathf.RoundToInt(move.initialPosRook.y)].moved = false;
			board [Mathf.RoundToInt(move.initialPosRook.x), Mathf.RoundToInt(move.initialPosRook.y)].pieceSquare = board [Mathf.RoundToInt(move.finalPosRook.x), Mathf.RoundToInt(move.finalPosRook.y)].pieceSquare;
			board [Mathf.RoundToInt(move.initialPosRook.x), Mathf.RoundToInt(move.initialPosRook.y)].sideSquare = board [Mathf.RoundToInt(move.finalPosRook.x), Mathf.RoundToInt(move.finalPosRook.y)].sideSquare;
			
			board [Mathf.RoundToInt(move.finalPosRook.x), Mathf.RoundToInt(move.finalPosRook.y)].moved = false;
			board [Mathf.RoundToInt(move.finalPosRook.x), Mathf.RoundToInt(move.finalPosRook.y)].pieceSquare = piece.None;
			board [Mathf.RoundToInt(move.finalPosRook.x), Mathf.RoundToInt(move.finalPosRook.y)].sideSquare = side.None;

			
			paintPiece(Mathf.RoundToInt(move.initialPosRook.x), Mathf.RoundToInt(move.initialPosRook.y));
			showPiece(Mathf.RoundToInt(move.initialPosRook.x), Mathf.RoundToInt(move.initialPosRook.y));
			hidePiece(Mathf.RoundToInt(move.finalPosRook.x), Mathf.RoundToInt(move.finalPosRook.y));
		}else{
			board [Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y)].moved = move.moved;
			board [Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y)].pieceSquare = board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].pieceSquare;
			board [Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y)].sideSquare = board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].sideSquare;
			
			paintPiece(Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y));
			showPiece(Mathf.RoundToInt(move.initialPos.x), Mathf.RoundToInt(move.initialPos.y));

			if(move.eatenPiece != piece.None){
				board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].moved = move.eatenMoved;
				board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].pieceSquare = move.eatenPiece;
				board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].sideSquare = move.eatenSide;
				
				paintPiece(Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y));
				showPiece(Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y));
			}else{
				board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].moved = false;
				board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].pieceSquare = piece.None;
				board [Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y)].sideSquare = side.None;
				
				hidePiece(Mathf.RoundToInt(move.finalPos.x), Mathf.RoundToInt(move.finalPos.y));
			}
		}

		changeTurn ();
	}
	
	public void rightCastle(){
		ChessMove move = new ChessMove ();
		move.castling = true;
		move.initialPos = new Vector2 (selectedI, selectedJ);
		move.finalPos = new Vector2 (selectedI, 8);
		move.initialPosRook = new Vector2 (selectedI, 9);
		move.finalPosRook = new Vector2 (selectedI, 7);

		/* Paramos bouncing */
		board [selectedI, selectedJ].obj.GetComponent<Bouncy>().stopBounce();
		
		/* Movemos selected (rey) a nueva posicion */
		board [selectedI, 8].sideSquare = board [selectedI, selectedJ].sideSquare;
		board [selectedI, 8].pieceSquare = board [selectedI, selectedJ].pieceSquare;
		board [selectedI, 8].moved = true;
		paintPiece (selectedI, 8);
		showPiece (selectedI, 8);
		
		/* Formateamos antigua posicion */
		board [selectedI, selectedJ].sideSquare = side.None;
		board [selectedI, selectedJ].pieceSquare = piece.None;
		board [selectedI, selectedJ].moved = true;
		hidePiece (selectedI, selectedJ);
		
		/* Movemos rook a nueva posicion */
		board [selectedI, 7].sideSquare = board [selectedI, 9].sideSquare;
		board [selectedI, 7].pieceSquare = board [selectedI, 9].pieceSquare;
		board [selectedI, 7].moved = true;
		paintPiece (selectedI, 7);
		showPiece (selectedI, 7);
		
		/* Formateamos antigua posicion */
		board [selectedI, 9].sideSquare = side.None;
		board [selectedI, 9].pieceSquare = piece.None;
		board [selectedI, 9].moved = true;
		hidePiece (selectedI, 9);

		history.Add (move);
		changeTurn ();
	}
	
	public void leftCastle(){
		ChessMove move = new ChessMove ();
		move.castling = true;
		move.initialPos = new Vector2 (selectedI, selectedJ);
		move.finalPos = new Vector2 (selectedI, 3);
		move.initialPosRook = new Vector2 (selectedI, 2);
		move.finalPosRook = new Vector2 (selectedI, 4);

		/* Paramos bouncing */
		board [selectedI, selectedJ].obj.GetComponent<Bouncy>().stopBounce();
		
		/* Movemos selected (rey) a nueva posicion */
		board [selectedI, 3].sideSquare = board [selectedI, selectedJ].sideSquare;
		board [selectedI, 3].pieceSquare = board [selectedI, selectedJ].pieceSquare;
		board [selectedI, 3].moved = true;
		paintPiece (selectedI, 3);
		showPiece (selectedI, 3);
		
		/* Formateamos antigua posicion */
		board [selectedI, selectedJ].sideSquare = side.None;
		board [selectedI, selectedJ].pieceSquare = piece.None;
		board [selectedI, selectedJ].moved = true;
		hidePiece (selectedI, selectedJ);
		
		/* Movemos rook a nueva posicion */
		board [selectedI, 4].sideSquare = board [selectedI, 2].sideSquare;
		board [selectedI, 4].pieceSquare = board [selectedI, 2].pieceSquare;
		board [selectedI, 4].moved = true;
		paintPiece (selectedI, 4);
		showPiece (selectedI, 4);
		
		/* Formateamos antigua posicion */
		board [selectedI, 2].sideSquare = side.None;
		board [selectedI, 2].pieceSquare = piece.None;
		board [selectedI, 2].moved = true;
		hidePiece (selectedI, 2);
		
		history.Add (move);
		changeTurn ();
	}
	
	public void Deselect(){
		leftCastling = false;
		rightCastling = false;
		if(selectedI != 0 && selectedJ !=0 && board [selectedI, selectedJ].obj != null)
			board [selectedI, selectedJ].obj.GetComponent<Bouncy>().stopBounce();
		
		for (int i = 2; i < 10; i ++)
			for (int j = 2; j < 10; j ++)
				board [i, j].square.GetComponent<Square>().resetColor();
	}

	void lightPawn(int i,int j){
		int mov = 1;
		if (board [i, j].sideSquare == side.Black)
			mov = -1;
		if(board [i+mov, j+1].sideSquare != board[i,j].sideSquare &&
		   board [i+mov, j+1].sideSquare != side.OutOfBoard &&
		   board [i+mov, j+1].sideSquare != side.None)
			board [i+mov, j+1].square.GetComponent<Square>().attack();
		if(board [i+mov, j-1].sideSquare != board[i,j].sideSquare &&
		   board [i+mov, j-1].sideSquare != side.OutOfBoard &&
		   board [i+mov, j-1].sideSquare != side.None)
			board [i+mov, j-1].square.GetComponent<Square>().attack();

		if(board [i+mov, j].sideSquare == side.None)
			board [i+mov, j].square.GetComponent<Square>().range();
		if((i==3 || i==8) && board [i+mov*2, j].sideSquare == side.None && board [i+mov, j].pieceSquare == piece.None)
			board [i+mov*2, j].square.GetComponent<Square>().range();
	}

	void lightKnight(int i, int j){
		if(board [i+2, j+1].sideSquare == side.None){
			board [i+2, j+1].square.GetComponent<Square>().range();
		}else if(board [i+2, j+1].sideSquare != board[i,j].sideSquare &&
		         board [i+2, j+1].sideSquare != side.OutOfBoard){
			board [i+2, j+1].square.GetComponent<Square>().attack();
		}
		if(board [i-2, j+1].sideSquare == side.None){
			board [i-2, j+1].square.GetComponent<Square>().range();
		}else if(board [i-2, j+1].sideSquare != board[i,j].sideSquare &&
		         board [i-2, j+1].sideSquare != side.OutOfBoard){
			board [i-2, j+1].square.GetComponent<Square>().attack();
		}
		if(board [i+2, j-1].sideSquare == side.None){
			board [i+2, j-1].square.GetComponent<Square>().range();
		}else if(board [i+2, j-1].sideSquare != board[i,j].sideSquare &&
		         board [i+2, j-1].sideSquare != side.OutOfBoard){
			board [i+2, j-1].square.GetComponent<Square>().attack();
		}
		if(board [i-2, j-1].sideSquare == side.None){
			board [i-2, j-1].square.GetComponent<Square>().range();
		}else if(board [i-2, j-1].sideSquare != board[i,j].sideSquare &&
		         board [i-2, j-1].sideSquare != side.OutOfBoard){
			board [i-2, j-1].square.GetComponent<Square>().attack();
		}
		
		if(board [i+1, j+2].sideSquare == side.None){
			board [i+1, j+2].square.GetComponent<Square>().range();
		}else if(board [i+1, j+2].sideSquare != board[i,j].sideSquare &&
		         board [i+1, j+2].sideSquare != side.OutOfBoard){
			board [i+1, j+2].square.GetComponent<Square>().attack();
		}
		if(board [i+1, j-2].sideSquare == side.None){
			board [i+1, j-2].square.GetComponent<Square>().range();
		}else if(board [i+1, j-2].sideSquare != board[i,j].sideSquare &&
		         board [i+1, j-2].sideSquare != side.OutOfBoard){
			board [i+1, j-2].square.GetComponent<Square>().attack();
		}
		if(board [i-1, j+2].sideSquare == side.None){
			board [i-1, j+2].square.GetComponent<Square>().range();
		}else if(board [i-1, j+2].sideSquare != board[i,j].sideSquare &&
		         board [i-1, j+2].sideSquare != side.OutOfBoard){
			board [i-1, j+2].square.GetComponent<Square>().attack();
		}
		if(board [i-1, j-2].sideSquare == side.None){
			board [i-1, j-2].square.GetComponent<Square>().range();
		}else if(board [i-1, j-2].sideSquare != board[i,j].sideSquare &&
		         board [i-1, j-2].sideSquare != side.OutOfBoard){
			board [i-1, j-2].square.GetComponent<Square>().attack();
		}
	}

	void lightRook(int i, int j){
		for(int x = 1;; x++){			
			if(board [i+x, j].sideSquare == board[i,j].sideSquare ||
			   board [i+x, j].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i+x, j].sideSquare == side.None){
				board [i+x, j].square.GetComponent<Square>().range();
			}else{
				board [i+x, j].square.GetComponent<Square>().attack();
				break;
			}
		}
		for(int x = 1;; x++){
			if(board [i-x, j].sideSquare == board[i,j].sideSquare ||
			   board [i-x, j].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i-x, j].sideSquare == side.None){
				board [i-x, j].square.GetComponent<Square>().range();
			}else{
				board [i-x, j].square.GetComponent<Square>().attack();
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i, j-x].sideSquare == board[i,j].sideSquare ||
			   board [i, j-x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i, j-x].sideSquare == side.None){
				board [i, j-x].square.GetComponent<Square>().range();
			}else{
				board [i, j-x].square.GetComponent<Square>().attack();
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i, j+x].sideSquare == board[i,j].sideSquare ||
			   board [i, j+x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i, j+x].sideSquare == side.None){
				board [i, j+x].square.GetComponent<Square>().range();
			}else{
				board [i, j+x].square.GetComponent<Square>().attack();
				break;
			}
		}
	}

	void lightKing(int i, int j){
		if(board [i+1, j+1].sideSquare == side.None){
			board [i+1, j+1].square.GetComponent<Square>().range();
		}else if(board [i+1, j+1].sideSquare != board[i,j].sideSquare &&
		         board [i+1, j+1].sideSquare != side.OutOfBoard){
			board [i+1, j+1].square.GetComponent<Square>().attack();
		}
		if(board [i+1, j-1].sideSquare == side.None){
			board [i+1, j-1].square.GetComponent<Square>().range();
		}else if(board [i+1, j-1].sideSquare != board[i,j].sideSquare &&
		         board [i+1, j-1].sideSquare != side.OutOfBoard){
			board [i+1, j-1].square.GetComponent<Square>().attack();
		}
		if(board [i-1, j+1].sideSquare == side.None){
			board [i-1, j+1].square.GetComponent<Square>().range();
		}else if(board [i-1, j+1].sideSquare != board[i,j].sideSquare &&
		         board [i-1, j+1].sideSquare != side.OutOfBoard){
			board [i-1, j+1].square.GetComponent<Square>().attack();
		}
		if(board [i-1, j-1].sideSquare == side.None){
			board [i-1, j-1].square.GetComponent<Square>().range();
		}else if(board [i-1, j-1].sideSquare != board[i,j].sideSquare &&
		         board [i-1, j-1].sideSquare != side.OutOfBoard){
			board [i-1, j-1].square.GetComponent<Square>().attack();
		}
		if(board [i-1, j].sideSquare == side.None){
			board [i-1, j].square.GetComponent<Square>().range();
		}else if(board [i-1, j].sideSquare != board[i,j].sideSquare &&
		         board [i-1, j].sideSquare != side.OutOfBoard){
			board [i-1, j].square.GetComponent<Square>().attack();
		}
		if(board [i+1, j].sideSquare == side.None){
			board [i+1, j].square.GetComponent<Square>().range();
		}else if(board [i+1, j].sideSquare != board[i,j].sideSquare &&
		         board [i+1, j].sideSquare != side.OutOfBoard){
			board [i+1, j].square.GetComponent<Square>().attack();
		}
		if(board [i, j+1].sideSquare == side.None){
			board [i, j+1].square.GetComponent<Square>().range();
		}else if(board [i, j+1].sideSquare != board[i,j].sideSquare &&
		         board [i, j+1].sideSquare != side.OutOfBoard){
			board [i, j+1].square.GetComponent<Square>().attack();
		}
		if(board [i, j-1].sideSquare == side.None){
			board [i, j-1].square.GetComponent<Square>().range();
		}else if(board [i, j-1].sideSquare != board[i,j].sideSquare &&
		         board [i, j-1].sideSquare != side.OutOfBoard){
			board [i, j-1].square.GetComponent<Square>().attack();
		}
	}

	void lightBishop(int i, int j){
		for(int x = 1;; x++){			
			if(board [i+x, j+x].sideSquare == board[i,j].sideSquare ||
			   board [i+x, j+x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i+x, j+x].sideSquare == side.None){
				board [i+x, j+x].square.GetComponent<Square>().range();
			}else{
				board [i+x, j+x].square.GetComponent<Square>().attack();
				break;
			}
		}
		for(int x = 1;; x++){
			if(board [i-x, j+x].sideSquare == board[i,j].sideSquare ||
			   board [i-x, j+x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i-x, j+x].sideSquare == side.None){
				board [i-x, j+x].square.GetComponent<Square>().range();
			}else{
				board [i-x, j+x].square.GetComponent<Square>().attack();
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i+x, j-x].sideSquare == board[i,j].sideSquare ||
			   board [i+x, j-x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i+x, j-x].sideSquare == side.None){
				board [i+x, j-x].square.GetComponent<Square>().range();
			}else{
				board [i+x, j-x].square.GetComponent<Square>().attack();
				break;
			}
		}
		
		for(int x = 1;; x++){
			if(board [i-x, j-x].sideSquare == board[i,j].sideSquare ||
			   board [i-x, j-x].sideSquare == side.OutOfBoard){
				break;
			}else if(board [i-x, j-x].sideSquare == side.None){
				board [i-x, j-x].square.GetComponent<Square>().range();
			}else{
				board [i-x, j-x].square.GetComponent<Square>().attack();
				break;
			}
		}
	}

	void OnGUI(){
		if (starting) {
			GUI.Box (new Rect (Screen.width / 10, Screen.height / 10, Screen.width / 10 * 8, Screen.height / 10 * 8), "");
			
			GUI.Box (new Rect (Screen.width / 2 - Screen.width / 20, Screen.height / 20, Screen.width / 10, Screen.height / 20), "Nueva Partida!");

			GUI.Box (new Rect (Screen.width / 10*2.5f, Screen.height / 10*3, Screen.width / 10, Screen.height / 10), "Blancas");
			if(whiteAI){
				if(GUI.Button (new Rect (Screen.width / 10*2, Screen.height / 10*4, Screen.width / 10 * 2, Screen.height / 10 * 2), "Ordenador"))
					whiteAI = false;
			}else{
				if(GUI.Button (new Rect (Screen.width / 10*2, Screen.height / 10*4, Screen.width / 10 * 2, Screen.height / 10 * 2), "Jugador"))
					whiteAI = true;
			}

			GUI.Box (new Rect (Screen.width / 10*6.5f, Screen.height / 10*3, Screen.width / 10, Screen.height / 10), "Negras");
			if(blackAI){
				if(GUI.Button (new Rect (Screen.width / 10*6, Screen.height / 10*4, Screen.width / 10 * 2, Screen.height / 10 * 2), "Ordenador"))
					blackAI = false;
			}else{
				if(GUI.Button (new Rect (Screen.width / 10*6, Screen.height / 10*4, Screen.width / 10 * 2, Screen.height / 10 * 2), "Jugador"))
					blackAI = true;
			}
			if(GUI.Button (new Rect (Screen.width / 2 - Screen.width / 20, Screen.height / 10 * 9f, Screen.width / 10, Screen.height / 20), "Go!")){
				starting = false;
				GameObject.FindObjectOfType<ControlTemporal>().activo = true;
			}

			return;
		}

		if(white)
			GUI.Box (new Rect (10, 10, 150, 22), "Turno de las blancas!");
		else
			GUI.Box (new Rect (10, 10, 150, 22), "Turno de las negras!");
		
		if (rightCastling)
			if (GUI.Button (new Rect (Screen.width - 160, Screen.height - 32, 150, 22), "Enroque Derecho"))
				rightCastle ();
		if (leftCastling)
			if (GUI.Button (new Rect (Screen.width - 160, Screen.height - 52, 150, 22), "Enroque Izquierdo"))
				leftCastle ();

		if (promotion){
			if(!playing){
				GUI.Box (new Rect (Screen.width - 160, Screen.height - 32, 150, 22), "Promocionar a Reina");
				GUI.Box (new Rect (Screen.width - 160, Screen.height - 52, 150, 22), "Promocionar a Caballo");
			}else{
				if (GUI.Button (new Rect (Screen.width - 160, Screen.height - 32, 150, 22), "Promocionar a Reina")){
					promoteQueen();
				}
				if (GUI.Button (new Rect (Screen.width - 160, Screen.height - 52, 150, 22), "Promocionar a Caballo")){
					promoteKnight();
				}
			}
		}

		if(check)
			GUI.Box (new Rect (10, 32, 150, 22), "CHECK!");

		if(Time.time - checkedCooldown < 3)
			GUI.Box (new Rect (Screen.width / 2 - 100, 20, 200, 22), "Ilegal Move: King checked!");

		if(fin){
			if(white)
				GUI.Box (new Rect (Screen.width - 160, 10, 150, 22), "Black wins!");
			else
				GUI.Box (new Rect (Screen.width - 160, 10, 150, 22), "White wins!");
		}else{
			if (!playing)
				GUI.Box (new Rect (Screen.width - 160, 10, 150, 22), "Rendirse!");
			else if (GUI.Button (new Rect (Screen.width - 160, 10, 150, 22), "Rendirse!")){
				theEnd ();
				white = false;
			}
		}
	}

	public void theEnd(){
		fin = true;
		playing = false;
	}
	
	void transPiece(int i, int j){
		board [i, j].obj.GetComponent<Pieces> ().translucent ();
	}

	void hidePiece(int i, int j){
		board [i, j].obj.GetComponent<Pieces> ().hide ();
	}
	
	void showPiece(int i, int j){
		board [i, j].obj.GetComponent<Pieces> ().show ();
	}
	
	void lightPiece(int i, int j){
		board [i, j].obj.GetComponent<Pieces> ().shine ();
	}
	
	void paintPiece(int i, int j){
		if(board[i,j].sideSquare == side.None)
			hidePiece(i, j);

		if(board[i,j].sideSquare == side.Black){
			if(board[i,j].pieceSquare == piece.Bishop)
				board[i,j].obj.renderer.material.mainTexture = BlackBishop;
			if(board[i,j].pieceSquare == piece.Queen)
				board[i,j].obj.renderer.material.mainTexture = BlackQueen;
			if(board[i,j].pieceSquare == piece.King)
				board[i,j].obj.renderer.material.mainTexture = BlackKing;
			if(board[i,j].pieceSquare == piece.Knight)
				board[i,j].obj.renderer.material.mainTexture = BlackKnight;
			if(board[i,j].pieceSquare == piece.Pawn)
				board[i,j].obj.renderer.material.mainTexture = BlackPawn;
			if(board[i,j].pieceSquare == piece.Rook)
				board[i,j].obj.renderer.material.mainTexture = BlackRook;
		}
		if(board[i,j].sideSquare == side.White){
			if(board[i,j].pieceSquare == piece.Bishop)
				board[i,j].obj.renderer.material.mainTexture = WhiteBishop;
			if(board[i,j].pieceSquare == piece.Queen)
				board[i,j].obj.renderer.material.mainTexture = WhiteQueen;
			if(board[i,j].pieceSquare == piece.King)
				board[i,j].obj.renderer.material.mainTexture = WhiteKing;
			if(board[i,j].pieceSquare == piece.Knight)
				board[i,j].obj.renderer.material.mainTexture = WhiteKnight;
			if(board[i,j].pieceSquare == piece.Pawn)
				board[i,j].obj.renderer.material.mainTexture = WhitePawn;
			if(board[i,j].pieceSquare == piece.Rook)
				board[i,j].obj.renderer.material.mainTexture = WhiteRook;
		}
	}
	
	void paintPiece(int i, int j, piece p, side s){
		if(s == side.Black){
			if(p == piece.Bishop)
				board[i,j].obj.renderer.material.mainTexture = BlackBishop;
			if(p == piece.Queen)
				board[i,j].obj.renderer.material.mainTexture = BlackQueen;
			if(p == piece.King)
				board[i,j].obj.renderer.material.mainTexture = BlackKing;
			if(p == piece.Knight)
				board[i,j].obj.renderer.material.mainTexture = BlackKnight;
			if(p == piece.Pawn)
				board[i,j].obj.renderer.material.mainTexture = BlackPawn;
			if(p == piece.Rook)
				board[i,j].obj.renderer.material.mainTexture = BlackRook;
		}
		if(s == side.White){
			if(p == piece.Bishop)
				board[i,j].obj.renderer.material.mainTexture = WhiteBishop;
			if(p == piece.Queen)
				board[i,j].obj.renderer.material.mainTexture = WhiteQueen;
			if(p == piece.King)
				board[i,j].obj.renderer.material.mainTexture = WhiteKing;
			if(p == piece.Knight)
				board[i,j].obj.renderer.material.mainTexture = WhiteKnight;
			if(p == piece.Pawn)
				board[i,j].obj.renderer.material.mainTexture = WhitePawn;
			if(p == piece.Rook)
				board[i,j].obj.renderer.material.mainTexture = WhiteRook;
		}
	}
	
	public void lightAttackers(Vector2 pos, bool light){
		lightAttackers (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y), light);
	}

	public void lightAttackers(int i, int j, bool light){
		AttacksTable t = attacksBlack;
		if (!white)
			t = attacksWhite;

		if(light){
			paintPiece (i, j, board [selectedI, selectedJ].pieceSquare, board [selectedI, selectedJ].sideSquare);
			transPiece (i, j);

			for (int x = 2; x < 10; x++){
				for (int y = 2; y < 10; y++) {
					if(t.attacks[i,j,x,y] != piece.None)
						lightPiece(x, y);
				}
			}
		}else{
			paintPiece (i, j);
			
			for (int x = 2; x < 10; x++){
				for (int y = 2; y < 10; y++) {
					if(t.attacks[i,j,x,y] != piece.None)
						showPiece(x, y);
				}
			}
		}
	}

	public void promoteQueen(){
		if(!promotion)
			return;

		if(board [selectedI, selectedJ].sideSquare == side.White)
			board [selectedI, selectedJ].obj.renderer.material.mainTexture = WhiteQueen;
		else
			board [selectedI, selectedJ].obj.renderer.material.mainTexture = BlackQueen;
		
		board [selectedI, selectedJ].pieceSquare = piece.Queen;
		
		playing = true;
		changeTurn();
	}

	public void promoteKnight(){
		if(!promotion)
			return;

		if(board [selectedI, selectedJ].sideSquare == side.White)
			board [selectedI, selectedJ].obj.renderer.material.mainTexture = WhiteKnight;
		else
			board [selectedI, selectedJ].obj.renderer.material.mainTexture = BlackKnight;
		
		board [selectedI, selectedJ].pieceSquare = piece.Knight;
		
		playing = true;
		changeTurn();
	}

	void Update(){
		if (starting)
			return;

		if (((white && whiteAI) || (!white && blackAI)) && !fin) {
			playing = false;
		}
		if (((white && whiteAI) || (!white && blackAI))&& !enemyPlaying && Time.time - turnCD > timeBetweenTurns && !fin) {
			enemy.go(board);
			enemyPlaying = true;
			playing = false;
		}
	}

	public void Reset(){
		resetBoard ();
		attacksWhite = new AttacksTable (board, true);
		attacksBlack = new AttacksTable (board, false);
		leftCastling = false;
		rightCastling = false;
		check = false;
		promotion = false;
		
		playing = true;
		fin = false;
		checkedCooldown = Time.time - 4f;
		
		history = new ArrayList ();
		
		white = true;
		enemyPlaying = false;
		turnCD = Time.time;
		
		starting = true;
	}
	
	void resetBoard(){
		for (int i = 0; i < 12; i++) {
			board [i,0].OutOfBoard();
			board [i,1].OutOfBoard();
			board [i,10].OutOfBoard();
			board [i,11].OutOfBoard();
			board [0,i].OutOfBoard();
			board [1,i].OutOfBoard();
			board [10,i].OutOfBoard();
			board [11,i].OutOfBoard();
		}
		for (int i = 2; i < 10; i++) {		
			for(int j = 2; j < 10; j++)
				board [j,i].None();
			board [3,i].pieceSquare = piece.Pawn;
			board [3,i].sideSquare = side.White;
			board [2,i].sideSquare = side.White;
			board [8,i].pieceSquare = piece.Pawn;
			board [8,i].sideSquare = side.Black;
			board [9,i].sideSquare = side.Black;
			
			board [9,i].moved = false;
			board [8,i].moved = false;
			board [3,i].moved = false;
			board [2,i].moved = false;
		}
		board [2,2].pieceSquare = piece.Rook;
		board [2,3].pieceSquare = piece.Knight;
		board [2,4].pieceSquare = piece.Bishop;
		board [2,5].pieceSquare = piece.Queen;
		board [2,6].pieceSquare = piece.King;
		board [2,7].pieceSquare = piece.Bishop;
		board [2,8].pieceSquare = piece.Knight;
		board [2,9].pieceSquare = piece.Rook;
		
		board [9,2].pieceSquare = piece.Rook;
		board [9,3].pieceSquare = piece.Knight;
		board [9,4].pieceSquare = piece.Bishop;
		board [9,5].pieceSquare = piece.King;
		board [9,6].pieceSquare = piece.Queen;
		board [9,7].pieceSquare = piece.Bishop;
		board [9,8].pieceSquare = piece.Knight;
		board [9,9].pieceSquare = piece.Rook;

		selectedI = 0;
		selectedJ = 0;
		
		for (int i = 2; i < 10; i ++) {
			for (int j = 2; j < 10; j ++) {
				paintPiece(i, j);
				
				if(board[i,j].pieceSquare == piece.None)
					hidePiece(i, j);
				else
					showPiece(i, j);
			}
		}
	}

	public bool getEnemyPlaying(){
		return enemyPlaying;
	}

	public ArrayList getHistory(){
		return history;
	}
	
	public Texture2D getBlackTex(){
		return blackTex;
	}
	
	public Texture2D getWhiteTex(){
		return whiteTex;
	}

	public bool getStarting(){
		return starting;
	}

	public bool getPromotion(){
		return promotion;
	}

	public bool isWhiteTurn(){
		return white;
	}
}
