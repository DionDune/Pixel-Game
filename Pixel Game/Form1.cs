using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Pixel_Game
{
    public partial class Form1 : Form
    {
        Random random = new Random();

        private List<List<string>> Blocks = new List<List<string>>();
        private PlayerBlock Player = new PlayerBlock();

        private List<EntityBlock> Entities = new List<EntityBlock>();
        private List<Projectile> Projectiles = new List<Projectile>();

        private List<string> BiomeRegions = new List<string>();

        // Colors
        SolidBrush Color_Red;
        SolidBrush Color_Blue;
        SolidBrush Color_Grass;
        SolidBrush Color_Foliage;
        SolidBrush Color_Dirt;
        SolidBrush Color_Rock;
        SolidBrush Color_BedRock;
        SolidBrush Color_Sand;
        SolidBrush Color_RedSand;
        SolidBrush Color_Water;
        SolidBrush Color_Void;
        SolidBrush Color_Standard;
        SolidBrush Color_Enemy;
        SolidBrush Color_Default;

        SolidBrush Color_ButtonActive;
        SolidBrush Color_ButtonAttack;

        int GameTick;

        // Sizes
        int screenWidth;
        int screenHeight;
        int prevScreenPixelWidth;
        int prevScreenPixelHeight;
        int worldWidth;
        int worldHeight;

        int blockWidth;
        int blockHeight;

        //Camera
        int cameraOffset_x;
        int cameraOffset_y;
        int playerCameraOffset_Y;
        int playerCameraOffset_X;

        int blockBound_X_Right;
        int blockBound_Y_Right;
        int blockBound_X_Left;
        int blockBound_Y_Left;


        // Player Movement
        bool goLeft, goRight, goDown, goUp;
        bool Player_Jump;
        bool Player_ShiftMove;
        bool Player_AllowBunnyHop;
        bool Random_TracerActive;


        //Survival
        bool Player_isMortal;
        int Player_Health_RegainInterval;
        int Player_Health_RegainAmount;
        int Player_Breath_RegainInterval;
        int Player_Breath_RegainAmount;

        //Highlighter
        private bool Highlighter_Visible;
        private List<List<bool>> Highligher = new List<List<bool>>();
        private List<int> Mouse_PrevPosition = new List<int>();
        int Highlighter_Size;
        int Highlighter_placeChance;
        bool Highlighter_FluidReplace;

        //Mouse
        int Mouse_X;
        int Mouse_Y;

        //UI
        private List<UIItem> UIItems = new List<UIItem>();
        bool Mouse_Clicking;

        //Materials
        private List<string> MaterialSelector_Materials = new List<string>();
        private List<List<int>> PhysicsMaterial_Water = new List<List<int>>();
        private List<List<int>> PhysicsMaterial_Sand = new List<List<int>>();
        string MaterialSelector_Selected;








        public Form1()
        {
            InitializeComponent();

            StartGame();
        }  // rdRD

        public void StartGame()
        {
            GameTick = 0;

            // Sizes
            blockHeight = 15;
            blockWidth = 15;
            screenHeight = Screen.Height / blockHeight;
            screenWidth = Screen.Width / blockWidth;
            worldHeight = 860;//480;
            worldWidth = 3360;//1680;
            prevScreenPixelWidth = Screen.Width;
            prevScreenPixelHeight = Screen.Height;

            // Camera
            cameraOffset_x = 0;
            cameraOffset_y = 0;
            playerCameraOffset_Y = blockHeight / 2;
            playerCameraOffset_X = 0; // blockWidth / 2;

            // Player
            Player_isMortal = true;
            Player_Health_RegainInterval = 25;
            Player_Health_RegainAmount = 2;
            Player_Breath_RegainInterval = 1;
            Player_Breath_RegainAmount = 1;
            Player_AllowBunnyHop = false;
            Player = new PlayerBlock
            {
                x = (Screen.Width / 2) / blockWidth * blockWidth,
                y = (Screen.Height / 2) / blockHeight * blockHeight,
                Health_Max = 100,
                Health = 100,
                Breath_Max = 1000,
                Breath = 1000,
                JumpHeight = 12,
                Speed_Base = 0.33F,
                Speed_Max = 0.66F
            };
            Random_TracerActive = false;

            //Entities
            SpawnEntities();

            // Highlighter
            Highlighter_Visible = true;
            Highlighter_Size = 5;
            Highlighter_placeChance = 1; // Chance is 1 out of {num}
            Highlighter_FluidReplace = false;
            Mouse_GenOutline();

            // UI
            Mouse_PrevPosition = new List<int>() { 0, 0 };
            UI_Build();

            // Materials
            Colors_Generate();
            MaterialSelector_Materials = new List<string>() { "Default", "Sand", "Red Sand", "Water", null };
            MaterialSelector_Selected = MaterialSelector_Materials[0];
            PhysicsMaterial_Water = new List<List<int>>();
            PhysicsMaterial_Sand = new List<List<int>>();



            // Terrain Generation
            Block_Generation_Border();
            Terrain_Generation();


            gameTimer.Start();
        }




        /////////////////////////////////////////

        #region Block Generation

        private void Block_Generation_Border()
        {
            for (int y_pos = 0; y_pos < worldHeight; y_pos++)
            {
                Blocks.Add(new List<string>());

                for (int x_pos = 0; x_pos < worldWidth; x_pos++)
                {
                    if ((x_pos < 10 || x_pos > worldWidth - 10 || y_pos < 10 || y_pos > worldHeight - 10) && random.Next(1, 6) != 3)
                    {
                        Blocks[y_pos].Add("Red");
                    }
                    else
                    {
                        Blocks[y_pos].Add(null);
                    }
                }
            }
        }

        private List<int> Terrain_Generation_GenHeights()
        {
            int Height_ChangeAmount = 1;

            var GroundHeights = new List<int>();

            GroundHeights.Add(worldHeight / 3);

            for (int x_pos = 1; x_pos < worldWidth - 9; x_pos++)
            {
                int heightChangeDirection = random.Next(0, 5);

                // DOWN
                if (heightChangeDirection == 0 && GroundHeights[x_pos - 1] < worldHeight)
                {
                    if (random.Next(0, 18) == 0) // Changes by 2
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] + 2 * Height_ChangeAmount);
                    }
                    else // Changes by 1
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] + 1 * Height_ChangeAmount);
                    }
                }

                // UP
                else if (heightChangeDirection == 1 && GroundHeights[x_pos - 1] > 5)
                {
                    if (random.Next(0, 18) == 0) // Changes by 2
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] - 2 * Height_ChangeAmount);
                    }
                    else if (random.Next(0, 112) == 0) // Changes by 5
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] - 5 * Height_ChangeAmount);
                    }
                    else // Changes by 1
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] - 1 * Height_ChangeAmount);
                    }
                }

                // SAME
                else
                {
                    GroundHeights.Add(GroundHeights[x_pos - 1]);
                }
            }

            return GroundHeights;
        }

        private List<string> Terrain_Generation_GenBiomes()
        {
            var BiomeRegions = new List<string>();

            string Biome_Default = "Grass";

            string Biome_Type = Biome_Default;
            int Biome_Left = 0;

            for (int x_pos = 0; x_pos < worldWidth; x_pos++)
            {
                if (random.Next(0, 600) == 0 && Biome_Type != "Sand")
                {
                    Biome_Type = "Sand";
                    Biome_Left = random.Next(200, 500);
                }

                BiomeRegions.Add(Biome_Type);
                if (Biome_Type != Biome_Default)
                {
                    Biome_Left--;
                    if (Biome_Left == 0)
                    {
                        Biome_Type = Biome_Default;
                    }
                }
            }

            return BiomeRegions;
        }

        private void Terrain_Generation()
        {
            List<int> GroundHeights = Terrain_Generation_GenHeights();
            BiomeRegions = Terrain_Generation_GenBiomes();

            // Gens surface terrain
            for (int x_pos = 10; x_pos < GroundHeights.Count(); x_pos++)
            {
                string Block_Type = BiomeRegions[x_pos];
                Blocks[GroundHeights[x_pos]][x_pos] = Block_Type;

                if (Block_Type == "Sand")
                {
                    PhysicsMaterial_Sand.Add(new List<int>() { x_pos, GroundHeights[x_pos], 0 });
                }
                else if (Block_Type == "Red Sand")
                {
                    PhysicsMaterial_Sand.Add(new List<int>() { x_pos, GroundHeights[x_pos], 1 });
                }
            }

            // Gens sub terrain
            for (int y_pos = 10; y_pos < Blocks.Count(); y_pos++)
            {
                for (int x_pos = 10; x_pos < GroundHeights.Count(); x_pos++)
                {
                    if (y_pos > GroundHeights[x_pos])
                    {
                        string type = "Rock";

                        if (y_pos < GroundHeights[x_pos] + 15 + random.Next(-5, 5))
                        {
                            type = "Dirt";
                            if (BiomeRegions[x_pos] == "Sand")
                            {
                                type = BiomeRegions[x_pos];
                                PhysicsMaterial_Sand.Add(new List<int>() { x_pos, y_pos, 0 });
                            }
                        }
                        if (y_pos > Blocks.Count() - 50)
                        {
                            if (random.Next(y_pos, Blocks.Count() - (Blocks.Count() - y_pos) / 2) == y_pos)
                            {
                                type = "Bed Rock";
                            }
                        }
                        Blocks[y_pos][x_pos] = type;
                    }
                }
            }

            // Entities
            for (int x_pos = 10; x_pos < GroundHeights.Count() - 1; x_pos++)
            {
                if (random.Next(0, 40) == 1)
                {
                    Blocks[GroundHeights[x_pos] - 1][x_pos] = "Shrub";
                    Blocks[GroundHeights[x_pos] - 2][x_pos] = "Shrub";
                }
                if (random.Next(0, 200) == 1)
                {
                    Blocks[GroundHeights[x_pos] - 1][x_pos] = "Enemy";
                    Blocks[GroundHeights[x_pos] - 2][x_pos] = "Enemy";
                    Blocks[GroundHeights[x_pos] - 2][x_pos + 1] = "Enemy";
                    Blocks[GroundHeights[x_pos] - 1][x_pos + 1] = "Enemy";
                }
            }
        }

        private void Terrain_Generation_Flat()
        {
            int SurfaceHeight = worldHeight / 3;
            BiomeRegions = Terrain_Generation_GenBiomes();


            // Gens surface terrain
            for (int x_pos = 10; x_pos < worldWidth - 10; x_pos++)
            {
                string Block_Type = BiomeRegions[x_pos];
                Blocks[SurfaceHeight][x_pos] = Block_Type;

                if (Block_Type == "Sand")
                {
                    PhysicsMaterial_Sand.Add(new List<int>() { x_pos, SurfaceHeight, 0 });
                }
                else if (Block_Type == "Red Sand")
                {
                    PhysicsMaterial_Sand.Add(new List<int>() { x_pos, SurfaceHeight, 1 });
                }
            }

            // Gens sub terrain
            for (int y_pos = 10; y_pos < Blocks.Count() - 10; y_pos++)
            {
                for (int x_pos = 10; x_pos < worldWidth - 10; x_pos++)
                {
                    if (y_pos > SurfaceHeight)
                    {
                        string type = "Rock";

                        if (y_pos < SurfaceHeight + 15 + random.Next(-5, 5))
                        {
                            type = "Dirt";
                            if (BiomeRegions[x_pos] == "Sand")
                            {
                                type = BiomeRegions[x_pos];
                                PhysicsMaterial_Sand.Add(new List<int>() { x_pos, y_pos, 0 });
                            }
                        }
                        if (y_pos > Blocks.Count() - 50)
                        {
                            if (random.Next(y_pos, Blocks.Count() - (Blocks.Count() - y_pos) / 2) == y_pos)
                            {
                                type = "Bed Rock";
                            }
                        }
                        Blocks[y_pos][x_pos] = type;
                    }
                }
            }

            // Entities
            for (int x_pos = 10; x_pos < worldWidth - 10; x_pos++)
            {
                if (random.Next(0, 40) == -1)
                {
                    Blocks[SurfaceHeight - 1][x_pos] = "Shrub";
                    Blocks[SurfaceHeight - 2][x_pos] = "Shrub";
                }
                if (random.Next(0, 200) == -1)
                {
                    Blocks[SurfaceHeight - 1][x_pos] = "Enemy";
                    Blocks[SurfaceHeight - 2][x_pos] = "Enemy";
                    Blocks[SurfaceHeight - 2][x_pos + 1] = "Enemy";
                    Blocks[SurfaceHeight - 1][x_pos + 1] = "Enemy";
                }
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Keybinds

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            //Standard Movement
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                goLeft = true;
            }
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                goRight = true;
            }
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            {
                goUp = true;
            }
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
            {
                goDown = true;
            }

            if (e.KeyCode == Keys.ShiftKey)
            {
                Player_ShiftMove = true;
            }

            // Jump
            if (e.KeyCode == Keys.Space)
            {
                Player_Jump = true;
            }

            // Regen World
            if (e.KeyCode == Keys.Tab)
            {
                Blocks.Clear();
                BiomeRegions.Clear();
                PhysicsMaterial_Water.Clear();
                PhysicsMaterial_Sand.Clear();
                Block_Generation_Border();
                Terrain_Generation_GenBiomes();
                Terrain_Generation();
            }

            // Random Fun
            if (e.KeyCode == Keys.E && Random_TracerActive == false)
            {
                Random_TracerActive = true;
            }
            else if (e.KeyCode == Keys.E && Random_TracerActive == true)
            {
                Random_TracerActive = false;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            // Standard Movmement
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                goLeft = false;
            }
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                goRight = false;
            }
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            {
                goUp = false;
            }
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
            {
                goDown = false;
            }

            if (e.KeyCode == Keys.ShiftKey)
            {
                Player_ShiftMove = false;
            }

            // Jump
            if (e.KeyCode == Keys.Space)
            {
                Player_Jump = false;
            }
        }

        private void Mouse_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0) //Scrolling Up
            {
                Highlighter_ChangeSize("Up");
            }

            else //Scrolling Down
            {
                Highlighter_ChangeSize("Down");
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Collision Detection

        private string CollisionType_Vertical(int Momentum, int x_pos, int y_pos)
        {
            string Type_BlockLeft = null;
            string Type_BlockRight = null;

            bool BetweenBlocks = false;


            if (x_pos % blockWidth != 0)
            {
                BetweenBlocks = true;
            }

            // Left block (Block player is standing on if not between blocks)
            if (Blocks[(y_pos + Momentum) / blockHeight][x_pos / blockWidth] == "Water")
            {
                Type_BlockLeft = "Fluid";
            }
            else if (Blocks[(y_pos + Momentum) / blockHeight][x_pos / blockWidth] != null)
            {
                Type_BlockLeft = "Solid";
            }

            // Right Block
            if (BetweenBlocks == true)
            {
                if (Blocks[(y_pos + Momentum) / blockHeight][x_pos / blockWidth + 1] == "Water")
                {
                    Type_BlockRight = "Fluid";
                }
                else if (Blocks[(y_pos + Momentum) / blockHeight][x_pos / blockWidth + 1] != null)
                {
                    Type_BlockRight = "Solid";
                }
            }

            // Return conclusion
            if (Type_BlockLeft == "Solid" || Type_BlockRight == "Solid")
            {
                return "Solid";
            }
            else if (Type_BlockLeft == "Fluid" || Type_BlockRight == "Fluid")
            {
                return "Fluid";
            }
            else
            {
                return null;
            }
        }

        private string CollisionType_Horizontal(int Momentum, int x_pos, int y_pos)
        {
            string Type_BlockUpper = null;
            string Type_BlockLower = null;

            bool BetweenBlocks = false;

            int offset = 0; // Needed so that detection to right works


            if (Momentum > 0)
            {
                offset = blockWidth - 1;
            }

            if (y_pos % blockHeight != 0)
            {
                BetweenBlocks = true;
            }


            // Upper Pixel
            if (Blocks[y_pos / blockHeight][(x_pos + Momentum + offset) / blockWidth] == "Water")
            {
                Type_BlockUpper = "Water";
            }
            else if (Blocks[y_pos / blockHeight][(x_pos + Momentum + offset) / blockWidth] != null)
            {
                Type_BlockUpper = "Solid";
            }

            // Lower Pixel
            if (BetweenBlocks == true)
            {
                if (Blocks[y_pos / blockHeight + 1][(x_pos + Momentum + offset) / blockWidth] == "Water")
                {
                    Type_BlockLower = "Water";
                }
                else if (Blocks[y_pos / blockHeight + 1][(x_pos + Momentum + offset) / blockWidth] != null)
                {
                    Type_BlockLower = "Solid";
                }
            }


            // Return conclusion
            if (Type_BlockUpper == "Solid" || Type_BlockLower == "Solid")
            {
                return "Solid";
            }
            else if (Type_BlockUpper == "Water" || Type_BlockLower == "Water")
            {
                return "Water";
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Player Movement

        private void Random_PlayerTracer()
        {
            if (Random_TracerActive == true)
            {
                Blocks[Player.y / blockHeight][Player.x / blockWidth - 1] = "Default";
            }
        }


        //Vertical
        private void Execute_PlayerMomentum_Vertical()
        {
            // Downward Movement
            if (Player.Momentum_Vertical > 0)
            {
                string Collision_Type = CollisionType_Vertical(Player.Momentum_Vertical, Player.x, Player.y);
                string Collision_Type_Bellow = CollisionType_Vertical(blockHeight, Player.x, Player.y);

                // Solid Bellow
                if (Collision_Type == "Solid" || Collision_Type_Bellow == "Solid")
                {
                    Player.Momentum_Vertical = 0;
                }

                // Water Bellow
                else if (Collision_Type == "Fluid")
                {
                    if (Player.Momentum_Vertical > blockHeight / 3)
                    {
                        Player.Momentum_Vertical = blockHeight / 3;
                    }

                    Player.y += Player.Momentum_Vertical;
                    cameraOffset_y += Player.Momentum_Vertical;

                    if (Player.Momentum_Vertical < blockHeight / 4 && GameTick % 12 == 0)
                    {
                        Player.Momentum_Vertical += 1;
                    }
                }

                // Air Bellow
                else if (Collision_Type == null)
                {
                    Player.y += Player.Momentum_Vertical;
                    cameraOffset_y += Player.Momentum_Vertical;

                    if (Player.Momentum_Vertical < blockHeight * 2 && GameTick % 2 == 0)
                    {
                        Player.Momentum_Vertical += 1;
                    }
                }
            }

            // Upward Movement
            if (Player.Momentum_Vertical < 0)
            {
                string Collision_Type = CollisionType_Vertical(Player.Momentum_Vertical, Player.x, Player.y);

                if (Collision_Type == "Solid")
                {
                    while (CollisionType_Vertical(-1, Player.x, Player.y) != "Solid")
                    {
                        Player.y--;
                        cameraOffset_y--;
                    }
                    Player.Momentum_Vertical = 0;
                }
                else if (Collision_Type == null || Collision_Type == "Fluid")
                {
                    Player.y += Player.Momentum_Vertical;
                    cameraOffset_y += Player.Momentum_Vertical;
                    if (GameTick % 2 == 0)
                    {
                        Player.Momentum_Vertical += 1;  // Change to -= 1 for fun
                    }
                }
            }

            //Give gravitational momentum
            if (Player.Momentum_Vertical == 0)
            {
                string Collision_Type = CollisionType_Vertical(blockHeight, Player.x, Player.y);
                if ((Collision_Type == null || Collision_Type == "Fluid") && GameTick % 2 == 0)
                {
                    Player.Momentum_Vertical += 1;
                }
            }
        }

        private void Execute_PlayerMomentum_Vertical_Handler()
        {
            if (Player_Jump && CollisionType_Vertical(blockHeight, Player.x, Player.y) == "Solid")
            {
                if (Player_AllowBunnyHop == true || Player.Momentum_Vertical == 0)
                {
                    if (Blocks[Player.y / blockHeight][Player.x / blockWidth] != "Water")
                    {
                        if (Player_ShiftMove)
                        {
                            Player.Momentum_Vertical = -Player.JumpHeight;
                        }
                        else
                        {
                            Player.Momentum_Vertical = -(Player.JumpHeight / 4 * 3);
                        }
                    }
                    else
                    {
                        Player.Momentum_Vertical = -Player.JumpHeight / 2;
                    }
                }
            }
            else if (Player_Jump && CollisionType_Vertical(blockHeight, Player.x, Player.y) == "Fluid")
            {
                if (Player.Momentum_Vertical > -Player.JumpHeight / 5)
                {
                    Player.Momentum_Vertical -= 2;
                }
            }
        }


        // Horizontal
        private void Execute_PlayerMomentum_Horizontal()
        {
            if (Player.Momentum_Horizontal != 0)
            {
                string CollsionType = CollisionType_Horizontal(Player.Momentum_Horizontal, Player.x, Player.y);


                // Solid Sideward
                if (CollsionType == "Solid")
                {
                    // AutoJump
                    int moveDirection = 1;
                    if (goLeft)
                    {
                        moveDirection *= -1;
                    }
                    if (((Blocks[Player.y / blockHeight - 1][Player.x / blockWidth] == null && Blocks[Player.y / blockHeight - 1][Player.x / blockWidth + moveDirection] == null)
                        || (Blocks[Player.y / blockHeight - 1][Player.x / blockWidth] == "Water" && Blocks[Player.y / blockHeight - 1][Player.x / blockWidth + moveDirection] == "Water"))
                        && GameTick % 1 == 0 && CollisionType_Vertical(blockHeight, Player.x, Player.y) == "Solid" &&
                        (Player.Momentum_Horizontal > 3 || Player.Momentum_Horizontal < -3 || Player_ShiftMove == true))
                    {
                        cameraOffset_x += Player.Momentum_Horizontal;
                        cameraOffset_y -= blockHeight;
                        Player.x += Player.Momentum_Horizontal;
                        Player.y -= blockHeight;
                    }

                    // Collision
                    else
                    {
                        Player.Momentum_Horizontal = 0;
                    }
                }

                // Fluid Sideward
                if (CollsionType == "Water") // Collision detecting water as air
                {
                    if (Player_ShiftMove == true)
                    {
                        if (Player.Momentum_Horizontal > blockWidth / 4)
                        {
                            Player.Momentum_Horizontal = blockWidth / 4;
                        }
                        else if (Player.Momentum_Horizontal < -blockWidth / 3)
                        {
                            Player.Momentum_Horizontal = -blockWidth / 4;
                        }
                    }
                    if (Player_ShiftMove == false)
                    {
                        if (Player.Momentum_Horizontal > blockWidth / 6)
                        {
                            Player.Momentum_Horizontal = blockWidth / 6;
                        }
                        else if (Player.Momentum_Horizontal < -blockWidth / 6)
                        {
                            Player.Momentum_Horizontal = -blockWidth / 6;
                        }
                    }


                    Player.x += Player.Momentum_Horizontal;
                    cameraOffset_x += Player.Momentum_Horizontal;
                }

                // Air Sideward
                if (CollsionType == null)
                {
                    Player.x += Player.Momentum_Horizontal;
                    cameraOffset_x += Player.Momentum_Horizontal;
                }
            }

            BackGround_Update();
        }

        private void Execute_PlayerMomentum_Horizontal_Handler()
        {
            int MomentumChangeMultiplier = 1;
            int MomentumSlowdown = 1;

            float MaxSpeed = Player.Speed_Base;

            // Assigns max speed
            if (Player_ShiftMove == true)
            {
                MaxSpeed = Player.Speed_Max;
            }


            // Left
            if (goLeft && Player.Momentum_Horizontal > -Convert.ToInt32(blockWidth * MaxSpeed))
            {
                if (Player.Momentum_Horizontal > 0)
                {
                    MomentumChangeMultiplier *= 3;
                }

                if (Player.Momentum_Horizontal == 0)
                {
                    Player.Momentum_Horizontal -= MomentumChangeMultiplier;
                }
                else if (GameTick % 4 == 0)
                {
                    Player.Momentum_Horizontal -= MomentumChangeMultiplier;
                }

            }

            // Right
            if (goRight && Player.Momentum_Horizontal < Convert.ToInt32(blockWidth * MaxSpeed))
            {
                if (Player.Momentum_Horizontal < 0)
                {
                    MomentumChangeMultiplier *= 3;
                }

                if (Player.Momentum_Horizontal == 0)
                {
                    Player.Momentum_Horizontal += MomentumChangeMultiplier;
                }
                else if (GameTick % 4 == 0)
                {
                    Player.Momentum_Horizontal += MomentumChangeMultiplier;
                }
            }


            // Momentum Reduction
            else if (((!goLeft && !goRight && Player.Momentum_Horizontal != 0) || (Player_ShiftMove == false &&
                (Player.Momentum_Horizontal > Player.Speed_Base * blockWidth || Player.Momentum_Horizontal < -Player.Speed_Base * blockWidth)))
                && GameTick % 3 == 0)
            {
                if (Player.Momentum_Horizontal < 0)
                {
                    Player.Momentum_Horizontal += MomentumSlowdown;
                }
                else if (Player.Momentum_Horizontal > 0)
                {
                    Player.Momentum_Horizontal -= MomentumSlowdown;
                }
            }
        }


        //Position Corrections
        private void Execute_PlayerMovement_Correction_Horizontal()
        {
            // Nulifies moving through block

            //Right
            if (Player.x % blockWidth != 0 && CollisionType_Horizontal(1, Player.x, Player.y) == "Solid")
            {
                while (Player.x % blockWidth != 0)
                {
                    Player.x -= 1;
                    cameraOffset_x -= 1;
                }
            }
            //Left
            else if (Player.x % blockWidth != 0 && CollisionType_Horizontal(0, Player.x, Player.y) == "Solid")
            {
                while (Player.x % blockWidth != 0)
                {
                    Player.x += 1;
                    cameraOffset_x += 1;
                }
            }
        }

        private void Execute_PlayerMovement_Correction_Vertical()
        {
            // Nulifies moving through block

            //Up
            if (Player.y % blockHeight != 0 && CollisionType_Vertical(0, Player.x, Player.y) == "Solid")
            {
                while (Player.y % blockHeight != 0)
                {
                    Player.y += 1;
                    cameraOffset_y += 1;
                }
            }
            //Down
            else if (Player.y % blockHeight != 0 && CollisionType_Vertical(blockHeight, Player.x, Player.y) == "Solid")
            {
                while (Player.y % blockHeight != 0)
                {
                    Player.y -= 1;
                    cameraOffset_y -= 1;
                }
            }
        }



        private void Execute_PlayerMovement_Handler()
        {
            // Horizontal
            Execute_PlayerMomentum_Horizontal();
            Execute_PlayerMovement_Correction_Horizontal();
            Execute_PlayerMomentum_Horizontal_Handler();

            // Vertical
            Execute_PlayerMomentum_Vertical();
            Execute_PlayerMovement_Correction_Vertical();
            Execute_PlayerMomentum_Vertical_Handler();
        }

        #endregion

        #region Entity Movement

        private void SpawnEntities()
        {
            while (Entities.Count < 100)
            {
                int x_pos = random.Next(10, worldWidth - 10) * blockWidth;
                int y_pos = 15 * blockHeight;

                EntityBlock Entity = new EntityBlock
                {
                    x = x_pos,
                    y = y_pos
                };

                Entities.Add(Entity);
            }
        }


        //Vertical
        private void Execute_EntityMomentum_Vertical()
        {
            foreach (EntityBlock Entity in Entities) 
            { 
                // Downward Movement
                if (Entity.Momentum_Vertical > 0)
                {
                    string Collision_Type = CollisionType_Vertical(Entity.Momentum_Vertical, Entity.x, Entity.y);
                    string Collision_Type_Bellow = CollisionType_Vertical(blockHeight, Entity.x, Entity.y);

                    // Solid Bellow
                    if (Collision_Type == "Solid" || Collision_Type_Bellow == "Solid")
                    {
                        Entity.Momentum_Vertical = 0;
                    }

                    // Water Bellow
                    else if (Collision_Type == "Fluid")
                    {
                        if (Entity.Momentum_Vertical > blockHeight / 3)
                        {
                            Entity.Momentum_Vertical = blockHeight / 3;
                        }

                        Entity.y += Entity.Momentum_Vertical;

                        if (Entity.Momentum_Vertical < blockHeight / 4 && GameTick % 12 == 0)
                        {
                            Entity.Momentum_Vertical += 1;
                        }
                    }

                    // Air Bellow
                    else if (Collision_Type == null)
                    {
                        Entity.y += Entity.Momentum_Vertical;

                        if (Entity.Momentum_Vertical < blockHeight * 2 && GameTick % 2 == 0)
                        {
                            Entity.Momentum_Vertical += 1;
                        }
                    }
                }

                // Upward Movement
                if (Entity.Momentum_Vertical < 0)
                {
                    string Collision_Type = CollisionType_Vertical(Entity.Momentum_Vertical, Entity.x, Entity.y);

                    // Solid Above
                    if (Collision_Type == "Solid")
                    {
                        while (CollisionType_Vertical(-1, Entity.x, Entity.y) != "Solid")
                        {
                            Entity.y--;
                        }
                        Entity.Momentum_Vertical = 0;
                    }

                    // Other Above
                    else if (Collision_Type == null || Collision_Type == "Fluid")
                    {
                        Entity.y += Entity.Momentum_Vertical;
                        if (GameTick % 2 == 0)
                        {
                            Entity.Momentum_Vertical += 1;  // Change to -= 1 for fun
                        }
                    }
                }

                //Give gravitational momentum
                if (Entity.Momentum_Vertical == 0)
                {
                    string Collision_Type = CollisionType_Vertical(blockHeight, Entity.x, Entity.y);
                    if ((Collision_Type == null || Collision_Type == "Fluid") && GameTick % 2 == 0)
                    {
                        Entity.Momentum_Vertical += 1;
                    }
                }
            }
        }

        private void Execute_EntityMomentum_Vertical_Handler()
        {
            // Entities will jump randomly. The rate defined by instances of "random.Next(0, 200) == 100"

            foreach (EntityBlock Entity in Entities)
            {
                if (random.Next(0, 200) == 100 && CollisionType_Vertical(blockHeight, Entity.x, Entity.y) == "Solid")
                {
                    if (Entity.Momentum_Vertical == 0)
                    {
                        if (Blocks[Entity.y / blockHeight][Entity.x / blockWidth] != "Water")
                        {
                            if (random.Next(0, 50) == 25)
                            {
                                Entity.Momentum_Vertical = -Entity.JumpHeight;
                            }
                            else
                            {
                                Entity.Momentum_Vertical = -(Entity.JumpHeight / 4 * 3);
                            }
                        }
                        else
                        {
                            Entity.Momentum_Vertical = -Entity.JumpHeight / 2;
                        }
                    }
                }
                else if (random.Next(0, 200) == 100 && CollisionType_Vertical(blockHeight, Entity.x, Entity.y) == "Fluid")
                {
                    if (Entity.Momentum_Vertical > -Entity.JumpHeight / 5)
                    {
                        Entity.Momentum_Vertical -= 2;
                    }
                }
            }
        }


        // Horizontal
        private void Execute_EntityMomentum_Horizontal()
        {
            foreach (EntityBlock Entity in Entities)
            {
                if (Entity.Momentum_Horizontal != 0)
                {
                    string CollsionType = CollisionType_Horizontal(Entity.Momentum_Horizontal, Entity.x, Entity.y);


                    // Solid Sideward
                    if (CollsionType == "Solid")
                    {
                        // AutoJump
                        int moveDirection = 1;
                        if (Entity.Direction == "Left")
                        {
                            moveDirection *= -1;
                        }
                        if (((Blocks[Entity.y / blockHeight - 1][Entity.x / blockWidth] == null && Blocks[Entity.y / blockHeight - 1][Entity.x / blockWidth + moveDirection] == null)
                            || (Blocks[Entity.y / blockHeight - 1][Entity.x / blockWidth] == "Water" && Blocks[Entity.y / blockHeight - 1][Entity.x / blockWidth + moveDirection] == "Water"))
                            && GameTick % 1 == 0 && CollisionType_Vertical(blockHeight, Entity.x, Entity.y) == "Solid" &&
                            (Entity.Momentum_Horizontal > 1 || Entity.Momentum_Horizontal < -1))
                        {
                            Entity.x += Entity.Momentum_Horizontal;
                            Entity.y -= blockHeight;
                        }

                        // Collision
                        else
                        {
                            Entity.Momentum_Horizontal = 0;
                        }
                    }

                    // Fluid Sideward
                    if (CollsionType == "Water") // Collision detecting water as air
                    {
                        if (Entity.Momentum_Horizontal > blockWidth / 4)
                        {
                            Entity.Momentum_Horizontal = blockWidth / 4;
                        }
                        else if (Entity.Momentum_Horizontal < -blockWidth / 3)
                        {
                            Entity.Momentum_Horizontal = -blockWidth / 4;
                        }

                        Entity.x += Entity.Momentum_Horizontal;
                    }

                    // Air Sideward
                    if (CollsionType == null)
                    {
                        Entity.x += Entity.Momentum_Horizontal;
                    }
                }
            }
        }

        private void Execute_EntityMomentum_Horizontal_Handler()
        {
            foreach (EntityBlock Entity in Entities)
            {
                int MomentumChangeMultiplier = 1;
                int MomentumSlowdown = 1;

                float MaxSpeed = Entity.Speed_Base;

                // Assigns max speed
                if (Entity.Aggravation > 25)
                {
                    MaxSpeed = Entity.Speed_Max;
                }


                // Left
                if (Entity.Direction == "Left" && Entity.Momentum_Horizontal > -Convert.ToInt32(blockWidth * MaxSpeed))
                {
                    if (Entity.Momentum_Horizontal > 0)
                    {
                        MomentumChangeMultiplier *= 3;
                    }

                    if (Entity.Momentum_Horizontal == 0)
                    {
                        Entity.Momentum_Horizontal -= MomentumChangeMultiplier;
                    }
                    else if (GameTick % 4 == 0)
                    {
                        Entity.Momentum_Horizontal -= MomentumChangeMultiplier;
                    }

                }

                // Right
                if (Entity.Direction == "Right" && Entity.Momentum_Horizontal < Convert.ToInt32(blockWidth * MaxSpeed))
                {
                    if (Entity.Momentum_Horizontal < 0)
                    {
                        MomentumChangeMultiplier *= 3;
                    }

                    if (Entity.Momentum_Horizontal == 0)
                    {
                        Entity.Momentum_Horizontal += MomentumChangeMultiplier;
                    }
                    else if (GameTick % 4 == 0)
                    {
                        Entity.Momentum_Horizontal += MomentumChangeMultiplier;
                    }
                }


                // Momentum Reduction
                else if (((Entity.Direction == "Still" && Entity.Momentum_Horizontal != 0) || (Entity.Aggravation < 25 &&
                    (Entity.Momentum_Horizontal > Entity.Speed_Base * blockWidth || Entity.Momentum_Horizontal < -Entity.Speed_Base * blockWidth)))
                    && GameTick % 3 == 0)
                {
                    if (Entity.Momentum_Horizontal < 0)
                    {
                        Entity.Momentum_Horizontal += MomentumSlowdown;
                    }
                    else if (Entity.Momentum_Horizontal > 0)
                    {
                        Entity.Momentum_Horizontal -= MomentumSlowdown;
                    }
                }
            }
        }


        //Position Corrections
        private void Execute_EntityMovement_Correction_Horizontal()
        {
            // Nulifies moving through block

            foreach (EntityBlock Entity in Entities)
            {
                //Right
                if (Entity.x % blockWidth != 0 && CollisionType_Horizontal(1, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.x % blockWidth != 0)
                    {
                        Entity.x -= 1;
                    }
                }
                //Left
                else if (Entity.x % blockWidth != 0 && CollisionType_Horizontal(0, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.x % blockWidth != 0)
                    {
                        Entity.x += 1;
                    }
                }
            }
        }

        private void Execute_EntityMovement_Correction_Vertical()
        {
            // Nulifies moving through block

            foreach (EntityBlock Entity in Entities)
            {
                //Up
                if (Entity.y % blockHeight != 0 && CollisionType_Vertical(0, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.y % blockHeight != 0)
                    {
                        Entity.y += 1;
                    }
                }
                //Down
                else if (Entity.y % blockHeight != 0 && CollisionType_Vertical(blockHeight, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.y % blockHeight != 0)
                    {
                        Entity.y -= 1;
                    }
                }
            }
        }


        private void Execute_EntityMovement_Handler()
        {
            // Horizontal
            Execute_EntityMomentum_Horizontal();
            Execute_EntityMovement_Correction_Horizontal();
            Execute_EntityMomentum_Horizontal_Handler();

            // Vertical
            Execute_EntityMomentum_Vertical();
            Execute_EntityMovement_Correction_Vertical();
            Execute_EntityMomentum_Vertical_Handler();
        }


        // Reduncant
        private void Execute_EntityMomentum_Vertical1()
        {
            foreach (EntityBlock Entity in Entities)
            {
                if (Blocks[Entity.y / blockHeight + 1][Entity.x / blockWidth] == null)
                {
                    Entity.y += blockHeight;
                }
            }
        }

        private void Execute_EntityMovement_Handler1()
        {
            Execute_EntityMomentum_Vertical();
        }

        #endregion

        #region Projectile Movement

        private void Attack_Projectile_Create(int x_pos, int y_pos, int momentum_x, int momentum_y)
        {
            Projectile projectile = new Projectile
            {
                x = x_pos,
                y = y_pos,
                Momentum_Horizontal = momentum_x,
                Momentum_Vertical = momentum_y
            };
            Projectiles.Add(projectile);
        }


        //Vertical
        private void Execute_ProjectileMomentum_Vertical()
        {
            foreach (Projectile projectile in Projectiles)
            {
                // Downward Movement
                if (projectile.Momentum_Vertical > 0)
                {
                    string Collision_Type = CollisionType_Vertical(projectile.Momentum_Vertical, projectile.x, projectile.y);
                    string Collision_Type_Bellow = CollisionType_Vertical(blockHeight, projectile.x, projectile.y);

                    // Solid Bellow
                    if (Collision_Type == "Solid" || Collision_Type_Bellow == "Solid")
                    {
                        projectile.Momentum_Vertical = 0;
                    }

                    // Water Bellow
                    else if (Collision_Type == "Fluid")
                    {
                        if (projectile.Momentum_Vertical > blockHeight / 3)
                        {
                            projectile.Momentum_Vertical = blockHeight / 3;
                        }

                        projectile.y += projectile.Momentum_Vertical;

                        if (projectile.Momentum_Vertical < blockHeight / 4 && GameTick % 12 == 0)
                        {
                            projectile.Momentum_Vertical += 1;
                        }
                    }

                    // Air Bellow
                    else if (Collision_Type == null)
                    {
                        projectile.y += projectile.Momentum_Vertical;

                        if (projectile.Momentum_Vertical < blockHeight * 2 && GameTick % 2 == 0)
                        {
                            projectile.Momentum_Vertical += 1;
                        }
                    }
                }

                // Upward Movement
                if (projectile.Momentum_Vertical < 0)
                {
                    string Collision_Type = CollisionType_Vertical(projectile.Momentum_Vertical, projectile.x, projectile.y);

                    // Solid Above
                    if (Collision_Type == "Solid")
                    {
                        while (CollisionType_Vertical(-1, projectile.x, projectile.y) != "Solid")
                        {
                            projectile.y--;
                        }
                        projectile.Momentum_Vertical = 0;
                    }

                    // Other Above
                    else if (Collision_Type == null || Collision_Type == "Fluid")
                    {
                        projectile.y += projectile.Momentum_Vertical;
                        if (GameTick % 2 == 0)
                        {
                            projectile.Momentum_Vertical += 1;  // Change to -= 1 for fun
                        }
                    }
                }

                //Give gravitational momentum
                if (projectile.Momentum_Vertical == 0)
                {
                    string Collision_Type = CollisionType_Vertical(blockHeight, projectile.x, projectile.y);
                    if ((Collision_Type == null || Collision_Type == "Fluid") && GameTick % 2 == 0)
                    {
                        projectile.Momentum_Vertical += 1;
                    }
                }
            }
        }

        // Horizontal
        private void Execute_ProjectileMomentum_Horizontal()
        {
            foreach (Projectile projectile in Projectiles)
            {
                if (projectile.Momentum_Horizontal != 0)
                {
                    string CollsionType = CollisionType_Horizontal(projectile.Momentum_Horizontal, projectile.x, projectile.y);


                    // Solid Sideward
                    if (CollsionType == "Solid")
                    {
                        projectile.Momentum_Horizontal = 0;
                    }

                    // Fluid Sideward
                    if (CollsionType == "Water") // Collision detecting water as air
                    {
                        if (projectile.Momentum_Horizontal > blockWidth / 4)
                        {
                            projectile.Momentum_Horizontal = blockWidth / 4;
                        }
                        else if (projectile.Momentum_Horizontal < -blockWidth / 3)
                        {
                            projectile.Momentum_Horizontal = -blockWidth / 4;
                        }

                        projectile.x += projectile.Momentum_Horizontal;
                    }

                    // Air Sideward
                    if (CollsionType == null)
                    {
                        projectile.x += projectile.Momentum_Horizontal;
                    }
                }
            }
        }

        private void Execute_ProjectileMomentum_Horizontal_Handler()
        {
            foreach (Projectile projectile in Projectiles)
            {
                int MomentumSlowdown = 1;

                // Momentum Reduction
                if (projectile.Momentum_Horizontal < 0)
                {
                    projectile.Momentum_Horizontal += MomentumSlowdown;
                }
                else if (projectile.Momentum_Horizontal > 0)
                {
                    projectile.Momentum_Horizontal -= MomentumSlowdown;
                }
            }
        }


        //Position Corrections
        private void Execute_ProjectileMovement_Correction_Horizontal()
        {
            // Nulifies moving through block

            foreach (EntityBlock Entity in Entities)
            {
                //Right
                if (Entity.x % blockWidth != 0 && CollisionType_Horizontal(1, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.x % blockWidth != 0)
                    {
                        Entity.x -= 1;
                    }
                }
                //Left
                else if (Entity.x % blockWidth != 0 && CollisionType_Horizontal(0, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.x % blockWidth != 0)
                    {
                        Entity.x += 1;
                    }
                }
            }
        }

        private void Execute_ProjectileMovement_Correction_Vertical()
        {
            // Nulifies moving through block

            foreach (EntityBlock Entity in Entities)
            {
                //Up
                if (Entity.y % blockHeight != 0 && CollisionType_Vertical(0, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.y % blockHeight != 0)
                    {
                        Entity.y += 1;
                    }
                }
                //Down
                else if (Entity.y % blockHeight != 0 && CollisionType_Vertical(blockHeight, Entity.x, Entity.y) == "Solid")
                {
                    while (Entity.y % blockHeight != 0)
                    {
                        Entity.y -= 1;
                    }
                }
            }
        }


        private void Execute_ProjectileMovement_Handler()
        {
            // Horizontal
            Execute_ProjectileMomentum_Horizontal();
            //Execute_EntityMovement_Correction_Horizontal();
            Execute_ProjectileMomentum_Horizontal_Handler();

            // Vertical
            Execute_ProjectileMomentum_Vertical();
            //Execute_EntityMovement_Correction_Vertical();
        }


        #endregion

        /////////////////////////////////////////

        #region Player Survival

        private void Execute_Player_HealthDetection()
        {
            // Damage
            if (Player.x / blockWidth > 0 && Player.x / blockWidth < worldWidth - 1 && Player.y / blockHeight > 0 && Player.y / blockHeight < worldHeight - 1 &&
                GameTick % 100 >= 75 && GameTick % 100 <= 99 && Player.Health > 0)
            {
                if (Player.x % blockWidth == 0)
                {
                    if (Blocks[Player.y / blockHeight][Player.x / blockWidth - 1] == "Enemy" ||
                        Blocks[Player.y / blockHeight][Player.x / blockWidth + 1] == "Enemy")
                    {
                        Player.Health--;
                        return;
                    }
                    if (Player.y % blockHeight != 0)
                    {
                        if (Blocks[Player.y / blockHeight + 1][Player.x / blockWidth - 1] == "Enemy" ||
                        Blocks[Player.y / blockHeight + 1][Player.x / blockWidth + 1] == "Enemy")
                        {
                            Player.Health--;
                            return;
                        }
                    }
                }
                if (Player.y % blockHeight == 0)
                {
                    if (Blocks[Player.y / blockHeight - 1][Player.x / blockWidth] == "Enemy" ||
                        Blocks[Player.y / blockHeight + 1][Player.x / blockWidth] == "Enemy")
                    {
                        Player.Health--;
                        return;
                    }
                    if (Player.x % blockWidth != 0)
                    {
                        if (Blocks[Player.y / blockHeight - 1][Player.x / blockWidth + 1] == "Enemy" ||
                            Blocks[Player.y / blockHeight + 1][Player.x / blockWidth + 1] == "Enemy")
                        {
                            Player.Health--;
                            return;
                        }
                    }
                }
            }

            // Healing
            if (Player.Health < Player.Health_Max && GameTick % Player_Health_RegainInterval == 0)
            {
                Player.Health += Player_Health_RegainAmount;
            }

            // Corrections
            if (Player.Health > Player.Health_Max)
            {
                Player.Health = Player.Health_Max;
            }
            else if (Player.Health < 0)
            {
                Player.Health = 0;
            }
        }

        private void Execute_Player_BreathDetection()
        {
            bool isUnderwater = true;
            

            if (Blocks[Player.y / blockHeight][Player.x / blockWidth] == null)
            {
                isUnderwater = false;
            }

            if (Player.x % blockWidth != 0)
            {
                if (Blocks[Player.y / blockHeight][Player.x / blockWidth + 1] == null)
                {
                    isUnderwater = false;
                }
            }
            if (Player.y % blockHeight != 0)
            {
                if (Blocks[Player.y / blockHeight + 1][Player.x / blockWidth] == null)
                {
                    isUnderwater = false;
                }
                if (Player.x % blockWidth != 0)
                {
                    if (Blocks[Player.y / blockHeight + 1][Player.x / blockWidth + 1] == null)
                    {
                        isUnderwater = false;
                    }
                }
            }

            //Breath Loss
            if (isUnderwater == true && Player.Breath > 0)
            {
                Player.Breath--;
            }
            else if (Player.Breath == 0 && Player.Health > 0 && GameTick % 25 == 0)
            {
                Player.Health -= Player.Health_Max / 20;
            }

            // Breath Regain
            else if (Player.Breath < Player.Breath_Max && GameTick % Player_Breath_RegainInterval == 0)
            {
                Player.Breath += Player_Breath_RegainAmount;
            }

            // UI
            if (Player.Breath == Player.Breath_Max)
            {
                UIItems[2].Visible = false;
            }
            else if (Player.Breath < Player.Breath_Max)
            {
                UIItems[2].Visible = true;
            }

            // Corrections
            if (Player.Breath > Player.Breath_Max)
            {
                Player.Breath = Player.Breath_Max;
            }
            else if (Player.Breath < 0)
            {
                Player.Breath = 0;
            }
        }

        private void Execute_PlayerSurvival()
        {
            Execute_Player_HealthDetection();
            Execute_Player_BreathDetection();
        }

        #endregion


        /////////////////////////////////////////

        #region Highlighter

        private void Mouse_GenOutline()
        {
            Highligher.Clear();

            for (int i = 0; i < Highlighter_Size; i++)
            {
                Highligher.Add(new List<bool>());

                for (int p = 0; p < Highlighter_Size; p++)
                {
                    Highligher[i].Add(true);
                }
            }
        }

        private List<int> Execute_BlockPlaceBoundary(int Mouse_X, int Mouse_Y)
        {
            int placeBound_X_Left;
            int placeBound_Y_Left;
            int placeBound_X_Right;
            int placeBound_Y_Right;

            //Left
            if ((Mouse_X + cameraOffset_x) / blockWidth - Highligher[0].Count() / 2 < 0)
            {
                placeBound_X_Left = 0;
            }
            else
            {
                placeBound_X_Left = (Mouse_X + cameraOffset_x) / blockWidth - Highligher[0].Count() / 2;
            }
            if ((Mouse_Y + cameraOffset_y) / blockHeight - Highligher.Count() / 2 < 0)
            {
                placeBound_Y_Left = 0;
            }
            else
            {
                placeBound_Y_Left = (Mouse_Y + cameraOffset_y) / blockHeight - Highligher[0].Count() / 2;
            }

            //Right
            if ((Mouse_X + cameraOffset_x) / blockWidth + Highligher[0].Count() / 2 > Blocks[0].Count() - 1)
            {
                placeBound_X_Right = Blocks[0].Count() - 1;
            }
            else
            {
                placeBound_X_Right = (Mouse_X + cameraOffset_x) / blockWidth + Highligher[0].Count() / 2 + 1;
            }
            if ((Mouse_Y + cameraOffset_y) / blockHeight + Highligher.Count() / 2 > Blocks.Count() - 1)
            {
                placeBound_Y_Right = Blocks.Count() - 1;
            }
            else
            {
                placeBound_Y_Right = (Mouse_Y + cameraOffset_y) / blockHeight + Highligher.Count() / 2 + 1;
            }

            return new List<int>() { placeBound_X_Left, placeBound_Y_Left, placeBound_X_Right, placeBound_Y_Right };
        }

        private void Highlighter_PlacePixels(int Mouse_X, int Mouse_Y)
        {
            List<int> Boundaries = Execute_BlockPlaceBoundary(Mouse_X, Mouse_Y);
            int placeBound_X_Left = Boundaries[0];
            int placeBound_Y_Left = Boundaries[1];
            int placeBound_X_Right = Boundaries[2];
            int placeBound_Y_Right = Boundaries[3];


            //Eraser
            if (MaterialSelector_Selected == null)
            {
                Highlighter_Execute_Eraser(Mouse_X, Mouse_Y);
            }

            if (MaterialSelector_Selected == "Attack")
            {
                int Momentum_Horizontal = (Mouse_X - Screen.Width / 2 - playerCameraOffset_X - 1) / blockWidth;
                int Momentum_Vertical = (Mouse_Y - Screen.Height / 2 - blockHeight + 2) / blockHeight;
                Attack_Projectile_Create(Player.x, Player.y, Momentum_Horizontal, Momentum_Vertical);
            }

            //Other
            else
            {
                for (int y_pos = placeBound_Y_Left; y_pos < placeBound_Y_Right; y_pos++)
                {
                    for (int x_pos = placeBound_X_Left; x_pos < placeBound_X_Right; x_pos++)
                    {
                        if (random.Next(0, Highlighter_placeChance) == 0)
                        {
                            Material_CreatePixel(MaterialSelector_Selected, x_pos, y_pos);
                        }
                    }
                }
            }
        }

        private void Highlighter_Execute_Eraser(int Mouse_X, int Mouse_Y)
        {
            List<int> Boundaries = Execute_BlockPlaceBoundary(Mouse_X, Mouse_Y);
            int placeBound_X_Left = Boundaries[0];
            int placeBound_Y_Left = Boundaries[1];
            int placeBound_X_Right = Boundaries[2];
            int placeBound_Y_Right = Boundaries[3];

            /* IDK
            restart:
            foreach (as wads in awdsad)
            {
                if (wads == asdljasdk)
                {
                    goto restart:
                }
            }
            */

            for (int y_pos = placeBound_Y_Left; y_pos < placeBound_Y_Right; y_pos++)
            {
                for (int x_pos = placeBound_X_Left; x_pos < placeBound_X_Right; x_pos++)
                {
                    Material_ErasePixel(Blocks[y_pos][x_pos], x_pos, y_pos);
                }
            }

        }

        private void Highlighter_ChangeSize(string ChangeDirection)
        {
            if (ChangeDirection == "Up")
            {
                Highlighter_Size += 1;
            }

            else if (ChangeDirection == "Down" && Highlighter_Size > 1)
            {
                Highlighter_Size -= 1;
            }

            Mouse_GenOutline();
        }

        #endregion

        #region UI

        private void UI_Build()
        {
            UIItem UI_Checkbox1 = new UIItem
            {
                Type = "MaterialSelector",
                Active = true,
                Active_Amount = 0,
                Size_X = 50,
                Size_Y = 50,
                Location_X = Screen.Width - 20 - 50,
                Location_Y = Screen.Height - 20 - 50
            };
            UIItems.Add(UI_Checkbox1);

            UIItem UI_HealthBar = new UIItem
            {
                Type = "HealthBar",
                Active = Player_isMortal,
                Size_X = 500,
                Size_Y = 40,
                Location_X = 70,
                Location_Y = Screen.Height - 90
            };
            UIItems.Add(UI_HealthBar);

            UIItem UI_BreathBar = new UIItem
            {
                Type = "BreathBar",
                Active = Player_isMortal,
                Size_X = 500,
                Size_Y = 40,
                Location_X = 70,
                Location_Y = Screen.Height - 150
            };
            UIItems.Add(UI_BreathBar);

            UIItem UI_AttackMode = new UIItem {
                Type = "AttackMode",
                Active = false,
                Size_X = 50,
                Size_Y = 50,
                Location_X = Screen.Width - 90 - 50,
                Location_Y = Screen.Height - 20 - 50
                };
            UIItems.Add(UI_AttackMode);
        }

        private void UI_PositionUpdate()
        {
            //Material Selector
            UIItems[0].Location_X = Screen.Width - 20 - 50;
            UIItems[0].Location_Y = Screen.Height - 40 - 50;

            //HealthBar
            UIItems[1].Location_X = 70;
            UIItems[1].Location_Y = Screen.Height - 90;

            //BreathBar
            UIItems[2].Location_X = 70;
            UIItems[2].Location_Y = Screen.Height - 150;

            //Attack Mode Button
            UIItems[3].Location_X = Screen.Width - 90 - 50;
            UIItems[3].Location_Y = Screen.Height - 40 - 50;
        }

        #endregion

        /////////////////////////////////////////

        #region Mouse Handler

        private void Mouse_Down(object sender, MouseEventArgs e)
        {
            Mouse_Clicking = true;
            if (Highlighter_Visible != false)
            {
                Highlighter_PlacePixels(e.Location.X, e.Location.Y);
                return;
            }
        }

        private void Mouse_Release(object sender, MouseEventArgs e)
        {
            Mouse_Clicking = false;

            foreach (UIItem Item in UIItems)
            {
                if (e.Location.X >= Item.Location_X && e.Location.X <= Item.Location_X + Item.Size_X &&
                    e.Location.Y >= Item.Location_Y && e.Location.Y <= Item.Location_Y + Item.Size_Y)
                {
                    if (Item.Type == "MaterialSelector")
                    {
                        if (Item.Active == false)
                        {
                            UIItems[3].Active = false;
                            Item.Active = true;
                        }

                        else if (Item.Active_Amount == MaterialSelector_Materials.Count() - 1)
                        {
                            Item.Active_Amount = 0;
                        }
                        else
                        {
                            Item.Active_Amount++;
                        }
                        MaterialSelector_Selected = MaterialSelector_Materials[Item.Active_Amount];

                        
                    }
                    if (Item.Type == "AttackMode")
                    {
                        if (Item.Active == true)
                        {
                            MaterialSelector_Selected = MaterialSelector_Materials[UIItems[0].Active_Amount];
                            UIItems[0].Active = true;

                            Item.Active = false;
                        }
                        else
                        {
                            UIItems[0].Active = false;

                            MaterialSelector_Selected = "Attack";
                            Item.Active = true;
                        }
                    }
                    return;
                }
            }
        }

        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            //Highlighter
            foreach (UIItem Item in UIItems)
            {
                if (Item.Visible == true)
                {
                    int Box_Location_X = Item.Location_X + (Item.Active_Amount * Item.Size_X / 100) - Item.Size_Sub_X / 2;
                    int Box_Location_Y = Item.Location_Y - Item.Size_Sub_Y / 2 + Item.Size_Y / 2;

                    if (e.Location.X >= Item.Location_X && e.Location.X <= Item.Location_X + Item.Size_X &&
                        e.Location.Y >= Item.Location_Y && e.Location.Y <= Item.Location_Y + Item.Size_Y)
                    {
                        Highlighter_Visible = false;
                        return;
                    }
                    if (e.Location.X >= Box_Location_X && e.Location.X <= Box_Location_X + Item.Size_Sub_X &&
                        e.Location.Y >= Box_Location_Y && e.Location.Y <= Box_Location_Y + Item.Size_Sub_Y)
                    {
                        Highlighter_Visible = false;
                        return;
                    }
                }
            }
            Highlighter_Visible = true;

            //Mouse
            Mouse_X = e.Location.X;
            Mouse_Y = e.Location.Y;

            if (Mouse_Clicking && Mouse_BlockChange(e.Location.X, e.Location.Y))
            {
                Execute_BlockPlaceBoundary(e.Location.X, e.Location.Y);
                Highlighter_PlacePixels(e.Location.X, e.Location.Y);
            }
        }

        private bool Mouse_BlockChange(int x_pos, int y_pos)
        {
            if (x_pos / blockWidth != Mouse_PrevPosition[0] || y_pos / blockHeight != Mouse_PrevPosition[1])
            {
                Mouse_PrevPosition[0] = x_pos / blockWidth;
                Mouse_PrevPosition[1] = y_pos / blockHeight;
                return true;
            }
            return false;
        }

        #endregion

        /////////////////////////////////////////

        #region Block Physics

        private void Execute_Physics_Sand()
        {
            foreach (List<int> Particle in PhysicsMaterial_Sand)
            {
                if (Particle[0] >= blockBound_X_Left / 2 && Particle[0] <= blockBound_X_Right * 2 &&
                    Particle[1] >= blockBound_Y_Left / 2 && Particle[1] <= blockBound_Y_Right * 2)
                {
                    string Sand_Type = "Sand";
                    if (Particle[2] == 1)
                    {
                        Sand_Type = "Red Sand";
                    }

                    //Vertical Gravity
                    if (Blocks[Particle[1] + 1][Particle[0]] == null)
                    {
                        Blocks[Particle[1]][Particle[0]] = null;
                        Blocks[Particle[1] + 1][Particle[0]] = Sand_Type;
                        Particle[1] += 1;
                    }

                    //Diagonal Gravity
                    int Direction = random.Next(0, 1);
                    if (Direction == 0)
                    {
                        Direction = -1;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (Blocks[Particle[1] + 1][Particle[0] + Direction] == null && Blocks[Particle[1] + 1][Particle[0]] != null)
                        {
                            if (random.Next(0, 6) != 3) // slight randomnes to movement, breaks up moving pillars
                            {
                                Blocks[Particle[1]][Particle[0]] = null;
                                Blocks[Particle[1] + 1][Particle[0] + Direction] = Sand_Type;
                                Particle[1] += 1;
                                Particle[0] += Direction;
                            }
                            break;
                        }
                        Direction *= -1;
                    }
                }
            }
        }

        private void Execute_Physics_Fluid()
        {
            foreach (List<int> Particle in PhysicsMaterial_Water)
            {
                if (Blocks[Particle[1] + 1][Particle[0]] == null)
                {
                    // Verical Gravity
                    Blocks[Particle[1] + 1][Particle[0]] = "Water";
                    Blocks[Particle[1]][Particle[0]] = null;
                    Particle[1] += 1;

                    // Sideways Flow
                    if (random.Next(0, 15) == 1 && Blocks[Particle[1]][Particle[0] + Particle[2]] == null)
                    {
                        Blocks[Particle[1]][Particle[0] + Particle[2]] = "Water";
                        Blocks[Particle[1]][Particle[0]] = null;
                        Particle[0] += Particle[2];
                    }
                    else if (random.Next(0, 25) == 1 && Blocks[Particle[1]][Particle[0] + (Particle[2] * -1)] == null)
                    {
                        Blocks[Particle[1]][Particle[0] + (Particle[2] * -1)] = "Water";
                        Blocks[Particle[1]][Particle[0]] = null;
                        Particle[0] += (Particle[2] * -1);
                    }
                }
                else
                {
                    // Sand Sink
                    if (Blocks[Particle[1] - 1][Particle[0]] == "Sand")
                    {
                        foreach (List<int> SandParticle in PhysicsMaterial_Sand)
                        {
                            if (SandParticle[1] == Particle[1] - 1 && SandParticle[0] == Particle[0])
                            {
                                SandParticle[1] += 1;
                            }
                        }
                        Blocks[Particle[1] - 1][Particle[0]] = "Water";
                        Blocks[Particle[1]][Particle[0]] = "Sand";
                        Particle[1] -= 1;
                    }
                    if (Blocks[Particle[1] - 1][Particle[0]] == "Red Sand")
                    {
                        foreach (List<int> SandParticle in PhysicsMaterial_Sand)
                        {
                            if (SandParticle[1] == Particle[1] - 1 && SandParticle[0] == Particle[0])
                            {
                                SandParticle[1] += 1;
                            }
                        }
                        Blocks[Particle[1] - 1][Particle[0]] = "Water";
                        Blocks[Particle[1]][Particle[0]] = "Red Sand";
                        Particle[1] -= 1;
                    }

                    // Sideways Flow
                    if (Blocks[Particle[1]][Particle[0] + Particle[2]] == null)
                    {
                        Blocks[Particle[1]][Particle[0] + Particle[2]] = "Water";
                        Blocks[Particle[1]][Particle[0]] = null;
                        Particle[0] += Particle[2];
                    }
                    else if (Blocks[Particle[1]][Particle[0] + (Particle[2] * -1)] == null)
                    {
                        Blocks[Particle[1]][Particle[0] + (Particle[2] * -1)] = "Water";
                        Blocks[Particle[1]][Particle[0]] = null;
                        Particle[0] -= Particle[2];
                        Particle[2] *= -1;
                    }
                }
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Blocks Create/Erase

        private void Material_ErasePixel(string BlockType, int x_pos, int y_pos)
        {
            if (BlockType == "Water")
            {
                foreach (List<int> Particle in PhysicsMaterial_Water)
                {
                    if (Particle[0] == x_pos && Particle[1] == y_pos)
                    {
                        PhysicsMaterial_Water.Remove(Particle);
                        break;
                    }
                }
            }
            if (BlockType == "Sand" || BlockType == "Red Sand")
            {
                foreach (List<int> Particle in PhysicsMaterial_Sand)
                {
                    if (Particle[0] == x_pos && Particle[1] == y_pos)
                    {
                        PhysicsMaterial_Sand.Remove(Particle);
                        break;
                    }
                }
            }

            Blocks[y_pos][x_pos] = null;
        }

        private void Material_CreatePixel(string BlockType, int x_pos, int y_pos)
        {
            // Erase Previouse Pixel
            if (Blocks[y_pos][x_pos] != null)
            {
                if (BlockType == "Water" && Highlighter_FluidReplace == false)
                {
                    return;
                }
                else
                {
                    Material_ErasePixel(Blocks[y_pos][x_pos], x_pos, y_pos);
                }
            }

            // Create New Pixel
            if (BlockType == "Water")
            {
                PhysicsMaterial_Water.Add(new List<int>() { x_pos, y_pos, 1 });
                return;
            }
            if (BlockType == "Sand")
            {
                PhysicsMaterial_Sand.Add(new List<int>() { x_pos, y_pos, 0 });
                return;
            }
            if (BlockType == "Red Sand")
            {
                PhysicsMaterial_Sand.Add(new List<int>() { x_pos, y_pos, 1 });
                return;
            }

            Blocks[y_pos][x_pos] = BlockType;
        }

        #endregion

        /////////////////////////////////////////

        #region Fundamentals - Sub

        private void Colors_Generate()
        {
            Color_Red = new SolidBrush(Color.Red);
            Color_Blue = new SolidBrush(Color.Blue);
            Color_Grass = new SolidBrush(Color.FromArgb(51, 204, 51));
            Color_Foliage = new SolidBrush(Color.Green);
            Color_Dirt = new SolidBrush(Color.FromArgb(134, 89, 45));
            Color_Rock = new SolidBrush(Color.FromArgb(128, 128, 128));
            Color_BedRock = new SolidBrush(Color.FromArgb(30, 30, 30));
            Color_Sand = new SolidBrush(Color.SandyBrown);
            Color_RedSand = new SolidBrush(Color.OrangeRed);
            Color_Water = new SolidBrush(Color.Blue);
            Color_Void = new SolidBrush(Color.Black);
            Color_Enemy = new SolidBrush(Color.Black);
            Color_Standard = new SolidBrush(Color.Purple);
            Color_Default = new SolidBrush(Color.White);

            Color_ButtonActive = new SolidBrush(Color.Gold);
            Color_ButtonAttack = new SolidBrush(Color.DarkRed);
        }

        private Brush Block_FetchColor(string type)
        {
            switch (type)
            {
                case "Red":
                    return Color_Red;
                case "Blue":
                    return Color_Blue;

                case "Grass":
                    return Color_Grass;
                case "Shrub":
                    return Color_Foliage;
                case "Dirt":
                    return Color_Dirt;
                case "Rock":
                    return Color_Rock;
                case "Bed Rock":
                    return Color_BedRock;
                case "Sand":
                    return Color_Sand;
                case "Red Sand":
                    return Color_RedSand;
                case "Water":
                    return Color_Water;

                case "Enemy":
                    return Color_Enemy;

                case "AttackButton":
                    return Color_ButtonAttack;
                case "ButtonActive":
                    return Color_ButtonActive;

                case null:
                    return Color_Void;
                case "Default":
                    return Color_Standard;
                default:
                    return Color_Default;
            }
        }

        private void Execute_BlockLoadBoundary()
        {
            //Left
            //X
            if (cameraOffset_x < 0)
            {
                blockBound_X_Left = 0;
            }
            else
            {
                blockBound_X_Left = cameraOffset_x / blockWidth;
            }
            //Y
            if (cameraOffset_y < 0)
            {
                blockBound_Y_Left = 0;
            }
            else
            {
                blockBound_Y_Left = cameraOffset_y / blockHeight;
            }


            //Right
            //X
            if (cameraOffset_x / blockWidth + screenWidth - 1 > Blocks[0].Count() - 3)
            {
                blockBound_X_Right = Blocks[0].Count() - 1;
            }
            else
            {
                blockBound_X_Right = cameraOffset_x / blockWidth + screenWidth + 2;

            }
            //Y
            if (cameraOffset_y / blockWidth + screenHeight - 1 > Blocks.Count() - 3)
            {
                blockBound_Y_Right = Blocks.Count() - 1;
            }
            else
            {
                blockBound_Y_Right = cameraOffset_y / blockHeight + screenHeight + 2;
            }
        }

        private void Execute_Movement()
        {
            if (goUp)
            {
                if (Player_ShiftMove == true)
                {
                    cameraOffset_y -= blockHeight;
                    Player.y -= blockHeight;
                }
                else
                {
                    Player.Momentum_Vertical = -15;
                }
            }
            if (goDown)
            {
                if (Player_ShiftMove == true)
                {
                    cameraOffset_y += blockHeight;
                    Player.y += blockHeight;
                }
                else
                {
                    Player.Momentum_Vertical = 0;
                }
            }
        }

        private void BackGround_Update()
        {
            if (BiomeRegions[Player.x / blockWidth] == "Sand")
            {
                Screen.BackColor = Color.FromArgb(255, 230, 179);
            }
            else
            {
                Screen.BackColor = Color.FromArgb(128, 255, 255);
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Fundamentals

        private void Screen_SizeChange(object sender, EventArgs e)
        {
            Screen.Width = Size.Width - 27;
            Screen.Height = Size.Height - 44;

            //UI
            UI_PositionUpdate();


            //X
            if (Screen.Width > prevScreenPixelWidth)
            {
                for (int i = 0; i < Screen.Width - prevScreenPixelWidth; i++)
                {
                    if ((prevScreenPixelWidth + i) % 2 == 0)
                    {
                        cameraOffset_x -= 1;
                    }
                }
            }
            else if (Screen.Width < prevScreenPixelWidth)
            {
                for (int i = 0; i < prevScreenPixelWidth - Screen.Width; i++)
                {
                    if ((prevScreenPixelWidth - i) % 2 == 0)
                    {
                        cameraOffset_x += 1;
                    }
                }
            }

            //Y
            if (Screen.Height > prevScreenPixelHeight)
            {
                for (int i = 0; i < Screen.Height - prevScreenPixelHeight; i++)
                {
                    if ((prevScreenPixelHeight + i) % 2 == 1)
                    {
                        cameraOffset_y -= 1;
                    }
                }
            }
            else if (Screen.Height < prevScreenPixelHeight)
            {
                for (int i = 0; i < prevScreenPixelHeight - Screen.Height; i++)
                {
                    if ((prevScreenPixelHeight - i) % 2 == 1)
                    {
                        cameraOffset_y += 1;
                    }
                }
            }

            prevScreenPixelWidth = Screen.Width;
            prevScreenPixelHeight = Screen.Height;

            screenHeight = Screen.Height / blockHeight;
            screenWidth = Screen.Width / blockWidth;
        }

        private void GameTick_Handler()
        {
            GameTick++;

            if (GameTick == 1000000)
            {
                GameTick = 0;
            }
        }

        private void UpdateScreen(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;

            //Blocks
            for (int y_pos = blockBound_Y_Left; y_pos < blockBound_Y_Right; y_pos++)
            {
                for (int x_pos = blockBound_X_Left; x_pos < blockBound_X_Right; x_pos++)
                {
                    if (false)
                    {
                        if (Blocks[y_pos][x_pos] == "Dirt") //SEIZURE DIRT
                        {
                            canvas.FillRectangle(new SolidBrush(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255))), new Rectangle(
                                (x_pos * blockWidth) - cameraOffset_x,
                                (y_pos * blockHeight) - cameraOffset_y,
                                blockWidth, blockHeight
                                ));
                            continue;
                        }
                    }

                    if (Blocks[y_pos][x_pos] != null)
                    {
                        canvas.FillRectangle(Block_FetchColor(Blocks[y_pos][x_pos]), new Rectangle(
                            (x_pos * blockWidth) - cameraOffset_x,
                            (y_pos * blockHeight) - cameraOffset_y,
                            blockWidth, blockHeight
                            ));
                    }
                }
            }

            //Entities
            foreach (EntityBlock Entity in Entities)
            {
                if (Entity.x > blockBound_X_Left * blockWidth && Entity.x < blockBound_X_Right * blockWidth &&
                    Entity.y > blockBound_Y_Left * blockHeight && Entity.y < blockBound_Y_Right * blockHeight)
                {
                    canvas.FillRectangle(Block_FetchColor("Enemy"), new Rectangle(
                        (Entity.x) - cameraOffset_x,
                        (Entity.y) - cameraOffset_y,
                        blockWidth, blockHeight
                        ));
                }
            }

            // Player
            canvas.FillRectangle(Brushes.Red, new Rectangle(
                Screen.Width / 2 - playerCameraOffset_X - 1,
                Screen.Height / 2 - blockHeight + 2,
                blockWidth, blockHeight
                ));


            //Highlighter
            if (Highlighter_Visible)
            {
                for (int y_pos = 0; y_pos < Highligher.Count(); y_pos++)
                {
                    for (int x_pos = 0; x_pos < Highligher[y_pos].Count(); x_pos++)
                    {
                        if (x_pos == 0 || x_pos == Highligher[y_pos].Count() - 1 || y_pos == 0 || y_pos == Highligher.Count() - 1)
                        {
                            canvas.FillRectangle(Block_FetchColor("Highlighter"), new Rectangle(
                                    (x_pos + (Mouse_X / blockWidth - Highlighter_Size / 2)) * blockWidth,
                                    (y_pos + (Mouse_Y / blockHeight - Highlighter_Size / 2)) * blockHeight,
                                    blockWidth, blockHeight
                                    ));
                        }
                    }
                }
            }

            //UI
            foreach (UIItem Item in UIItems)
            {
                if (Item.Visible == true)
                {
                    // Material Selector
                    if (Item.Type == "MaterialSelector")
                    {
                        Brush ActiveColor = Block_FetchColor(Item.Type);
                        if (Item.Active == true)
                        {
                            ActiveColor = Block_FetchColor("ButtonActive");
                        }

                        canvas.FillRectangle(ActiveColor, new Rectangle(
                                Item.Location_X,
                                Item.Location_Y,
                                Item.Size_X, Item.Size_Y
                                ));

                        canvas.FillRectangle(Block_FetchColor(MaterialSelector_Materials[Item.Active_Amount]), new Rectangle(
                                Item.Location_X + 5,
                                Item.Location_Y + 5,
                                Item.Size_X - 10, Item.Size_Y - 10
                                ));
                    }

                    // Attack Mode Button
                    if (Item.Type == "AttackMode")
                    {
                        Brush ActiveColor = Block_FetchColor(Item.Type);
                        if (Item.Active == true)
                        {
                            ActiveColor = Block_FetchColor("ButtonActive");
                        }

                        canvas.FillRectangle(ActiveColor, new Rectangle(
                                Item.Location_X,
                                Item.Location_Y,
                                Item.Size_X, Item.Size_Y
                                ));

                        canvas.FillRectangle(Block_FetchColor("AttackButton"), new Rectangle(
                                Item.Location_X + 5,
                                Item.Location_Y + 5,
                                Item.Size_X - 10, Item.Size_Y - 10
                                ));
                    }

                    // Health Bar
                    if (Item.Type == "HealthBar")
                    {
                        canvas.FillRectangle(Block_FetchColor(Item.Type), new Rectangle(
                                Item.Location_X,
                                Item.Location_Y,
                                Item.Size_X, Item.Size_Y
                                ));

                        canvas.FillRectangle(Brushes.Red, new Rectangle(
                                Item.Location_X + 5,
                                Item.Location_Y + 5,
                                Convert.ToInt32(Player.Health * Decimal.Divide(Item.Size_X - 10, 100)), Item.Size_Y - 10
                                ));
                    }

                    // Breath Bar
                    if (Item.Type == "BreathBar")
                    {
                        canvas.FillRectangle(Block_FetchColor(Item.Type), new Rectangle(
                                Item.Location_X,
                                Item.Location_Y,
                                Item.Size_X, Item.Size_Y
                                ));

                        canvas.FillRectangle(Brushes.Blue, new Rectangle(
                                Item.Location_X + 5,
                                Item.Location_Y + 5,
                                Convert.ToInt32(Player.Breath * Decimal.Divide(Item.Size_X - 10, 1000)), Item.Size_Y - 10
                                ));
                    }

                    // ScrollBar Example
                    if (Item.Type == "ScrollBar")
                    {
                        canvas.FillRectangle(Block_FetchColor(Item.Type), new Rectangle(
                                Item.Location_X,
                                Item.Location_Y,
                                Item.Size_X, Item.Size_Y
                                ));
                        int Box_Location_X = Item.Location_X + (Item.Active_Amount * Item.Size_X / 100) - Item.Size_Sub_X / 2;
                        int Box_Location_Y = Item.Location_Y - Item.Size_Sub_Y / 2 + Item.Size_Y / 2;

                        // Amount Selector Outer
                        canvas.FillRectangle(Brushes.Red, new Rectangle(
                            Box_Location_X,
                            Box_Location_Y,
                            Item.Size_Sub_X, Item.Size_Sub_Y
                            ));

                        //Amount Selector Inner
                        canvas.FillRectangle(Brushes.White, new Rectangle(
                            Box_Location_X + 5,
                            Box_Location_Y + 5,
                            Item.Size_Sub_X - 10, Item.Size_Sub_Y - 10
                            ));


                        int FontOffset = 0;
                        if (Item.Active_Amount < 10)
                        {
                            FontOffset = 5;
                        }

                        //Amount Selector Text
                        canvas.DrawString(Item.Active_Amount.ToString(),
                            new Font("ArcadeClassic Regular", 16),
                            Brushes.Black,
                            //Item.Location_X + Item.Active_Amount - Item.Size_Sub_X / 4,
                            //Item.Location_Y - Item.Size_Sub_Y / 4 + 2,
                            Box_Location_X + 5 + FontOffset,
                            Box_Location_Y + Item.Size_Sub_Y / 5
                            );
                    }
                }
            }
        }

        private void gameTimerEvent(object sender, EventArgs e)
        {
            GameTick_Handler();

            Execute_Movement();

            // Player
            Execute_PlayerMovement_Handler();
            Execute_PlayerSurvival();

            // Entities
            Execute_EntityMovement_Handler();
            Execute_ProjectileMovement_Handler();

            // Block Physics
            Execute_Physics_Fluid();
            Execute_Physics_Sand();

            Execute_BlockLoadBoundary();
            Random_PlayerTracer();

            Screen.Invalidate();
        }

        #endregion

        /////////////////////////////////////////
    }
}
