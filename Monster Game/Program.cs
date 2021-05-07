using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Monster_Game
{
    class Program
    {
        public const int NS = 4;
        public const int WE = 6;

        public struct CellReference
        {
            public int NoOfCellsEast;
            public int NoOfCellsSouth;
        }

        class Game
        {
            private Character Player = new Character();
            private Grid Cavern = new Grid();
            private SleepyEnemy Monster = new SleepyEnemy();
            private Item Flask = new Item();
            private Trap Trap1 = new Trap();
            private Trap Trap2 = new Trap();
            private Boolean TrainingGame;

            public Game(Boolean IsATrainingGame)
            {
                TrainingGame = IsATrainingGame;
                SetUpGame(); //sets up the game
                Play(); 
            }

            public void Play()
            {
                int Count;
                Boolean Eaten;
                Boolean FlaskFound;
                char MoveDirection;
                Boolean ValidMove;
                CellReference Position;
                Eaten = false;
                FlaskFound = false;
                Cavern.Display(Monster.GetAwake()); //checks if the monster is awake
                do
                {
                    do
                    {
                        DisplayMoveOptions(); //displays move options
                        MoveDirection = GetMove(); //sets the move direction to the character returned from GetMove()
                        ValidMove = CheckValidMove(MoveDirection); //Checks if the move is valid
                        if (!ValidMove)
                            Console.WriteLine("That is not a valid move, please try again");
                    } while (!ValidMove);   //repeats until the move is valid
                    if (MoveDirection != 'M' && MoveDirection != 'A')   //as long as the move is not return to menu
                    {
                        Cavern.PlaceItem(Player.GetPosition(), ' '); //sets the current position of the player to whitespace
                        Player.MakeMove(MoveDirection);
                        Cavern.PlaceItem(Player.GetPosition(), '*'); //sets the position of where the player will be to '*'
                        Cavern.Display(Monster.GetAwake()); //checks if the monster is awake
                        FlaskFound = Player.CheckIfSameCell(Flask.GetPosition()); //checks if the player and the flask are in the same cell. If they are, FlaskFound = true
                        if (FlaskFound)
                        {
                            DisplayWonGameMessage(); //displays the won game message
                        }
                        Eaten = Monster.CheckIfSameCell(Player.GetPosition()); //checks if the player and the monster are in the same cell. If they are, Eaten = true
                        // This selection structure checks to see if the player has 
                        // triggered one of the traps in the cavern
                        if (!Monster.GetAwake() && !FlaskFound && !Eaten && ((Player.CheckIfSameCell(Trap1.GetPosition()) && !Trap1.GetTriggered()) || (Player.CheckIfSameCell(Trap2.GetPosition()) && !Trap2.GetTriggered())))
                        {
                            Monster.ChangeSleepStatus();
                            DisplayTrapMessage();
                            Cavern.Display(Monster.GetAwake());
                        }
                        if (Monster.GetAwake() && !Eaten && !FlaskFound)
                        {
                            Count = 0;
                            do
                            {
                                Cavern.PlaceItem(Monster.GetPosition(), ' ');
                                Position = Monster.GetPosition();
                                Monster.MakeMove(Player.GetPosition());
                                Cavern.PlaceItem(Monster.GetPosition(), 'M');
                                if (Monster.CheckIfSameCell(Flask.GetPosition()))
                                {
                                    Flask.SetPosition(Position);
                                    Cavern.PlaceItem(Position, 'F');
                                }
                                Eaten = Monster.CheckIfSameCell(Player.GetPosition());
                                Console.WriteLine();
                                Console.WriteLine("Press Enter key to continue");
                                Console.ReadLine();
                                Cavern.Display(Monster.GetAwake());
                                Count = Count + 1;
                            } while (Count != 2 && !Eaten);
                        }
                        if (Eaten)
                        {
                            DisplayLostGameMessage();
                        }
                    }
                    else if (MoveDirection == 'A')
                    {
                        var direction = Player.GetArrowDirection();
                        if ((Player.GetPosition().NoOfCellsEast == 4 && Player.GetPosition().NoOfCellsSouth == 2) && 
                            (Monster.GetPosition().NoOfCellsEast == 4 && (Monster.GetPosition().NoOfCellsSouth == 1 || 
                                                                          Monster.GetPosition().NoOfCellsSouth == 0)) && direction == 'N' )
                        {
                            Console.WriteLine("You have shot the monster and it cannot stop you finding the flask");
                            Console.WriteLine();
                            FlaskFound = true;
                        }
                    }
                    
                } while (!Eaten && !FlaskFound && MoveDirection != 'M'); //repeats as long as the player is still alive, has not found the flask, and the direction of movement is not 'M'
            }

            public void DisplayMoveOptions() //displays the move options
            {
                Console.WriteLine();
                Console.WriteLine("Enter N to move NORTH");
                Console.WriteLine("Enter S to move SOUTH");
                Console.WriteLine("Enter E to move EAST");
                Console.WriteLine("Enter W to move WEST");
                Console.WriteLine("Enter A to shoot an ARROW");
                Console.WriteLine("Enter M to return to the Main Menu");
                Console.WriteLine();
            }
            

            public char GetMove() //gets the move
            {
                char Move;
                Move = char.Parse(Console.ReadLine()); //coverts the input of type string to a character
                Console.WriteLine();
                return Move;
            }

            public void DisplayWonGameMessage() //displays the winning game message
            {
                Console.WriteLine("Well done! you have found the flask containing the Styxian potion.");
                Console.WriteLine("You have won the game of MONSTER!");
                Console.WriteLine();
            }

            public void DisplayTrapMessage()
            {
                Console.WriteLine("Oh no! You have set off a trap. Watch out, the monster is now awake!");
                Console.WriteLine();
            }

            public void DisplayLostGameMessage()
            {
                Console.WriteLine("ARGHHHHHH! The monster has eaten you. GAME OVER.");
                Console.WriteLine("Maybe you will have better luck next time you play MONSTER!");
                Console.WriteLine();
            }

            public Boolean CheckValidMove(char Direction)   //checks whether a move is one of the choices available, but it does not check whether the player is on the edge of the map or not.
            {
                Boolean ValidMove;
                ValidMove = true;
                
                if (!(Direction == 'N' || Direction == 'S' || Direction == 'W' || Direction == 'E' || Direction == 'A' || Direction == 'M'))
                {
                    ValidMove = false;
                }
                else if (Direction == 'W' && Player.GetPosition().NoOfCellsEast == 0)
                {
                    ValidMove = false;
                }
                
                else if (Direction == 'A' && !Player.GetHasArrow())
                {
                    Console.WriteLine("Out of Arrows");
                    ValidMove = false;
                }
                
                return ValidMove;
            }

            public CellReference SetPositionOfItem(char Item)   //gets the representation character of an item
            {
                CellReference Position;     //Instantiates cellreference position
                do
                {
                    Position = GetNewRandomPosition(); //sets the coordinates of the position to a randomly discovered value
                } while (!Cavern.IsCellEmpty(Position)); //checks if the cell at that position is already occupied
                Cavern.PlaceItem(Position, Item);   //if not it places the item
                return Position;       //returns the value of position
            }

            public void SetUpGame() //sets up the game
            {
                CellReference Position; //coordinate
                Cavern.Reset();     //clears the board
                if (!TrainingGame)
                {
                    Position.NoOfCellsEast = 0; //sets the x coordinate to 0
                    Position.NoOfCellsSouth = 0; //sets the y coordinate to 0
                    Player.SetPosition(Position); //sets the player position to (0,0)
                    Cavern.PlaceItem(Position, '*'); //sets the player marker at the same position as the player
                    Trap1.SetPosition(SetPositionOfItem('T')); //runs set position then takes the return value of set position and enters it as a parameter into setposition
                    Trap2.SetPosition(SetPositionOfItem('T'));
                    Monster.SetPosition(SetPositionOfItem('M'));
                    Flask.SetPosition(SetPositionOfItem('F'));
                }
                else
                {
                    Position.NoOfCellsEast = 4;
                    Position.NoOfCellsSouth = 2;
                    Player.SetPosition(Position);
                    Cavern.PlaceItem(Position, '*');
                    Position.NoOfCellsEast = 6;
                    Position.NoOfCellsSouth = 2;
                    Trap1.SetPosition(Position);
                    Cavern.PlaceItem(Position, 'T');
                    Position.NoOfCellsEast = 4;
                    Position.NoOfCellsSouth = 3;
                    Trap2.SetPosition(Position);
                    Cavern.PlaceItem(Position, 'T');
                    Position.NoOfCellsEast = 4;
                    Position.NoOfCellsSouth = 0;
                    Monster.SetPosition(Position);
                    Cavern.PlaceItem(Position, 'M');
                    Position.NoOfCellsEast = 3;
                    Position.NoOfCellsSouth = 1;
                    Flask.SetPosition(Position);
                    Cavern.PlaceItem(Position, 'F');
                }
            }

            public CellReference GetNewRandomPosition()
            {
                CellReference Position;
                Random rnd = new Random();  //gets a random number
                Position.NoOfCellsSouth = rnd.Next(0, NS + 1); //sets the coordinate to whatever the random number is
                Position.NoOfCellsEast = rnd.Next(0, WE + 1);
                return Position;    //returns the value in position
            }
        }

        class Grid
        {
            private char[,] CavernState = new char[NS + 1, WE + 1]; //two dimensional character array of width 7 and height 5

            public void Reset() //resets the array to whitespace
            {
                int Count1;
                int Count2;
                for (Count1 = 0; Count1 <= NS; Count1++)    //completes all 7 columns before switching to the next row
                {
                    for (Count2 = 0; Count2 <= WE; Count2++)
                    {
                        CavernState[Count1, Count2] = ' ';  //sets the value of the two dimensional array at the given coordinate to whitespace
                    }
                }
            }

            public void Display(Boolean MonsterAwake)
            {
                int Count1;
                int Count2;
                for (Count1 = 0; Count1 <= NS; Count1++)
                {
                    Console.WriteLine(" ------------- ");
                    for (Count2 = 0; Count2 <= WE; Count2++)
                    {
                        if (CavernState[Count1, Count2] == ' ' || CavernState[Count1, Count2] == '*' || (CavernState[Count1, Count2] == 'M' && MonsterAwake))
                        {
                            Console.Write("|" + CavernState[Count1, Count2]);
                        }
                        else
                        {
                            Console.Write("| ");
                        }
                    }
                    Console.WriteLine("|");
                }
                Console.WriteLine(" ------------- ");
                Console.WriteLine();
            }

            public void PlaceItem(CellReference Position, char Item)
            {
                CavernState[Position.NoOfCellsSouth, Position.NoOfCellsEast] = Item;        //sets the value of the cell at the given coordinates to the character stored in Item
            }

            public Boolean IsCellEmpty(CellReference Position)
            {
                if (CavernState[Position.NoOfCellsSouth, Position.NoOfCellsEast] == ' ')    //checks if the value of the cell at the given coordinates is whitespace
                    return true;
                else
                    return false;
            }
        }

        class Enemy : Item
        {
            private Boolean Awake;

            public virtual void MakeMove(CellReference PlayerPosition)
            {
                if (NoOfCellsSouth < PlayerPosition.NoOfCellsSouth)
                {
                    NoOfCellsSouth = NoOfCellsSouth + 1;
                }
                else
                  if (NoOfCellsSouth > PlayerPosition.NoOfCellsSouth)
                {
                    NoOfCellsSouth = NoOfCellsSouth - 1;
                }
                else
                    if (NoOfCellsEast < PlayerPosition.NoOfCellsEast)
                {
                    NoOfCellsEast = NoOfCellsEast + 1;
                }
                else
                {
                    NoOfCellsEast = NoOfCellsEast - 1;
                }
            }

            public Boolean GetAwake()
            {
                return Awake;
            }

            public virtual void ChangeSleepStatus()
            {
                if (!Awake)
                    Awake = true;
                else
                    Awake = false;
            }

            public Enemy()
            {
                Awake = false;
            }
        }

        class SleepyEnemy : Enemy
        {
            private int movesTillSleep = 4;
            private int counter = 0;

            public override void ChangeSleepStatus()
            {
                if (counter == 0)
                {
                    base.ChangeSleepStatus();
                    counter++;
                }
                if (movesTillSleep == 0)
                {
                    base.ChangeSleepStatus();
                    Console.WriteLine("The monster is asleep.");
                }
            }

            public override void MakeMove(CellReference PlayerPosition)
            {
                if (movesTillSleep > 0)
                {
                    base.MakeMove(PlayerPosition);
                }
                movesTillSleep -= 1;
                ChangeSleepStatus();
            }
        }

        class Character : Item
        {
            private bool hasArrow = true;

            public void MakeMove(char Direction) //updates the position of the player
            {
                switch (Direction)
                {
                    case 'N':
                        NoOfCellsSouth = NoOfCellsSouth - 1;
                        break;
                    case 'S':
                        NoOfCellsSouth = NoOfCellsSouth + 1;
                        break;
                    case 'W':
                        NoOfCellsEast = NoOfCellsEast - 1;
                        break;
                    case 'E':
                        NoOfCellsEast = NoOfCellsEast + 1;
                        break;
                }
            }

            public bool GetHasArrow()
            {
                return hasArrow;
            }

            public char GetArrowDirection()
            {
                bool ValidMove;
                var input = ' ';

                do
                {
                    Console.WriteLine("Which direction do you want to shoot the arrow?");
                    Console.WriteLine();
                    Console.WriteLine("Enter N to shoot NORTH");
                    Console.WriteLine("Enter S to shoot SOUTH");
                    Console.WriteLine("Enter E to shoot EAST");
                    Console.WriteLine("Enter W to shoot WEST");
                    Console.WriteLine();
                    input = char.Parse(Console.ReadLine());

                    ValidMove = true;

                    if (!(input == 'N' || input == 'S' || input == 'W' || input == 'E'))
                    {
                        ValidMove = false;
                    }
                    
                } while (!ValidMove);

                hasArrow = false;
                return input;
            }
        }

        class Trap : Item
        {
            private Boolean Triggered;

            public Boolean GetTriggered()
            {
                return Triggered;
            }

            public Trap()
            {
                Triggered = false;
            }

            public void ToggleTrap()
            {
                Triggered = !Triggered;
            }
        }

        class Item
        {
            protected int NoOfCellsEast;
            protected int NoOfCellsSouth;

            public CellReference GetPosition()
            {
                CellReference Position;
                Position.NoOfCellsEast = NoOfCellsEast;
                Position.NoOfCellsSouth = NoOfCellsSouth;
                return Position;
            }

            public void SetPosition(CellReference Position)
            {
                NoOfCellsEast = Position.NoOfCellsEast;
                NoOfCellsSouth = Position.NoOfCellsSouth;
            }

            public Boolean CheckIfSameCell(CellReference Position)
            {
                if (NoOfCellsEast == Position.NoOfCellsEast && NoOfCellsSouth == Position.NoOfCellsSouth)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        static void Main(string[] args)
        {
            int Choice = 0;
            while (Choice != 9) //if the choice is 9 then the program exits
            {
                DisplayMenu();
                Choice = GetMainMenuChoice(); //records the main menu choice
                switch (Choice)
                {
                    case 1:
                        Game NewGame = new Game(false); //makes a normal game
                        break;
                    case 2:
                        Game TrainingGame = new Game(true); //makes a training game
                        break;
                }
            }
        }

        public static void DisplayMenu() //displays the main menu
        {
            Console.WriteLine("MAIN MENU");
            Console.WriteLine();
            Console.WriteLine("1. Start new game");
            Console.WriteLine("2. Play training game");
            Console.WriteLine("9. Quit");
            Console.WriteLine();
            Console.Write("Please enter your choice: ");
        }

        public static int GetMainMenuChoice()
        {
            int Choice;
            Choice = int.Parse(Console.ReadLine());
            Console.WriteLine();
            return Choice;
        }
    }
}