using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class csTempleRun 
	: MonoBehaviour 
{
	#region Enums

		// Determines direction of cells (track pieces) and the player
		public enum enCellDir
		{
			North,
			East,
			West,
			South,
			NorthSouth,
			EastWest,
		};
		
		// Determines the type of cell (track piece)
		public enum enCellType
		{
			Run,
			JumpGap,	
			JumpObstacle,
			DuckObstacle,
			LedgeLeft,
			LedgeRight,
		};
		
		// Determines which "tileset" to use for the cell (stone ruin or wooden bridge)
		public enum enCellTheme
		{
			Stone,
			Bridge,
		};

	#endregion

	#region Cell Data
	
		// Class to hold information about an individual cell (piece)
		public class stCell
		{
			// Basic cell information
			public Vector3     CellPosition;		
			public enCellDir   CellDirection;
			public enCellType  CellType;
			public enCellTheme CellTheme;
			public GameObject  CellModel;
			
			// Indices of neighbouring cells
			public stCell NeighbourN;
			public stCell NeighbourS;
			public stCell NeighbourE;
			public stCell NeighbourW;
	
			// Decoration Objects (Rocks, Trees, etc.)
			public GameObject DecoN;
			public GameObject DecoS;
			public GameObject DecoE;
			public GameObject DecoW;

			// Decorations (rocks, trees, etc.)
			public List<GameObject> 
				m_deco = new List<GameObject>();	

			// Collectable Coins
			public List<GameObject> 
				m_coins = new List<GameObject>();
		
			// Landmines 
			public List<GameObject> 
				m_mines = new List<GameObject>();
		
			// Has the player visited this cell?  
			// This is used to remove old cells
			public bool Visited = false;		
	
			// CTOR
			public stCell
				(Vector3    cellPosition,
				 enCellDir  cellDirection,
				 enCellType cellType)
			{
				this.CellPosition  = cellPosition;
				this.CellDirection = cellDirection;
				this.CellType 	= cellType;
				this.CellTheme  = csTempleRun.enCellTheme.Stone;
				
				this.NeighbourN = null;
				this.NeighbourS = null;
				this.NeighbourE = null;
				this.NeighbourW = null;
			}
		}	

	#endregion
	
	#region Public Members
	
		// These are the prefabs used by the system.
		
		public GameObject m_ragdollPrefab;
		public GameObject m_playerPrefab;
	
		public GameObject m_ITile;
		public GameObject m_ITileBroken;
		public GameObject m_ITileBrokenHalf;
		public GameObject m_ITile_ObstacleJump;
		public GameObject m_ITile_ObstacleDuck;
		public GameObject m_LTile;
	
		public GameObject m_Bridge_ITile;
		public GameObject m_Bridge_ITileBroken;	
		public GameObject m_Bridge_ITileBrokenHalf;
		public GameObject m_Bridge_ITile_ObstacleJump;
		public GameObject m_Bridge_ITile_ObstacleDuck;	
		public GameObject m_Bridge_LTile;
	
		public GameObject m_Bridge2Stone_ITile;
	
		public GameObject m_Rock1;
		public GameObject m_Rock2;
		public GameObject m_Rock3;
		public GameObject m_Rock4;
	
		public GameObject m_Column;
		public GameObject m_Tree;
	
		public GameObject m_Ruin1;
		public GameObject m_Ruin2;
		public GameObject m_Arch;
	
		public GameObject m_GoldCoin;
	
		public GameObject m_LandMineSign;
		public GameObject m_LandMine;
		public GameObject m_Explosion;

		public Texture2D 
			m_LOGO;

	#endregion

	#region Private Members
	
		// Instantiated player
		private GameObject m_player;

		// Instantiated ragdoll
		private GameObject m_playerRagDoll;

		// The cells (track pieces)
		private List<stCell> 
			m_cells = new List<stCell>();	
		
		// The maximum number of cells to be created ahead of the player (NOT the maximum length of the track; this changes dynamically)
		private int		
			m_maxCells 
				= 8;
		
		// track scaling / spacing
		private float
			m_cellSpacing 
				= 6.0f;
		
		// modifier applied to Y axis when placing cells
		private float 
			m_tileBaseHeightRuin
				= -0.1f; 
	
		// modifier applied to Y axis when placing cells
		private float 
			m_tileBaseHeightBridge
				= 0.65f; 
	
		// Height at which to place coins
		private float		
			m_coinHeight
				= 0.8f;
	
		// modifier applied to Y axis when updating the player position
		private float
			m_playerBaseHeight 
				= 0.7f;
		
		// which cell is the player currently in?
		private stCell 
			m_playerCell = null;
		
		// "loop" timer, used to move a player across a cell
		private float	
			m_playerTimer
				= 0.0f;
		
		// How fast is the player moving when game starts	
		private float
			m_playerRunSpeedStart
				= 0.75f;
	
		// How fast is the player actually moving
		private float
			m_playerRunSpeed
				= 0.75f;
	
		// Controls player jumping
		private float	
			m_playerJump = -1.0f;
		private float		
			m_playerYvel = 0.0f;
		
		// Controls player sliding
		private float
			m_playerSlide = 0.0f;
		
		// current direction of travel
		private enCellDir
			m_playerDirection 
				= enCellDir.North;
		
		// next direction of travel (from input)
		private enCellDir
			m_playerNextDirection
				= enCellDir.North;
		
		// direction of previous cell
		private enCellDir
			m_previousCellDirection
				= enCellDir.North;
	
		// How far have we travelled (meaningless to game logic, purely for players benefit)
		private float
			m_distanceRun;
	
		// What was our best run distance? (meaningless to game logic, purely for players benefit)
		private float
			m_distanceRunBest;	

		// How many coins has the player collected?
		private float	
			m_coinsCollected;
		
		// What the best coin pickup score?
		private float
			m_coinsCollectedBest;
		
		// temporary, until track placement algorythm is better.  used to make track turn left and then right alternately
		private float		
			m_lastTurnDir = -0.5f;
	
		// Stumble timer.  Kicks in if the player turns to early, to make the character "trip"
		private float 
			m_stumble
				= 0.0f;	
	
		// Used to strafe the player from one side of the track to the other, based on mouse input.
		// On a phone, this would be the tilt of the phone.
		private float		
			m_tilt;	
	
		// How long after death before GUI is displayed.	
		private float 
			m_resetTimer;	

		// Set when the player dies, so the GUI can explain what happened (or insult the player...)
		private string m_dieReason = "";

	#endregion

	#region Audio Clips
	
		public AudioClip 
			m_splatAudio;
	
		public AudioClip 
			m_fallAudio;
	
		public AudioClip 
			m_stumbleAudio;
	
		public AudioClip
			m_turnAudio;
		
		public AudioClip
			m_jumpAudio;
	
		public AudioClip
			m_chingAudio;
	
		public AudioClip
			m_explodeAudio;
	
	#endregion		

	#region Core Functions

		void Start () 
		{			
			CreateCells();	
			CreatePlayer();			
		}	
		
		void Update () 
		{	
			UpdatePlayer();
	
			if (m_player != null)
			{
				// Make the camera follow the player (if active)
				SmoothFollow sF = (SmoothFollow)Camera.mainCamera.GetComponent(typeof(SmoothFollow));
				sF.target = m_player.transform;			
	
				// Check for collisions with interactive objects
				UpdateCoins();
				UpdateMines();

				// Dynamically update the track
				CreateNewCellsIfNeeded(false);
			}		
		}

		/// <summary>
		/// VERY simple GUI.
		/// </summary>
		public void OnGUI()
		{		
			GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
	    	centeredStyle.alignment = TextAnchor.UpperCenter;
	
			if (m_player == null)
			{
				m_resetTimer+=Time.deltaTime;
				if (m_resetTimer > 5.0f)
				{
					m_resetTimer = 5.0f;
	
					GUI.DrawTexture(screenRect(0.2f, 0.2f, 0.6f, 0.2f), m_LOGO);
	
					GUI.Label(screenRect(0.2f, 0.45f, 0.6f, 0.2f), m_dieReason);
	
					if (GUI.Button(screenRect(0.4f, 0.6f, 0.2f, 0.1f), "TRY AGAIN")) 
					{	
						DestroyAllCells();
						CreateCells();			
						CreatePlayer();
						m_resetTimer = 0.0f;
					}							
				}
				
			}
	
			if (m_distanceRun > m_distanceRunBest)
				m_distanceRunBest = m_distanceRun;
	
			GUI.Label(new Rect(10,10,100,20), "Distance:" + m_distanceRun.    ToString() + "M");
			GUI.Label(new Rect(10,40,100,20), "Best:" + m_distanceRunBest.ToString() + "M");
	
			GUI.Label(new Rect(10,70, 100,20), "Coins:" + m_coinsCollected.ToString() );
			GUI.Label(new Rect(10,100,100,20), "Best:"  + m_coinsCollectedBest.ToString() );
		}

	#endregion

	#region Private Functions

	private void CreateNewCellsIfNeeded(bool ForceCreate)
	{
		// If player is halfway along the track, we should create new segments and remove old ones
		if ((m_cells.IndexOf(m_playerCell) >= m_cells.Count / 2 && m_playerTimer > 0.5f) || ForceCreate)
		{
			int startFrom = m_cells.Count;

			// This is a recursive call.  Creates [m_maxCells] segments
			CreateNextCell(m_cells[m_cells.Count-1], 0);			
			
			// Create scenery, change segments into obstacles, place 3d Models, etc.
			for (int k=startFrom;k<m_cells.Count;k++)
			{
				CreateScenery(m_cells[k]);
				CreateObstacle(m_cells[k]);
				CreateCellModel(m_cells[k]);
				CreateLandMine(m_cells[k]);
				CreateCoins(m_cells[k]);
			}			
			
			// Cleanup old segments (remove them)
			if (!ForceCreate)
			{
				for (int k = m_cells.IndexOf(m_playerCell);k>=0;k--)
				{
					if (m_cells[k].Visited == true && m_playerCell != m_cells[k])
					{
						for (int l=m_cells[k].m_deco.Count-1;l>=0;l--)
						{
							Destroy(m_cells[k].m_deco[l]);
						}

						m_cells[k].m_deco.Clear();
						m_cells[k].m_deco.TrimExcess();
	
						for (int l=m_cells[k].m_coins.Count-1;l>=0;l--)
						{
							Destroy(m_cells[k].m_coins[l]);
						}

						m_cells[k].m_coins.Clear();
						m_cells[k].m_coins.TrimExcess();	
						
						for (int l=m_cells[k].m_mines.Count-1;l>=0;l--)
						{
							Destroy(m_cells[k].m_mines[l]);
						}

						m_cells[k].m_mines.Clear();
						m_cells[k].m_mines.TrimExcess();	
	
						Destroy(m_cells[k].CellModel);
						m_cells.RemoveAt(k);
					}
				}
			}	
		}
	}

	/// <summary>
	/// Checks to see if the player has collided with gold coins
	/// </summary>
	private void UpdateCoins()
	{
		if (m_player == null)
			return;

		for (int c = 0;c<m_cells.Count;c++)
		{	
			for (int k=m_cells[c].m_coins.Count-1;k>=0;k--)
			{
				if (Vector3.Distance(m_cells[c].m_coins[k].transform.position, m_player.transform.position) < 0.3f)
				{				
					AudioSource.PlayClipAtPoint(m_chingAudio, m_player.transform.position);	
					Destroy(m_cells[c].m_coins[k]);
					m_cells[c].m_coins.RemoveAt(k);
					m_coinsCollected++;
				}
			}
		}
	}

	/// <summary>
	/// Checks to see if the player has collided with landmines
	/// </summary>
	private void UpdateMines()
	{
		if (m_player == null)
			return;		
		
		for (int c = 0;c<m_cells.Count;c++)
		{
			for (int k=m_cells[c].m_mines.Count-1;k>=0;k--)
			{
				if (Vector3.Distance(m_cells[c].m_mines[k].transform.position, m_player.transform.position) < 0.2f)
				{	
					Instantiate(m_Explosion, m_player.transform.position, Quaternion.identity);						
					Destroy(m_cells[c].m_mines[k]);
					m_cells[c].m_mines.RemoveAt(k);
					DoRagDoll(true, true, m_explodeAudio);		
					m_dieReason = "It's all fun and games until somebody loses a leg.  (Avoid landmines!)";
				}
			}
		}
	}

	

	// Helper function for scaling GUI elements
	public static Rect screenRect
		(float tx,
		 float ty,
	     float tw,
		 float th) 
    {
        float x1 = tx * Screen.width;
        float y1 = ty * Screen.height;     
        float sw = tw * Screen.width;
        float sh = th * Screen.height;
        return new Rect(x1,y1,sw,sh);
    }
	
	// Set / reset the player at the start of each run.
	private void CreatePlayer()
	{
		m_player = (GameObject)Instantiate(m_playerPrefab);

		m_playerRunSpeed = m_playerRunSpeedStart;

		m_player.animation["run" ].speed = m_playerRunSpeed * 2.0f;
		m_player.animation["jump"].speed = 1.5f;

		m_playerCell            = m_cells[0];
		m_playerDirection       = enCellDir.North;
		m_playerNextDirection   = enCellDir.North;
		m_previousCellDirection = enCellDir.North;

		m_playerJump = -1.0f;	
		m_playerYvel =  0.0f;
		m_playerSlide = 0.0f;		

		if (m_distanceRun > m_distanceRunBest)
			m_distanceRunBest = m_distanceRun;

		m_distanceRun = 0.0f;

		if (m_coinsCollected > m_coinsCollectedBest)
			m_coinsCollectedBest = m_coinsCollected;
	
		m_coinsCollected = 0;

		if (m_playerRagDoll != null) Destroy(m_playerRagDoll);
	}
	
	// Logic for the player. 
	private void UpdatePlayer()
	{
		// if the player is dead (replaced with ragdoll) then exit since none of this code should fire.
		if (m_player == null) 
		{
			return;		
		}
		
		// AUTO PLAYER FOR TESTING
		// Allows the AI to control the player.  Used for debugging mainly.

//		m_playerNextDirection = m_playerCell.CellDirection;
//		if (m_playerCell.CellType == enCellType.DuckObstacle && m_playerSlide <=0.0f && m_playerTimer <0.25f) 
//		{
//			m_playerSlide = 1.0f;
//		}
//		else if (m_playerCell.CellType != enCellType.Run &&  m_playerCell.CellType != enCellType.DuckObstacle && m_playerJump <=-1.0f && m_playerTimer >=0.2f && m_playerSlide <=0.0f)
//		{
//			m_playerJump = 1.0f;
//		}
		
		// Gradually increase the players' running speed, and update the animation to match.
		m_playerRunSpeed += Time.deltaTime * 0.005f;
		m_playerRunSpeed = Mathf.Clamp(m_playerRunSpeed, 0.5f, 3.0f);
		m_player.animation["run"].speed = m_playerRunSpeed * 2.0f;
		
		// ****************************************************************************************
		// INPUT

		// Player can only turn if they are not already sliding / jumping.  
		// Equally, sliding / jumping are mutually exclusive.

		if (Input.GetKeyDown(KeyCode.LeftArrow) && m_playerJump <= -1.0f && m_playerSlide <=0.0f)
		{
			if (m_playerDirection == enCellDir.North) m_playerNextDirection = enCellDir.West;
			if (m_playerDirection == enCellDir.East ) m_playerNextDirection = enCellDir.North;
			if (m_playerDirection == enCellDir.South) m_playerNextDirection = enCellDir.East;
			if (m_playerDirection == enCellDir.West ) m_playerNextDirection = enCellDir.South;
		}

		if (Input.GetKeyDown(KeyCode.RightArrow) && m_playerJump <= -1.0f && m_playerSlide <=0.0f)
		{
			if (m_playerDirection == enCellDir.North) m_playerNextDirection = enCellDir.East;
			if (m_playerDirection == enCellDir.East ) m_playerNextDirection = enCellDir.South;
			if (m_playerDirection == enCellDir.South) m_playerNextDirection = enCellDir.West;
			if (m_playerDirection == enCellDir.West ) m_playerNextDirection = enCellDir.North;
		}	

		if (Input.GetKeyDown(KeyCode.DownArrow) && m_playerJump <= -1.0f && m_playerSlide <=0.0f)
		{
			m_playerSlide = 1.0f;
			m_player.animation.Play("slide_fake");
		}			 
		
		if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) && m_playerJump <= -1.0f && m_playerSlide <=0.0f)
		{
			AudioSource.PlayClipAtPoint(m_jumpAudio, m_player.transform.position);
			m_playerJump = 1.0f;
			m_playerYvel = 0.0f;
			m_player.animation.Play("jump");
		}	

		// ****************************************************************************************	
		
		// Reset animation if not jumping, sliding, or stumbling.
		if (m_playerJump <=-1.0f && m_playerSlide <=0.0f)
		{
			if (!m_player.animation.IsPlaying("flip"))
				m_player.animation.Play("run");
		}

		// If we are stumbling, make the camera shake a little.
		if (m_stumble > 0.0f) 
		{
			m_stumble -= Time.deltaTime * 2.0f;

			Camera.mainCamera.transform.Rotate
				(Vector3.up, Random.Range(m_stumble * -5.0f, m_stumble * 5.0f));
		}
		
		// Increase the players progress across the current cell, taking into accound current run speed and stumbling
		m_playerTimer += 
			Time.deltaTime * (m_stumble > 0.0f ? m_playerRunSpeed / 4.0f : m_playerRunSpeed);

		
		// If we reach the end of a cell, we need to move to the next one (or possibly die!)
		if (m_playerTimer >= 1.0f)
		{
			m_distanceRun += 10.0f;
			m_playerTimer  = 0.0f;

			m_previousCellDirection = m_playerCell.CellDirection;

			// Determine which cell to move the player to.
			switch (m_playerCell.CellDirection)
			{
				case enCellDir.North:
					m_playerCell = m_playerCell.NeighbourN;
					break;

				case enCellDir.South:
					m_playerCell = m_playerCell.NeighbourS;
					break;

				case enCellDir.East:
					m_playerCell = m_playerCell.NeighbourE;
					break;

				case enCellDir.West:
					m_playerCell = m_playerCell.NeighbourW;
					break;
			}			
		}
		
		// Tell the current cell it's been visited, so it can be removed later.
		m_playerCell.Visited = true;		

		// If current cell is unspecified (usually on first run) then reset some stuff.
		if (m_playerCell == null)
		{
			m_playerCell            = m_cells[0];
			m_playerDirection       = enCellDir.North;
			m_playerNextDirection   = enCellDir.North;
			m_previousCellDirection = enCellDir.North;
		}
		
		#region Obstacles
		
		// here, we check for the various ways in which the player can screw up, and die.

		// Is the current cell an obstacle that should be ducked under?
		if (m_playerCell.CellType == enCellType.DuckObstacle)
		{			
			if (m_playerTimer >= 0.4f && m_playerTimer <= 0.6f)
			{				
				if (m_playerJump >-1.0f || m_playerSlide <=0.0f)
				{
					if (m_playerSlide <=0.0f && m_playerJump <=-1.0f)
					{
						m_player.transform.Translate(Vector3.up * 0.1f);
					}

					DoRagDoll(true, false, m_splatAudio);
					m_dieReason = "Ouch!  Migraine.  Duck next time!";
				}
			}
		}
		
		// Is the current cell an obstacle that should be jumped (boxes in this case)?
		if (m_playerCell.CellType == enCellType.JumpObstacle &&
            m_playerJump  <= -1.0f && 
			m_playerTimer >= 0.45f &&
			m_playerTimer <= 0.55f &&
			m_stumble     <=0.0f)		
		{
			DoRagDoll(true, false, m_splatAudio);
			m_dieReason = "Don't forget to jump.  Boxes hurt.";
		}

		// Are we on a narrow ledge, and not leaning towards the opposite side?
		if (m_playerCell.CellType == enCellType.LedgeLeft)
		{
			if (m_tilt >=-0.05f && m_playerTimer >=0.3f && m_playerTimer <=0.6f && m_playerJump <=-1.0f) 
			{
				DoRagDoll(false, false, m_playerCell.CellTheme == enCellTheme.Stone ? m_splatAudio : m_fallAudio);
				m_dieReason = "Watch your footing!  Use the mouse to slide to the side.";
			}
		}
		
		// Are we on a narrow ledge, and not leaning towards the opposite side?
		if (m_playerCell.CellType == enCellType.LedgeRight)
		{			
			if (m_tilt <=0.05f && m_playerTimer >=0.3f && m_playerTimer <=0.6f && m_playerJump <=-1.0f) 
			{
				DoRagDoll(false, false, m_playerCell.CellTheme == enCellTheme.Stone ? m_splatAudio : m_fallAudio);
				m_dieReason = "Watch your footing!  Use the mouse to slide to the side.";
			}
		}
		
		// Should we be jumping over a gap?
		if (m_playerCell.CellType == enCellType.JumpGap && m_playerJump <=-1.0f)
		{
			if (m_playerTimer > 0.3f && m_playerTimer < 0.7f)
			{
				if (m_playerTimer <  0.4f) DoRagDoll(false, false, m_fallAudio);
				if (m_playerTimer >= 0.4f) DoRagDoll(false, false, m_splatAudio);

				m_dieReason = "Swan dive with triple bone-crunch!  (don't forget to jump at the right time...)";
			}
		}

		//If we are on a straight, and the player has changed direction, we should "stumble"
		if ( (m_previousCellDirection    == m_playerCell.CellDirection) &&
			 (m_playerCell.CellDirection != m_playerNextDirection) && m_stumble <=0.0f)
		{
			m_stumble = 1.0f;
			m_playerNextDirection = m_playerDirection;
			AudioSource.PlayClipAtPoint(m_stumbleAudio, m_player.transform.position);
			m_player.animation.Blend("flip");
		}

		// Change Player Direction if we are on a turn cell
		if (m_playerTimer >= 0.5f && m_previousCellDirection != m_playerCell.CellDirection)
		{			
			// If we haven't already
			if (m_playerDirection != m_playerNextDirection)
			{
				// Set player direction to player next direction (from input)
				m_playerDirection = m_playerNextDirection;			
				AudioSource.PlayClipAtPoint(m_turnAudio, m_player.transform.position);
			}

			// If player is not travelling in the correct direction, then we destroy player and create ragdoll
			if (m_playerDirection !=  m_playerCell.CellDirection)
			{
				if (m_playerJump > -1.0f)
				{			
					DoRagDoll(true, false, m_fallAudio);
					m_dieReason = "Wheeeeeeeee!";
				}
				else
				{
					DoRagDoll(false, false, m_splatAudio);
					m_dieReason = "Good job your face was there to take the brunt of the impact.";
				}
			}	
		}	

		#endregion	
		
		// This is a cheap trick as we didn't have a slide animation.  Simply put the character on his back and play a mini-jump animation.
		// Hey, if it works...
		float xRot = m_playerSlide >0.0f ? -70.0f : 0.0f;

		// Change Player Rotation
		if (m_playerDirection == enCellDir.North) m_player.transform.rotation = Quaternion.Euler(xRot,000,0);
		if (m_playerDirection == enCellDir.South) m_player.transform.rotation = Quaternion.Euler(xRot,180,0);
		if (m_playerDirection == enCellDir.East ) m_player.transform.rotation = Quaternion.Euler(xRot,090,0);
		if (m_playerDirection == enCellDir.West ) m_player.transform.rotation = Quaternion.Euler(xRot,270,0);
		
		// Update player position
		Vector3 pos = 
			m_playerCell.CellPosition;
		
		float offset = 
			(m_playerTimer * m_cellSpacing) - (m_cellSpacing / 2.0f);
		
		switch (m_playerDirection)
		{
			case enCellDir.North:
				pos.z += offset;
				break;

			case enCellDir.South:
				pos.z -= offset;
				break;

			case enCellDir.East:
				pos.x += offset;
				break;

			case enCellDir.West:
				pos.x -= offset;
				break;
		}	

		pos.y = m_playerBaseHeight;
		
		// Do Jumping 
		if (m_playerJump > -1.0f)
		{			
			// This controls how fast the jump happens
			// Tweak at your peril.  Too long, jumps are easy, too short, you'll never make it.
			m_playerJump -= Time.deltaTime * (m_playerRunSpeed * 3.5f);		
			m_playerYvel += m_playerJump;

			// This controls how high the player jumps.  Has no effect on gameplay.
			pos.y += m_playerYvel * 0.05f;			
		}
		
		// Do Sliding
		if (m_playerSlide >0.0f)
		{
			m_playerSlide -= Time.deltaTime* (m_playerRunSpeed * 1.5f);
		}
		
		// Set the player's position taking everything above into account.
		m_player.transform.position = pos;	
		
		// Strafing, based on mouse input (to simulate tilting the phone, ala Temple Run).
		m_tilt += Input.GetAxis("Mouse X") * 0.035f;		
		m_tilt = Mathf.Clamp(m_tilt, -0.35f, 0.35f);
		m_tilt = Mathf.Lerp(m_tilt, 0.0f, Time.deltaTime * 2.0f);
		m_player.transform.Translate(Vector3.right * m_tilt);			
	}
	
	// Does exactly what it says on the tin.
	// Used to clean-up before creating a new track.
	private void DestroyAllCells()
	{
		// Destroy EVERYTHING
		for (int k = m_cells.Count-1;k>=0;k--)
		{
			if (m_cells[k].m_deco.Count>0)
			{
				for (int l=m_cells[k].m_deco.Count-1;l>=0;l--)
				{
					Destroy(m_cells[k].m_deco[l]);
				}

				m_cells[k].m_deco.Clear();
				m_cells[k].m_deco.TrimExcess();
			}
			
			if (m_cells[k].m_coins.Count>0)
			{
				for (int l=m_cells[k].m_coins.Count-1;l>=0;l--)
				{
					Destroy(m_cells[k].m_coins[l]);
				}

				m_cells[k].m_coins.Clear();
				m_cells[k].m_coins.TrimExcess();

			}
			
			if (m_cells[k].m_mines.Count>0)
			{			
				for (int l=m_cells[k].m_mines.Count-1;l>=0;l--)
				{
					Destroy(m_cells[k].m_mines[l]);
				}
	
				m_cells[k].m_mines.Clear();
				m_cells[k].m_mines.TrimExcess();
			}

			Destroy(m_cells[k].CellModel);
			m_cells.RemoveAt(k);			
		}	
		
		m_cells.Clear();
		m_cells.TrimExcess();
		m_cells = new List<stCell>();
	}

	// Start cell creation process
	private void CreateCells()
	{
		// Setup Root Cell
		stCell rootCell = 
			new stCell
				(Vector3.zero, enCellDir.North, enCellType.Run);

		rootCell.CellPosition.y
			= m_tileBaseHeightRuin;

		m_cells.Add(rootCell);

		GameObject newCellModel = 
				(GameObject)Instantiate
					(m_ITile, rootCell.CellPosition, Quaternion.identity);
	
			newCellModel.transform.parent 
				= this.transform;

		rootCell.CellModel = newCellModel;
		
		// Create track segment ready for player to run along
		CreateNewCellsIfNeeded(true);	
		
	}
	
	// Dependant on a number of factors (neighbouring cells, randomness) change the provided cell into an obstacle.
	private void CreateObstacle(stCell cell)
	{
		int k = m_cells.IndexOf(cell);

		if (cell.CellType == enCellType.Run)
		{
			if (cell.CellDirection == enCellDir.North || cell.CellDirection == enCellDir.South)
			{
				if (cell.NeighbourN != null && cell.NeighbourS != null)
				{						
					if (cell.NeighbourN.CellDirection == cell.CellDirection  &&
						cell.NeighbourS.CellDirection == cell.CellDirection )
					{
						// Decide whether or not to change this tile into something else
						if (Random.Range(0.0f, 1.0f)>0.1f)
						{
							int i = Random.Range(0,4);
	
							switch (i)
							{
								// Possibly make this a broken piece that needs to be jumped
								case 0:
							
									cell.CellType = enCellType.JumpGap;
	
									// Maybe change cell theme
									if (Random.Range(0.0f, 1.0f)>0.5f)
									{
										for (int l=k+1;l<m_cells.Count;l++)
										{
											stCell c = m_cells[l];
											c.CellTheme = (c.CellTheme == enCellTheme.Stone ? enCellTheme.Bridge : enCellTheme.Stone);
											c.CellPosition.y = (c.CellTheme == enCellTheme.Stone? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
											m_cells[l] = c;										
										}
									}
	
									break;
	
								// Possibly make this a narrow tile
								case 1:
									cell.CellType = (Random.Range(0.0f, 1.0f) >= 0.5f ? enCellType.LedgeLeft : enCellType.LedgeRight);
									break;
								
								// Possibly make this a jump obstacle
								case 2:
									cell.CellType = enCellType.JumpObstacle;
									cell.CellPosition.y = m_tileBaseHeightRuin; //(cell.CellTheme == enCellTheme.Stone? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
									break;
								// Possibly make this a duck obstacle
								case 3:
									cell.CellType = enCellType.DuckObstacle;
									cell.CellPosition.y = m_tileBaseHeightRuin; //(cell.CellTheme == enCellTheme.Stone? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
									break;
							}
						}						
					}
				}
			}				
			
			if (cell.CellDirection == enCellDir.East || cell.CellDirection == enCellDir.West)
			{	
				if (cell.NeighbourE != null && cell.NeighbourW != null)
				{						
					if (cell.NeighbourE.CellDirection == cell.CellDirection &&
						cell.NeighbourW.CellDirection == cell.CellDirection )
					{
						// Decide whether or not to change this tile into something else
						if (Random.Range(0.0f, 1.0f)>0.1f)
						{
							int i = Random.Range(0,4);
	
							switch (i)
							{
								// Possibly make this a broken piece that needs to be jumped
								case 0:
							
									cell.CellType = enCellType.JumpGap;
	
									// Maybe change cell theme
									if (Random.Range(0.0f, 1.0f)>0.5f)
									{
										for (int l=k+1;l<m_cells.Count;l++)
										{
											stCell c = m_cells[l];
											c.CellTheme = (c.CellTheme == enCellTheme.Stone ? enCellTheme.Bridge : enCellTheme.Stone);
											c.CellPosition.y = (c.CellTheme == enCellTheme.Stone? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
											m_cells[l] = c;										
										}
									}
	
									break;
	
								// Possibly make this a narrow tile
								case 1:
									cell.CellType = (Random.Range(0.0f, 1.0f) >= 0.5f ? enCellType.LedgeLeft : enCellType.LedgeRight);
									break;
	
								// Possibly make this a jump obstacle
								case 2:
									cell.CellType = enCellType.JumpObstacle;
									cell.CellPosition.y = m_tileBaseHeightRuin; //(cell.CellTheme == enCellTheme.Stone? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
									break;
								// Possibly make this a duck obstacle
								case 3:
									cell.CellType = enCellType.DuckObstacle;
									cell.CellPosition.y = m_tileBaseHeightRuin; //(cell.CellTheme == enCellTheme.Stone? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
									break;
	
							}
						}						
					}
				}
			}
		}
	}

	// Add some stuff in the water around the current cell to make the game more interesting (rocks, trees, ruins, etc).
	private void CreateScenery(stCell cell)
	{
		// We never decorate the "last" cell, since we might end up blocking off the route when new track get's created.
		// And yes I could have cleaned up existing decorations when this happens but couldn't be arsed.	
		if (m_cells.IndexOf(cell) == m_cells.Count-1) return;
		
		// Randomly place archways over the track (as long as we're not on a corner).
		if (Random.Range(0.0f, 1.0f) > 0.25f)
		{
			Quaternion archRot = Quaternion.identity;

			if (cell.CellDirection == enCellDir.North || cell.CellDirection == enCellDir.South) archRot = Quaternion.Euler(0,0,0);
			if (cell.CellDirection == enCellDir.East  || cell.CellDirection == enCellDir.West)  archRot = Quaternion.Euler(0,90,0);

			bool bOkArch = true;

			switch (cell.CellDirection)
			{
				case enCellDir.North:
					
					if (cell.NeighbourS != null) 
					{
						bOkArch = true;
					}

					if (cell.NeighbourW != null) 
					{
						bOkArch = false;
					}

					if (cell.NeighbourE != null) 
					{
						bOkArch = false;
					}
					
					break;

				case enCellDir.East:
					
					if (cell.NeighbourW != null) 
					{
						bOkArch = true;
					}

					if (cell.NeighbourS != null) 
					{
						bOkArch = false;
					}
					
					break;

				case enCellDir.West:
					
					if (cell.NeighbourE != null) 
					{
						bOkArch = true;
					}

					if (cell.NeighbourS != null) 
					{
						bOkArch = false;
					}

					if (cell.NeighbourN != null) 
					{
						bOkArch = false;
					}
					
					break;

			}
			
			Vector3 pos = cell.CellPosition;
			if (cell.CellTheme == enCellTheme.Bridge) pos.y = m_tileBaseHeightRuin;
			
			if (bOkArch)
			{
				GameObject arch = (GameObject)Instantiate(m_Arch, pos, archRot);
				cell.m_deco.Add(arch);
			}
		}
		
		// Create Scenery
		if (cell.NeighbourW == null && Random.Range(0.0f, 1.0f) > 0.25f)
		{
			int sceneryID = Random.Range(0,8);
			GameObject sceneryPrefab = null;

			switch (sceneryID)
			{
				case 0: sceneryPrefab = m_Rock1;  break;
				case 1: sceneryPrefab = m_Rock2;  break;
				case 2: sceneryPrefab = m_Rock3;  break;
				case 3: sceneryPrefab = m_Rock4;  break;
				case 4: sceneryPrefab = m_Column; break;					
				case 5: sceneryPrefab = m_Tree;   break;					
				case 6: sceneryPrefab = m_Ruin1;  break;							
				case 7: sceneryPrefab = m_Ruin2;  break;					
			}

			Vector3 pos = cell.CellPosition + Vector3.left * m_cellSpacing / 3.0f;
			pos.y = -0.5f;

			GameObject sceneryObject = (GameObject)Instantiate(sceneryPrefab, pos, Quaternion.Euler(0.0f, Random.Range(0,3) * 90, 0.0f)); 			
			sceneryObject.transform.parent = this.transform;
			cell.m_deco.Add(sceneryObject);
		}	

		// Create Scenery
		if (cell.NeighbourE == null && Random.Range(0.0f, 1.0f) > 0.25f)
		{
			int sceneryID = Random.Range(0,8);
			GameObject sceneryPrefab = null;

			switch (sceneryID)
			{
				case 0: sceneryPrefab = m_Rock1;  break;
				case 1: sceneryPrefab = m_Rock2;  break;
				case 2: sceneryPrefab = m_Rock3;  break;
				case 3: sceneryPrefab = m_Rock4;  break;
				case 4: sceneryPrefab = m_Column; break;					
				case 5: sceneryPrefab = m_Tree;   break;					
				case 6: sceneryPrefab = m_Ruin1;  break;							
				case 7: sceneryPrefab = m_Ruin2;  break;				
			}

			Vector3 pos = cell.CellPosition + Vector3.right * m_cellSpacing * 0.5f;	
			pos.y = -0.5f;

			GameObject sceneryObject = (GameObject)Instantiate(sceneryPrefab, pos, Quaternion.Euler(0.0f, Random.Range(0,3) * 90, 0.0f)); 			
			sceneryObject.transform.parent = this.transform;
			cell.m_deco.Add(sceneryObject);
		}				
		
		// Create Scenery
		if (cell.NeighbourN == null && Random.Range(0.0f, 1.0f) > 0.25f)
		{
			int sceneryID = Random.Range(0,8);
			GameObject sceneryPrefab = null;

			switch (sceneryID)
			{
				case 0: sceneryPrefab = m_Rock1;  break;
				case 1: sceneryPrefab = m_Rock2;  break;
				case 2: sceneryPrefab = m_Rock3;  break;
				case 3: sceneryPrefab = m_Rock4;  break;
				case 4: sceneryPrefab = m_Column; break;					
				case 5: sceneryPrefab = m_Tree;   break;					
				case 6: sceneryPrefab = m_Ruin1;  break;							
				case 7: sceneryPrefab = m_Ruin2;  break;				
			}

			Vector3 pos = cell.CellPosition + Vector3.forward * m_cellSpacing * 0.5f;	
			pos.y = -0.5f;

			GameObject sceneryObject = (GameObject)Instantiate(sceneryPrefab, pos, Quaternion.Euler(0.0f, Random.Range(0,3) * 90, 0.0f)); 			
			sceneryObject.transform.parent = this.transform;
			cell.m_deco.Add(sceneryObject);
		}	
		
		// Create Scenery
		if (cell.NeighbourS == null && Random.Range(0.0f, 1.0f) > 0.25f)
		{
			int sceneryID = Random.Range(0,8);
			GameObject sceneryPrefab = null;

			switch (sceneryID)
			{
				case 0: sceneryPrefab = m_Rock1;  break;
				case 1: sceneryPrefab = m_Rock2;  break;
				case 2: sceneryPrefab = m_Rock3;  break;
				case 3: sceneryPrefab = m_Rock4;  break;
				case 4: sceneryPrefab = m_Column; break;					
				case 5: sceneryPrefab = m_Tree;   break;					
				case 6: sceneryPrefab = m_Ruin1;  break;							
				case 7: sceneryPrefab = m_Ruin2;  break;					
			}

			Vector3 pos = cell.CellPosition + Vector3.back * m_cellSpacing * 0.5f;	
			pos.y = -0.5f;

			GameObject sceneryObject = (GameObject)Instantiate(sceneryPrefab, pos, Quaternion.Euler(0.0f, Random.Range(0,3) * 90, 0.0f)); 			
			sceneryObject.transform.parent = this.transform;
			cell.m_deco.Add(sceneryObject);
		}
	}
	
	// Add landmine to the provided cell.  Maybe.
	private void CreateLandMine(stCell cell)
	{
		// Create LandMines!!!
		if (Random.Range(0.0f, 1.0f) > 0.5f && m_cells.IndexOf(cell) > 2)
		{	
			// We only create landmines on "standard" cells
			if (cell.CellType == enCellType.Run)
			{
				// And not in corners.  That wouldn't be fair.
				if (!isCornerCell(cell))
				{
					// Create the landmine.
					GameObject landMine = (GameObject)Instantiate(m_LandMine, cell.CellPosition + Vector3.up * (cell.CellTheme == enCellTheme.Stone? 0.8f : 0.1f), Quaternion.identity);					
					cell.m_mines.Add(landMine);
					
					// Decide where to place the warning sign based on cell orientation
					Vector3 lmPos = cell.CellPosition + Vector3.up * (cell.CellTheme == enCellTheme.Stone? 1.0f : 0.05f);
					Quaternion lmRot = Quaternion.identity;

					if (cell.CellDirection == enCellDir.North)
					{
						lmPos.x -= (m_cellSpacing * 0.085f);
						lmPos.z -= (m_cellSpacing * 0.6f);
					}

					if (cell.CellDirection == enCellDir.East)
					{
						lmPos.z += (m_cellSpacing * 0.085f);
						lmPos.x -= (m_cellSpacing * 0.6f);
						lmRot = Quaternion.Euler(0,90,0);						
					}
					
					if (cell.CellDirection == enCellDir.West)
					{
						lmPos.z += (m_cellSpacing * 0.085f);
						lmPos.x += (m_cellSpacing * 0.6f);
						lmRot = Quaternion.Euler(0,270,0);						
					}
					
					// Create warning sign.
					GameObject warningSign 	= (GameObject)Instantiate(m_LandMineSign, lmPos, lmRot);
					cell.m_deco.Add(warningSign);		
				}					
			}
		}
	}
	
	// Randomly place coins on the provided cell
	private void CreateCoins(stCell cell)
	{
		// Create Coins!
		if (Random.Range(0.0f, 1.0f) > 0.25f)
		{
			float r = Random.Range(-1,1);
			
			// Offset placement if on a ledge, so player can get the coins.  Aren't we nice?

			if (cell.CellType == enCellType.LedgeLeft  && cell.CellDirection == enCellDir.North) r = -1;
			if (cell.CellType == enCellType.LedgeRight && cell.CellDirection == enCellDir.North) r =  1;
			
			if (cell.CellType == enCellType.LedgeLeft  && cell.CellDirection == enCellDir.East) r =  1;
			if (cell.CellType == enCellType.LedgeRight && cell.CellDirection == enCellDir.East) r = -1;

			Vector3 laneOffset = Vector3.zero;

			if (cell.CellDirection == enCellDir.North || cell.CellDirection == enCellDir.South)
				laneOffset.x = r * 0.3f;

			if (cell.CellDirection == enCellDir.East || cell.CellDirection == enCellDir.West)
				laneOffset.z = r * 0.3f;				

			for (float k=0.5f; k<1.0f; k+=0.1f)
			{
				Vector3 pos = cell.CellPosition;
				pos.y = m_coinHeight;

				float offset = 
					(k * m_cellSpacing) - (m_cellSpacing / 2.0f);					
	
				switch (cell.CellDirection)
				{
					case enCellDir.North:
						pos.z += offset;
						break;
		
					case enCellDir.South:
						pos.z -= offset;
						break;
		
					case enCellDir.East:
						pos.x += offset;
						break;
		
					case enCellDir.West:
						pos.x -= offset;
						break;
				}

				if (cell.CellType == enCellType.JumpGap || cell.CellType == enCellType.JumpObstacle)
				{
					pos.y += 0.5f + (Mathf.Sin(k * 10.0f) * 0.1f);
					if (cell.CellDirection == enCellDir.North) pos.z -= (m_cellSpacing / 5.0f);
					if (cell.CellDirection == enCellDir.East)  pos.x -= (m_cellSpacing / 5.0f);
				}

				GameObject coin = (GameObject)Instantiate(m_GoldCoin, pos + laneOffset, Quaternion.Euler(0, 360.0f / (k + 0.5f),0));
				cell.m_coins.Add(coin);
			}	
		}
	}
	
	// Recursive function to create cells.  
	private bool CreateNextCell(stCell previous, int cellCount)
	{
		// Can't do any more?  
		if (cellCount == m_maxCells)
			return false;

		// Create New Cell
		stCell newCell = 
			new stCell
				(Vector3.zero, enCellDir.North, enCellType.Run);	
		
		stCell prevCell	= previous;
		
		// Determine position of new cell
		Vector3 newCell_Position 
			= prevCell.CellPosition;
		
		// Determine Height of new cell
		newCell_Position.y 
			= (newCell.CellTheme == enCellTheme.Stone ? m_tileBaseHeightRuin : m_tileBaseHeightBridge);
		
		// Offset new cell from it's previous cell based on direction
		switch (prevCell.CellDirection)
		{
			case enCellDir.North:
				newCell_Position.z += m_cellSpacing;
				break;

			case enCellDir.South:
				newCell_Position.z -= m_cellSpacing;
				break;

			case enCellDir.East:
				newCell_Position.x += m_cellSpacing;
				break;

			case enCellDir.West:
				newCell_Position.x -= m_cellSpacing;
				break;
		}	

		newCell.CellPosition 
			= newCell_Position;
		
		newCell.CellDirection = prevCell.CellDirection;

		// Should we change direction?
		bool changeDirection = 
			( cellCount > 2 && (Random.Range(0.0f, 1.0f) > 0.65f ? true : false) ); // && (float)cellCount % 2.0f == 0.0f);		
		
		if (changeDirection)		
		{
			// Determine whether to turn left or right
			// Bit of a dirty trick, but we don't yet cater for T junctions or doubling back.
			// This prevents the track from ever crossing itself.

			float LR = (m_lastTurnDir < 0.0f? -0.5f : 0.5f);
			m_lastTurnDir *= -1.0f;

			switch (newCell.CellDirection)
			{
				case enCellDir.North:
					newCell.CellDirection = LR < 0.0f ? enCellDir.West : enCellDir.East;
					break;

				case enCellDir.South:
					newCell.CellDirection = LR < 0.0f ? enCellDir.East : enCellDir.West;
					break;

				case enCellDir.East:
					newCell.CellDirection = LR < 0.0f ? enCellDir.North : enCellDir.South;
					break;

				case enCellDir.West:
					newCell.CellDirection = LR < 0.0f ? enCellDir.South : enCellDir.North;
					break;
			}
		}	

		//TODO: "Look ahead" to make sure we are not going to run into existing cells, and change direction if we are.	
		// Maybe in the next version.			
		
		// Link cells together using neighbour indices	
		if (prevCell.CellDirection == enCellDir.North)
		{
			prevCell.NeighbourN = newCell;
			newCell .NeighbourS = previous;
		}

		if (prevCell.CellDirection == enCellDir.South)
		{
			prevCell.NeighbourS = newCell;
			newCell .NeighbourN = previous;
		}

		if (prevCell.CellDirection == enCellDir.East)
		{
			prevCell.NeighbourE = newCell;
			newCell .NeighbourW = previous;
		}

		if (prevCell.CellDirection == enCellDir.West)
		{
			prevCell.NeighbourW = newCell;
			newCell .NeighbourE = previous;
		}

		// Add new cell to list	
		m_cells.Add (newCell);		
		
		// Recursive call to create next cell
		CreateNextCell(newCell, cellCount + 1);
		
		return true;		
	}
	
	// places the correct 3d model in the scene for a cell based on direction, type and neighbours
	// Simple nested switch statements and model rotations, nothing complex here.
	private void CreateCellModel(stCell cell)
	{
		GameObject prefabToInstantiate 
			= null;

		Quaternion rotation = Quaternion.identity;

		switch (cell.CellType)
		{
			case enCellType.JumpObstacle:

				if (cell.CellTheme == enCellTheme.Stone ) prefabToInstantiate = m_ITile_ObstacleJump;
				if (cell.CellTheme == enCellTheme.Bridge) prefabToInstantiate = m_Bridge_ITile_ObstacleJump;

				if (cell.CellDirection == enCellDir.North || cell.CellDirection == enCellDir.South) rotation = Quaternion.Euler(0.0f,  0.0f, 0.0f);
				if (cell.CellDirection == enCellDir.East  || cell.CellDirection == enCellDir.West ) rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

				break;

			case enCellType.DuckObstacle:

				if (cell.CellTheme == enCellTheme.Stone ) prefabToInstantiate = m_ITile_ObstacleDuck;
				if (cell.CellTheme == enCellTheme.Bridge) prefabToInstantiate = m_Bridge_ITile_ObstacleDuck;

				if (cell.CellDirection == enCellDir.North || cell.CellDirection == enCellDir.South) rotation = Quaternion.Euler(0.0f,  0.0f, 0.0f);
				if (cell.CellDirection == enCellDir.East  || cell.CellDirection == enCellDir.West)  rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

				break;
				
			case enCellType.LedgeLeft:

				switch (cell.CellDirection)
				{	
					case enCellDir.North:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,0.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,0.0f,0.0f);

								break;
						}

						break;

					case enCellDir.South:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,180.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,180.0f,0.0f);

								break;
						}

						break;

					case enCellDir.East:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,90.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,90.0f,0.0f);

								break;
						}

						break;

					case enCellDir.West:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,270.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,270.0f,0.0f);

								break;
						}

						break;
				}

				break;
			
			case enCellType.LedgeRight:

				switch (cell.CellDirection)
				{	
					case enCellDir.North:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,180.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,180.0f,0.0f);

								break;
						}

						break;

					case enCellDir.South:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,0.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,0.0f,0.0f);

								break;
						}

						break;

					case enCellDir.East:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,270.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,270.0f,0.0f);

								break;
						}

						break;

					case enCellDir.West:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:

								prefabToInstantiate = m_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,90.0f,0.0f);

								break;

							case enCellTheme.Bridge:

								prefabToInstantiate = m_Bridge_ITileBrokenHalf;
								rotation = Quaternion.Euler(0.0f,90.0f,0.0f);

								break;
						}

						break;
				}

				break;

			case enCellType.JumpGap:

				switch (cell.CellDirection)
				{	
					case enCellDir.North:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:
								
								if (cell.NeighbourN.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_ITileBroken;
									rotation = Quaternion.Euler(0.0f,0.0f,0.0f);
								}

								if (cell.NeighbourN.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,0.0f,0.0f);									
								}

								break;

							case enCellTheme.Bridge:
					
								if (cell.NeighbourN.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge_ITileBroken;
									rotation = Quaternion.Euler(0.0f,0.0f,0.0f);
								}

								if (cell.NeighbourN.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,180.0f,0.0f);
									m_cells[m_cells.IndexOf(cell)].CellPosition.y = m_tileBaseHeightRuin;
								}
								
								break;
						}
						
						break;

					case enCellDir.South:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:
								
								if (cell.NeighbourN.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_ITileBroken;
									rotation = Quaternion.Euler(0.0f,180.0f,0.0f);
								}

								if (cell.NeighbourN.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,180.0f,0.0f);									
								}

								break;

							case enCellTheme.Bridge:
					
								if (cell.NeighbourN.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge_ITileBroken;
									rotation = Quaternion.Euler(0.0f,180.0f,0.0f);
								}

								if (cell.NeighbourN.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,0.0f,0.0f);
									m_cells[m_cells.IndexOf(cell)].CellPosition.y = m_tileBaseHeightRuin;
								}
								
								break;
						}
						
						break;

					case enCellDir.East:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:
								
								if (cell.NeighbourE.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_ITileBroken;
									rotation = Quaternion.Euler(0.0f,90.0f,0.0f);
								}

								if (cell.NeighbourE.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,90.0f,0.0f);									
								}

								break;

							case enCellTheme.Bridge:
					
								if (cell.NeighbourE.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge_ITileBroken;
									rotation = Quaternion.Euler(0.0f,90.0f,0.0f);
								}

								if (cell.NeighbourE.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,270.0f,0.0f);
									m_cells[m_cells.IndexOf(cell)].CellPosition.y = m_tileBaseHeightRuin;
								}
								
								break;
						}
						
						break;	

					case enCellDir.West:

						switch (cell.CellTheme)
						{
							case enCellTheme.Stone:
								
								if (cell.NeighbourE.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_ITileBroken;
									rotation = Quaternion.Euler(0.0f,270.0f,0.0f);
								}

								if (cell.NeighbourE.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,270.0f,0.0f);									
								}

								break;

							case enCellTheme.Bridge:
					
								if (cell.NeighbourE.CellTheme == enCellTheme.Bridge)
								{
									prefabToInstantiate = m_Bridge_ITileBroken;
									rotation = Quaternion.Euler(0.0f,270.0f,0.0f);
								}

								if (cell.NeighbourE.CellTheme == enCellTheme.Stone)
								{
									prefabToInstantiate = m_Bridge2Stone_ITile;
									rotation = Quaternion.Euler(0.0f,90.0f,0.0f);
									m_cells[m_cells.IndexOf(cell)].CellPosition.y = m_tileBaseHeightRuin;
								}
								
								break;
						}
						
						break;								
				}

				break;

			case enCellType.Run:

				switch (cell.CellDirection)
				{
					case enCellDir.North:
						
						if (cell.NeighbourS != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_ITile : m_Bridge_ITile);		
							rotation = Quaternion.Euler(0.0f,0.0f,0.0f);				
						}

						if (cell.NeighbourW != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_LTile : m_Bridge_LTile);		
							rotation = Quaternion.Euler(0.0f,270.0f,0.0f);				
						}

						if (cell.NeighbourE != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_LTile : m_Bridge_LTile);		
							rotation = Quaternion.Euler(0.0f,0.0f,0.0f);				
						}
						
						break;

					case enCellDir.East:
						
						if (cell.NeighbourW != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_ITile : m_Bridge_ITile);		
							rotation = Quaternion.Euler(0.0f,90.0f,0.0f);				
						}

						if (cell.NeighbourS != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_LTile : m_Bridge_LTile);		
							rotation = Quaternion.Euler(0.0f,90.0f,0.0f);				
						}
						
						break;

					case enCellDir.West:
						
						if (cell.NeighbourE != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_ITile : m_Bridge_ITile);		
							rotation = Quaternion.Euler(0.0f,270.0f,0.0f);				
						}

						if (cell.NeighbourS != null) 
						{
							prefabToInstantiate = (cell.CellTheme == enCellTheme.Stone ? m_LTile : m_Bridge_LTile);		
							rotation = Quaternion.Euler(0.0f,180.0f,0.0f);				
						}
						
						break;


				}

				break;				
		}
		
		if (prefabToInstantiate != null)
		{
			GameObject newCellModel = 
				(GameObject)Instantiate
					(prefabToInstantiate, cell.CellPosition, rotation);
	
			newCellModel.transform.parent 
				= this.transform;

			cell.CellModel = newCellModel;
		}
	}
	
	// Spawn a ragdoll and destroy the player on death.
	private void DoRagDoll(bool upwardForce, bool IsLandMine, AudioClip clip)
	{
		Vector3	ragdollPos = m_player.transform.position;
				ragdollPos.y += 0.25f;

		float ragDollForwardForce = 150;
		float upForce = upwardForce == true ? 25.0f : 0.0f;	

		if (IsLandMine==true) 
		{ 
			ragDollForwardForce *=-0.25f;
			upForce = 50.0f;
		}		
				
		m_playerRagDoll = (GameObject)Instantiate(m_ragdollPrefab, ragdollPos, m_player.transform.rotation);	

		// Copy Bone Transforms!  Make's ragdoll that little bit more realistic...

		foreach (Transform rag in m_playerRagDoll.GetComponentsInChildren(typeof(Transform)))
		{
			foreach (Transform mdl in m_player.GetComponentsInChildren(typeof(Transform)))
			{
				if (rag.name==mdl.name) 
				{
					rag.position = mdl.position;
					rag.rotation = mdl.rotation;
				}
			}			
		}	
	
		switch (m_previousCellDirection)
		{
			case enCellDir.North:
				m_playerRagDoll.transform.Find("Bip001").rigidbody.AddForce(new Vector3(0.0f, upForce, ragDollForwardForce));
				break;

			case enCellDir.East:
				m_playerRagDoll.transform.Find("Bip001").rigidbody.AddForce(new Vector3(ragDollForwardForce, upForce, 0.0f));
				break;

			case enCellDir.West:
				m_playerRagDoll.transform.Find("Bip001").rigidbody.AddForce(new Vector3(-ragDollForwardForce, upForce, 0.0f));
				break;
		}

		Destroy(m_player);
		
		AudioSource.PlayClipAtPoint(clip, ragdollPos);		
	}	
	
	// Is the provided cell a corner?
	private bool isCornerCell(stCell cell)
	{
		if (cell.CellDirection == enCellDir.North)
		{
			if (cell.NeighbourE != null || cell.NeighbourW != null) return true;
		}

		if (cell.CellDirection == enCellDir.East)
		{
			if (cell.NeighbourN != null || cell.NeighbourS != null) return true;
		}

		if (cell.CellDirection == enCellDir.West)
		{
			if (cell.NeighbourN != null || cell.NeighbourS != null) return true;
		}

		return false;
	}

	#endregion
}