using System;
using System.ComponentModel.Design;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Metrics;


namespace AstrophobiaFirst
{
    internal class Program
    {
        public delegate void RoomMethod(); //Allows player to return the the room they were in before opening the menu.

        public struct Item //Qualities of the item
        {
            public string Name;
            public string Description;
            public int Quant;
        }
     
        public static List <Item> Inventory = new List <Item> ();//Inventory list that is added to with each new item.

        public static bool
                Comms = false,
                Thrusters = false,
                Reactor = false,
                ShipAi = false;
        public static bool power = false;
        public static int oxygenLevel = 999;
        public static int reactorCore = 150;
        public static string currentRoom = "\0", enemy = "bob";
        public static int dormRoomCount = 0;
        public static int taskCount = 0;
        public static int playerHP = 100, enemyHP = 100;
        public static bool bridgeEvent = false;

        static void Main(string[] args)
        {
            Combat();
            Mainmenu();
        }
        static void Mainmenu()
        {
            Console.Clear();
            String logo = "\n   __    ___  ____  ____  _____  ____  _   _  _____  ____  ____    __   \r\n  /__\\  / __)(_  _)(  _ \\(  _  )(  _ \\( )_( )(  _  )(  _ \\(_  _)  /__\\  \r\n /(__)\\ \\__ \\  )(   )   / )(_)(  )___/ ) _ (  )(_)(  ) _ < _)(_  /(__)\\ \r\n(__)(__)(___/ (__) (_)\\_)(_____)(__)  (_) (_)(_____)(____/(____)(__)(__)\n"
             + "        ~+                                    \r\n                                              \r\n                 *       +               .'.  \r\n           '                  |          |o|  \r\n       ()    .-.,=\"``\"=.    - o -       .'o'. \r\n             '=/_       \\     |         |.-.| \r\n          *   |  '=._    |              '   ' \r\n               \\     `=./`,        '     ( )  \r\n            .   '=.__.=' `='      *       )   \r\n   +                         +           ( )  \r\n        O      *        '       .             \n";
            ScrollText(logo);
            Thread.Sleep(700);
            Console.WriteLine("\n1    Play" +
                              "\n2    Help" +
                              "\n3    Options" +
                              "\n4    Exit\n");

            int userInput = ValidateUserInput(4);            

            switch (userInput)
            {
                case 1:
                    Intro();
                    break;
                case 2:
                    Help();
                    break;
                case 3:
                    //Options();
                    break;
                case 4:
                    GameEnd();
                    break;
            }
        }

        static void Help()
        {
            Console.Clear();
            Console.WriteLine("This is the help section, where everything you may need as you play through this game. Below you will find the Help options, which goes into specifics about the specified topic.");
            Console.WriteLine("\n1    Commands" +
                              "\n2    Purpose\n");
            Console.WriteLine("\nHit Enter to Go back to the Main Menu");

            int userInput;
            userInput = ValidateUserInput(2);

            //Options are Commands, Purpose
            switch (userInput)
            {
                case 1:
                    Console.Clear();
                    Console.WriteLine("This page will specify globally used commands within the game:");
                    Console.WriteLine("\nlook: This command is used to look around the room you are currently in, to help you with your surroundings, \nit may also show any items found in said room.");
                    Console.WriteLine("\nleave: Used to leave the current room you are in, assuming said room is linked to the hallway.");
                    Console.WriteLine("\nTo pick up any items that can be found in the room you are currently in, you will likely answer in yes or no. \nYou will also have to write which slot the item fills.");
                    Console.WriteLine("\nmenu: this command will bring up the ingame menu, and with it, a few more options for the player, \nsuch as restarting exiting the game, going to the main menu etc...");
                    Console.WriteLine("\nUse a rooms name while in the hallway to go to the room you have typed (e.g. typing dorm goes to the Dorm room).");
                    Console.WriteLine("\ninventory: This is used to access your inventory and see what slots are free and full.");
                    Console.WriteLine("\nskip: This is used to skip any story if you don't want to read or you have already read.");
                    Console.WriteLine("\nship stats: This allows you to access oxygen levels and see what parts of the ship or damaged, enabled or disabled.");
                    Console.WriteLine("\nHit Enter to go back to the Help Options page.");
                    Console.ReadLine();
                    Help();
                    break;
                case 2:
                    Console.Clear();
                    Console.WriteLine("This page will outline the story/purpose of this game.");
                    Console.WriteLine("\nThe purpose of this game is to escape from this ship that you have woken up on. You have no memory or why you \nare here or how you got here.");
                    Console.WriteLine("You start to explore the ship to get use to your surroundings and find out that it is damaged.");
                    Console.WriteLine("As you are the only one on the ship, it is up to you to fix the ship to save your own life, else you may not survive.");
                    Console.WriteLine("While fixing the ship you have to come over multiple challenges that get harder as the gme goes on.");
                    Console.WriteLine("There are multiple endings to find along the way, some good, some bad.\nWill you be able to fix the ship or escape before it is to late?");
                    Console.WriteLine("\nHit Enter to go back to the Help Options page.");
                    Console.ReadLine();
                    Help();
                    break;
                default:
                    {
                        Console.Clear();
                        Mainmenu();
                        break;
                    }
            }
        }

        static void DisplayMap()
        {
            string[] mapLines = new string[]
            {
                "          MMM         ",
                "         |___|        ",
                "     __MMMMMMMMM__    ",
                "    [   |______|  ]   ",
                "   [    |BRIDGE|   ]  ",
                "  [     |      |    ] ",
                "  |-----------------| ",
                "  |       | | DORM  | ",
                "  |       | |       | ", 
                "  |_______| |_______| ",
                "  |       | | MED   | ",
                "  |STORAGE| |       | ",
                "  |       |_|_______| ", 
                "  |-------     |    | ",
                "  |    |AIRLOCK|    | ",
                "  |    |       |    | ",
                "  |    |-------|    | ",
                "  A    |       |    A ",
                " A|||||||||||||||||||A ",
                "|||A|A|A|||A|||A|A|A||| ",
                "A||A|A|A|||A|||A|A|A||A "
            };

            for (int i = 0; i < mapLines.Length; i++)
            {
                if (mapLines[i].Contains(currentRoom.ToUpper()))
                {
                    char[] currentLine=mapLines[i].ToArray();
                    char[] lineBelow = mapLines[i+1].ToArray();
                    int pad = Array.IndexOf(currentLine, currentRoom.ToUpper()[2]);
                    lineBelow[pad] = 'X';
                    mapLines[i + 1] = new string(lineBelow);
                }
            }

            if (currentRoom == "Hall")
            {
                char[] HallLine=mapLines[8].ToArray(); 
                HallLine[11] = 'X';
                mapLines[8] = new string(HallLine);
            }

            Console.WriteLine("X shows your current position");
            Console.WriteLine();

            foreach(string mapLine in mapLines)
            {
                Console.WriteLine(mapLine);
            }

            Console.ReadLine();
            switch(currentRoom)
            {
                case "Dorm": 
                    Dorm();
                    break;
                case "Hall": 
                    Hall(); 
                    break;
                case "Bridge":
                    Bridge();
                    break;
                case "Med": 
                    Med();
                    break;
                case "Storage": 
                    Storage();
                    break;
                case "Airlock": 
                    AirLock();
                    break;
            }
        }

        public static void InventoryMethod(RoomMethod previousRoom)
        {
            Console.Clear();
            Console.WriteLine("You open your bag;\n");
            foreach (var Item in Inventory)//Displays each inventory item in a readable format
            {
                Console.WriteLine($"{Item.Name} - x{Item.Quant}\n   {Item.Description}\n");

            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadLine();
            Console.Clear();
            previousRoom();
        }

        public static bool CheckInventory(string item)
        {
            Console.Clear();
            bool itemFound = Inventory.Any(Item => Item.Name == "Torch");
            return itemFound;
        }
        static void restartGame()
        {
            Console.Clear();
            Comms = false;
            Thrusters = false;
            Reactor = false;
            ShipAi = false;
            power = false;
            oxygenLevel = 999;
            reactorCore = 150;
            currentRoom = "\0";
            dormRoomCount = 0;
            Inventory.Clear();
            Intro();
        }

        static void IGmenu(ref string currentRoom)
        {
            string Border = new string('*', 42);
            Console.WriteLine();
            Console.WriteLine(Border);
            Console.WriteLine("*\t\t  <Press>\t\t *");
            Console.WriteLine("* (1) To Resume   \t\t\t *\n* (2) If you wish to Restart\t\t *\n* (3) To go to the Main Menu\t\t *\n* (4) If you would like to Exit the game *");
            Console.WriteLine(Border);

            int userInput;
            userInput = ValidateUserInput(4);
            switch (userInput)
            {
                case 1: //Resume
                    if (currentRoom == "Dorm")
                    {
                        Dorm();
                    }
                    if (currentRoom == "Hall")
                    {
                        Hall();
                    }
                    if (currentRoom == "Bridge")
                    {
                        Bridge();
                    }
                    break;
                case 2: //Restart
                    restartGame();
                    break;
                case 3: //Main Menu
                    Mainmenu();
                    break;
                case 4: //Exit
                    GameEnd();
                    break;
            }
        }
        //The methods below are all the rooms that will be found in this game.
        public static void randomNumForEvent()
        {
            Random rand = new Random();
            int num = rand.Next(2); //Wanted to put this random generator in the event method but it would then initiate the event every
            if (num == 1)
            {
                randomEventBridge();
            }
        }
        public static void randomEventBridge()
        {
            if (bridgeEvent == false)
            {

                Console.WriteLine("Oh no! You enter the bridge and you see a fire has started, you quickly grab the extinguisher and put it out but the oxygen supply is damaged and depleting fast you'll have to fix it quick or you're doomed!");
                bridgeEvent = true;
                Console.ReadLine();
                Console.Clear();
                bridgeEventGame();
            }
            else
            {
                Bridge();
            }
            
        }
        public static DateTime startTime;
        public static TimeSpan total;
        public static void bridgeEventGame()
        {
            Console.WriteLine("You will have 5 seconds to answer this question. If you do not answer in time the oxygen will run out and all hope will be lost!\n\nPress any key to continue");
            Console.ReadLine() ;
            DateTime startTime=DateTime.Now;
            Console.WriteLine("What is the first man on the moons given/first name");
            string temp = Console.ReadLine();
            Console.Clear() ;
            string ans = temp.ToUpper();
            DateTime endTime = DateTime.Now;
            total=endTime-startTime;
            if(ans=="NEIL"&&total.Seconds<5.1||ans=="NIEL"&& total.Seconds < 5.1)
            {
            Console.WriteLine($"Seconds taken:  {total.Seconds:F1}");
            Console.WriteLine("\nYou did it! The oxygen level is returning to normal");
                Console.ReadLine();
                ShipStats();
                Console.Clear();
            }
            else
            {
                bridgeEventLoss();
            }
        }
        public static void bridgeEventLoss()
        {
            Console.WriteLine($"Seconds taken:  {total.Seconds:F1}");

            Console.WriteLine("You couldn't fix it in time, you're gasping for air but taking nothing in\n");
            Thread.Sleep(2000);
            Console.WriteLine("You feel yourself slipping into an eternal sleep");
            Thread.Sleep(2000);
            Console.Clear() ;
            Console.Write("Unfortunatly you have failed this mission. Would you like to return to main menu? (y or n):  ");
            string temp = Console.ReadLine();
            switch (temp)
            {
                case "y":
                case "Y":
                    Mainmenu();
                    break;
                case "n":
                case "N":
                    GameEnd();
                    break;
                default:
                    break;
            }
        }

        static void Intro()
        {

            string? playerChoice;

            Console.Clear();
            Console.WriteLine("There is a little bit of story, type skip if you wish to skip it, otherwise just hit enter to begin...");
            playerChoice = Console.ReadLine();
            playerChoice = playerChoice.ToUpper();

            switch (playerChoice)
            {
                case "SKIP":
                    {
                        Console.WriteLine("You have Chosen to skip, skipping...");
                        Thread.Sleep(15000);
                        Console.Clear();
                        Dorm();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("This story takes place in the year 2197, humanity has advanced to and beyond the stars, developing FTL engines \n(Faster Than Light) And, as humanity does, it used this technology to expand their territory.\nTo give themselves places to go, to get away from Earth. Which, at the time was breaching a population of over \n50 billion. Earth alone was far from enough to sustain this population, and so many fled aboard vast ships, heading for \nfaraway planets, for a second chance at life. You, happened to be aboard on of these ships...");
                        Console.WriteLine("Hit Enter to Begin...");
                        Console.ReadLine();
                        Console.Clear();
                        Dorm();
                        break;
                    }

            } 
        }

        //below are all the rooms
        public static void Dorm()
        {
            string temp = null;
            string currentRoom = "Dorm";
          
            Console.Clear();
            int userInput;
          

            if (currentRoom == "Dorm" && CheckInventory("Torch") == false && dormRoomCount == 0)
            {
                Console.WriteLine("You awaken in the dorm and it is dark. Maybe there is something in the room to help you see better." +
                    "\nWhat would you like to do, your options are:");
                Console.WriteLine("\n1    Look" +
                                  "\n2    Leave" +
                                  "\n3    Menu" +
                                  "\n4    Inventory" +
                                  "\n5    Map\n");

                userInput = ValidateUserInput(5);
                switch (userInput)
                {
                    case 1: //Look
                        LookDorm();
                        break;
                    case 2: //Leave
                        if (currentRoom == "Dorm" && CheckInventory("Torch") == false) 
                        {
                            Console.WriteLine("You cannot see, so you stumble around for a little bit. Making no progress, you may want to see if you can find something to light the way.");
                            Dorm();
                        }
                        break;
                    case 3: //Menu
                        IGmenu(ref currentRoom);
                        break;
                    case 4: //Inventory
                        InventoryMethod(Dorm);
                        break;
                    case 5:
                         DisplayMap();
                         break;
                }
            }
            
            if (currentRoom == "Dorm" && CheckInventory("Torch") == true && dormRoomCount == 0)
            {
                Console.WriteLine("You can now see around the room. \nThere are many beds but you seem to be the only one here. \nAre you alone ? \nMaybe you will find answers if you explore outside of the room, \nthrough the door in front of you that seems to lead to a hallway... ");
                Console.WriteLine("\n1    Look" +
                                  "\n2    Leave" +
                                  "\n3    Menu" +
                                  "\n4    Inventory" +
                                  "\n5    Map\n");
                
                userInput = ValidateUserInput(5);
                switch (userInput)
                {
                    case 1: //Look
                        LookDorm();
                        break;
                    case 2: //Leave
                        dormRoomCount++;
                        Hall();
                        break;
                    case 3: //Menu
                        IGmenu(ref currentRoom);
                        break;
                    case 4: //Inventory
                        InventoryMethod(Dorm);
                        break;
                    case 5:
                        DisplayMap();
                        break;
                }
            }

            if (currentRoom == "Dorm" && CheckInventory("Torch") == true && dormRoomCount >= 2)
            {
                Console.WriteLine("\nYou are in the Dorm");
                Console.WriteLine("\n1    Look" +
                                  "\n2    Leave" +
                                  "\n3    Menu" +
                                  "\n4    Inventory" +
                                  "\n5    Map\n");

                userInput = ValidateUserInput(5);
                switch (userInput)
                {
                    case 1: //Look
                        LookDorm();
                        break;
                    case 2: //Leave
                        dormRoomCount++;
                        Hall();
                        break;
                    case 3: //Menu
                        IGmenu(ref currentRoom);
                        break;
                    case 4: //Inventory
                        InventoryMethod(Dorm);
                        break;
                    case 5: //Map
                        DisplayMap();
                        break;
                }
            }
        }
        static void Hall()
        {
            Console.Clear();
            currentRoom = "Hall";
            int count = 0;
            Console.Clear();
            oxygenLevel = oxygenLevel - 25;
            Console.WriteLine("\nYou are in the hallway, most of the rooms are shut except for the dorm and the bridge down the end of the hallway. You could go in there or you could go back into the dorm.\nYour options are:");
            Console.WriteLine("\n1    Enter Dorm" +
                              "\n2    Enter Bridge" +
                              "\n3    Enter Med" +
                              "\n4    Enter Storage" +
                              "\n5    Enter AirLock" +
                              "\n6    Look" +
                              "\n7    Menu" +
                              "\n8    Inventory" +
                              "\n9    Map\n");

            int userInput;
            userInput = ValidateUserInput(9);

            do
            {
                switch (userInput)
                {
                    case 1: //Enter Dorm
                        dormRoomCount++;
                        Dorm();
                        break;
                    case 2: //Enter Bridge
                        Bridge();
                        break;
                    case 3: //Enter Med
                        if (power == true) 
                        {
                          Med(); 
                        }
                        else
                        {
                            Console.WriteLine("This room is locked, press enter to return");
                            Console.ReadLine();
                            Hall();
                        }
                        break;
                    case 4: //Enter Storage
                        if (power == true)
                        {
                            Storage();
                        }
                        else
                        {
                            Console.WriteLine("This room is locked, press enter to return");
                            Console.ReadLine();
                            Hall();
                        }
                        break;
                    case 5: //Enter Airlock
                        if (power == true)
                        {
                            AirLock();
                        }
                        else
                        {
                            Console.WriteLine("This room is locked, press enter to return");
                            Console.ReadLine();
                            Hall();
                        }
                        break;
                    case 6: //Look
                        LookHall();
                        break;
                    case 7: //Menu
                        IGmenu(ref currentRoom);
                        break;
                    case 8: //Inventory
                        InventoryMethod(Hall);
                        break;
                    case 9: //Map
                        DisplayMap();
                        break;
                }
            } while (count == 0);

            Console.ReadLine();
        }
        static void Med()
        {
            enemy = "Hobo";
            Console.WriteLine("You are in Medroom");
            Console.ReadLine();
            Console.Beep(3000, 200);
            Console.WriteLine("Out jumps a crazed space hobo from behind the med cabinet\n\n...\n");
            Thread.Sleep(2000);            
            Console.WriteLine("!!FIGHT!!");            
            oxygenLevel = oxygenLevel - 25;
            Console.ReadLine();
            Console.Clear();
            Combat();
            Hall();
        }
        static void Storage()
        {
            enemy = "Rat";
            Console.WriteLine("You are in the Storage room");
            Console.ReadLine();
            Console.Beep(3000, 200);
            Console.WriteLine("Opps! bad move going in that room\n\n...\n");
            Thread.Sleep(2000);
            Console.WriteLine("!!FIGHT!!");
            oxygenLevel = oxygenLevel - 25;
            Console.ReadLine();
            Console.Clear();
            Combat();                        
            Hall();
        }
        static void AirLock()
        {
            Console.Clear();
            Console.WriteLine("You are in the AirLock");
            oxygenLevel = oxygenLevel - 25;
            Console.ReadLine();
            Hall();
        }
        static void Bridge()
        {
            Console.Clear();
            currentRoom = "Bridge";
            string temp, playerChoice;
            oxygenLevel = oxygenLevel - 25;
            randomNumForEvent();
            Console.WriteLine("\nYou are in the bridge, the brain of the ship where messages are received and commands are sent throughout the rest of the vessel. There seems to be power in here as some computer lights flicker and there are beeping noises all around, it seems some parts of the ship are still working. Just like the dorm room and the hallway, the thick layer of dust on all of the controls would indicate that has not been any life here for quite some time. \nAre you truly alone floating through space... \nYour options are:");
            Console.WriteLine("\n1    Look" +
                              "\n2    Ship Stats" +
                              "\n3    Leave" +
                              "\n4    Menu" +
                              "\n5    Map\n");

            int userInput;
            userInput = ValidateUserInput(5);
            switch (userInput)
            {
                case 1: //Look
                    LookBridge();
                    break;
                case 2: //Ship Stats
                    ShipStats();
                    ShipSystems();
                    Bridge();
                    break;
                case 3: //Leave
                    Hall();
                    break;
                case 4: //Menu
                    IGmenu(ref currentRoom);
                    break;
                case 5: //Map
                     DisplayMap();
                     break;
            }
        }
        static void BridgeIntro()
        {
            Console.Clear();
            bool BridgeIntro = false;
            if (BridgeIntro == false)
            {
                Console.WriteLine("You have now entered what looks to be the main Bridge --");
                //Thread.Sleep(3000);
                Console.WriteLine("Press Enter to Continue");
                Console.ReadLine();
                //Thread.Sleep(2000);
                Console.WriteLine();
                Console.WriteLine("There seems to be power in here as all the lights and computer systems are still running");
                //Thread.Sleep(4000);
                Console.WriteLine();
                Console.WriteLine("You notice a console screen to your left showing information about the ships vitals... ");
                //Thread.Sleep(3000);
                Console.WriteLine("Press Enter to Inspect");
                Console.ReadLine();
                ShipStats();
                BridgeIntro = true;

            }
            else
            {
                // Nest all other code in here
            }
        }
        //Below this are all the "LOOK" methods.
        static void LookDorm()
        {
            Console.Clear();
            int dormRoomCount = 1;
            currentRoom = "Dorm";
            string playerChoice = null;

            Console.WriteLine("\nYou have looked around the room");
            if (currentRoom == "Dorm" && CheckInventory("Torch") == false)
            {
                Console.WriteLine("It is very dark in the dorm, but you manage notice a torch lying on the ground next to you, do you pick it up?");
                Console.WriteLine("\n1    Yes" +
                                  "\n2    No\n");

                int userInput;
                userInput = ValidateUserInput(2);
                switch (userInput)
                {
                    case 1: //Yes
                        Console.WriteLine("\nYou pick up the torch...(Press any Key)\n");
                       
                            Item torchItem; //This adds a torch to the inventory, there may be a simpler way however.
                            torchItem.Name = "Torch";
                            torchItem.Description = "A device for finding your way in the dark.";
                            torchItem.Quant = 1;
                            Inventory.Add(torchItem);
                        Console.ReadLine();
                        break;
                    case 2: //No
                        Console.WriteLine("\nYou decided not to pick up the torch, But you still cannot see.\nMaybe it would be better to pick it up...");
                        break;
                    default:
                        break;
                }

                Dorm();
            }
            else if (currentRoom == "Dorm" && CheckInventory("Torch") == true && dormRoomCount > 0)
            {
                Console.WriteLine("There is nothing else in the room \nPress any key...");
                dormRoomCount++;
                Console.ReadLine();
                Dorm();
            }
        }
        static void LookBridge()
        {
            Console.Clear();
            Console.WriteLine("In front of you to your left and right are the two pilot seats, various buttons and knobs in front of each. To your left is a computer console displaying the ship's status. To your right are a few more consoles with flashing ERROR screens. \nYou spot a manual on the controls to your left. \nWhat would you like to do?");
            Console.WriteLine("\n1    Check computer" +
                              "\n2    Stop looking\n");
            
            int userInput;
            userInput = ValidateUserInput(2);
            switch (userInput)
            {
                case 1: //Check Computer
                    ShipComputer();
                    break;
                case 2: //Stop Looking
                    Bridge();
                    break;
            }
        }
        static void ShipComputer()
        {
            Console.Clear();
            Console.WriteLine("You look over at the computer console, there are a couple things you can do here.");
            Console.WriteLine("\n1    Check oxygen levels and reactor core fuel" +
                              "\n2    Check ship health" +
                              "\n3    Turn the main power back on" +
                              "\n4    Fix Engines" +
                              "\n5    Fix Oxygen" +
                              "\n6    Leave" +
                              "\n7    Map\n");

            
            int userInput;
            userInput = ValidateUserInput(7);   
            switch (userInput)
            {
                case 1: //Check Oxygen and Reactor Core Fuel
                    ShipStats();
                    ShipComputer();
                    break;
                case 2: //Check Ship Health
                    ShipSystems();
                    ShipComputer();
                    break;
                case 3: //Turn power on
                    Task1();
                    taskCount++;
                    ShipComputer();
                    break;
                case 4: //Fix Engines
                    Task2();
                    taskCount++;
                    ShipComputer();
                    break;
                case 5: //Fix Oxygen
                    if (taskCount > 1)
                    {
                        Task3();
                    }
                    else
                    {
                        Console.WriteLine("You must complete other tasks first, press Enter to continue");
                        Console.ReadLine();
                    }
                    ShipComputer();
                    break;
                case 6: //Leave
                    LookBridge();
                    break;
                case 7: //Map
                     DisplayMap();
                     break;
            }
        }
        static void LookHall()
        {
            Console.Clear();
            int dormRoomCount = 1;
            Console.WriteLine("\nThe ship is on backup power and some of the doors seem to be locked shut, maybe if you get the main power back on they'll open.");
            Console.ReadLine();
            Hall();
        }
        static void ShipStats()
        {
            Console.Clear();
            string Border = new string('*', 25);

            Thread.Sleep(100);
            Console.WriteLine(Border);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"*  OXYGEN = {oxygenLevel}/999".PadRight(24) + "*");
            Thread.Sleep(100);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"*  ENERGY = {reactorCore}/1500".PadRight(24) + "*");
            Console.ResetColor();
            Console.WriteLine(Border);
            Console.WriteLine("Press Enter To Exit");
            Console.ReadLine();

        }
        // ShipSystems status window
        static void ShipSystems()
        {
            string Border = new string('-', 44);
            List<bool> components = new List<bool>();           
            string[] cNames = { "Long Ranged Comms", "Thrusters", "Reactor Core", "Ai Systems" };
            string A = "Active", D = "Disabled";
            
            Console.WriteLine(Border);
            components.Add(Comms);
            components.Add(Thrusters);
            components.Add(Reactor);
            components.Add(ShipAi);

            for (int j = 0; j < cNames.Length; j++)
            {
                if (components[j] == true)
                {                   
                    Console.Write($"|  {cNames[j]}".PadRight(31));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"[{A}]".PadRight(12));
                    Console.ResetColor();
                    Console.WriteLine("|");
                }
                else
                {                   
                    Console.Write($"|  {cNames[j]}".PadRight(31));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{D}]".PadRight(12));
                    Console.ResetColor();
                    Console.WriteLine("|");
                }
                
            }
            Console.WriteLine(Border);
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
        //Task 1 is for within the bridge/within the main computer
        public static void Task1()
        {
            Console.Clear();
            Random rand = new Random();
            int[] numbers = new int[7];
            int[] user = new int[7];
            string temp;
            int comp, guess = 0, correct = 0;

            Console.WriteLine("The ship is currently on backup power, which is why some doors are shut. There is a security lock on the ship's main power, you will have to hack it open. \nThe computer will display 7 numbers for a couple seconds, then clear the screen. You will have to remember what the numbers were then type them out one at a time in the correct spot. You need to remember at least 6 to progress.\nPress enter to begin");
            Console.ReadLine();
            Console.Clear();
            Thread.Sleep(700);
            Console.WriteLine("Starting in:");
            Thread.Sleep(700);
            Console.WriteLine("3");
            Thread.Sleep(700);
            Console.WriteLine("2");
            Thread.Sleep(700);
            Console.WriteLine("1");
            Thread.Sleep(700);
            Console.Clear();
            for (int i = 0; i < numbers.Length; i++)
            {
                comp = rand.Next(1, 10);
                numbers[i] = comp;
                Console.WriteLine(comp);
            }
            Thread.Sleep(2000);
            Console.Clear();
            Console.WriteLine("What were the numbers? Type them one at a time.");
            
            for (int i = 0; i < user.Length; i++)
            {
                Console.WriteLine($"Guess {i + 1}:");
                guess = ValidateUserInput(9);
                user[i] = guess;
            }
            

            Console.WriteLine();
            for (int i = 0; i < user.Length; i++)
            {
                if (user[i] == numbers[i])
                {
                    Console.WriteLine("Correct");
                    correct++;
                }
                else
                {
                    Console.WriteLine("Incorrect");
                }
            }
            Console.WriteLine();
            Console.WriteLine("Computer:");
            foreach (var item in numbers)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Your Answers:");
            foreach (var item in user)
            {
                Console.WriteLine(item.ToString());
            }
            if (correct >= 6)
            {
                power = true;
                Console.WriteLine("You did it! The ships energy has gone up by 200!\nPress enter to continue.");
                reactorCore = reactorCore + 200;
                Console.ReadLine();
            }
            else
            {
                string temp2;
                Console.WriteLine("You failed, try again? Y or N:");
                temp = Console.ReadLine();
                temp = temp.ToUpper();
                switch (temp)
                {
                    case "Y":
                        Task1();
                        break;

                }
            }
        }
        //Task 2 is for Engine/operation room once added
        public static void Task2()
        {
            int Round = 2;
            int Correct = 0;
            bool win = false;
            string Q1 = "V2ROCKET";
            string Q2 = "311";
            string Q3 = "SATURN";
            string Q4 = "VENUS";
            string Q5 = "1969";
            

            do
            {
                Correct = 0;
                Console.Clear();
                Round++;
                if (Round > 5)
                {
                    Console.WriteLine("--- You failed to fix the ships thruster =( ---");
                    Thread.Sleep(2000);
                    Lose2();
                    int frequency = 2000;

                    for (int i = 0; i < 10; i++)
                    {
                        Console.Beep(frequency, 300);
                        frequency -= 200;
                    }
                    break;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"You need to guess {Round} out of 5 answers correct in order to have the knowledge to fix the engine thruster");
                    Thread.Sleep(1000);
                    Console.WriteLine();
                    Console.WriteLine("Question 1");
                    Console.WriteLine();
                    Thread.Sleep(1000);
                    Console.Write("What is the name of the first rocket to go into space?\nsaturn5\nV2rocket\napollo1 \nsputnik\n\nAnswer:  ");
                    string Answer1 = Console.ReadLine();
                    Answer1 = Answer1.ToUpper();

                    if (Answer1 == Q1)
                    {
                        Console.WriteLine("Correct!");
                        Correct++;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Question 2");
                    Console.WriteLine();
                    Thread.Sleep(1000);
                    Console.Write("How many days was the Russian man Sergei Krikalev lost in space for?\n64 \n104 \n251 \n311 \n\nAnswer:  ");
                    string Answer2 = Console.ReadLine();
                    Answer2 = Answer2.ToUpper();

                    if (Answer2 == Q2)
                    {
                        Console.WriteLine("Correct!");
                        Correct++;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Question 3");
                    Console.WriteLine();
                    Thread.Sleep(1000);
                    Console.Write("What planet in our solar system has the most moons\n(Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune)?\n\nAnswer:  ");
                    string Answer3 = Console.ReadLine();
                    Answer3 = Answer3.ToUpper();

                    if (Answer3 == Q3)
                    {
                        Console.WriteLine("Correct!");
                        Correct++;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Question 4");
                    Thread.Sleep(1000);
                    Console.Write("What is the warmest planet in our solar system\n(Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune)?\n\nAnswer:  ");
                    string Answer4 = Console.ReadLine();
                    Answer4 = Answer4.ToUpper();

                    if (Answer4 == Q4)
                    {
                        Console.WriteLine("Correct!");
                        Correct++;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Question 5");
                    Thread.Sleep(1000);
                    Console.Write("What year did man first walk on the moon, 1964, 1969, 1971, 1968?\n\nAnswer:  ");
                    string Answer5 = Console.ReadLine();
                    Answer5 = Answer5.ToUpper();

                    if (Answer5 == Q5)
                    {
                        Console.WriteLine("Correct!");

                        Correct++;
                    }
                }
                if (Correct >= Round) win = true;
                if (win == true)
                {
                    Console.WriteLine($"You got {Correct} of 5 answers correct and have successfully fixed the ships thruster =)\nThe ship has gained 200 energy");
                    reactorCore = reactorCore + 200;
                    Thread.Sleep(2000);
                    Console.ReadLine();
                }

            } while ((Correct != Round) && (Correct < Round));
                
               
            
           
        }
        //Task 3 is for in the oxygen room once that has been made
        public static void Task3()
        {
            Console.WriteLine("You must enter the Oxygen Stabilizer Code.");
            string temp;
            char answer;
            int number = 1;
            do
            {
                Console.WriteLine("\nAre you ready: y or n");
                temp = Console.ReadLine();
                answer = Convert.ToChar(temp);
            } while ((answer != 'y') && (answer != 'n'));
            Console.WriteLine("Readying...");
            Thread.Sleep(1000);
            Console.Clear();

            int count = 0, num1, num2;
            Random rand = new Random();
            num1 = rand.Next(1, 101);
            Console.WriteLine("\nThe code is a number between 1-100, but you can't remember it.\nYou will have to guess quickly to find the answer before you black out\nYou will have 8 guesses.");
            do
            {
                Console.Write("\nPlease type a number:  ");
                num2 = ValidateUserInput(100);
                if (num2 > num1)
                {
                    Console.WriteLine("The number you are looking for is smaller than this");
                }
                else if (num2 < num1)
                {
                    Console.WriteLine("The number you are looking for is larger than this");
                }
                else if (num2 == num1)
                {
                    Console.WriteLine("Correct code entered");
                }
                count++;
                if ((count >= 8) && (num2 !=num1))
                {
                    Console.WriteLine("Oxygen Levels are critical! You have failed.");
                    Thread.Sleep(1000);
                    Lose1();

                }
                else if (num2 != num1)
                {
                    Console.WriteLine("Try a different number");
                }
                else
                {
                    Console.WriteLine("Oxygen levels stabilized.");
                }
            } while ((num2 != num1) && (count <= 8));

            Console.ReadLine();
        }
        public static void Lose1()
        {
            int frequency = 2000;

            for (int i = 0; i < 10; i++)
            {
                Console.Beep(frequency, 300);
                frequency -= 200;
            }
            Console.WriteLine("\n\nYou feel yourself starting to lose consciousness and you know the end is near.\nYou can no longer hold yourself up to the oxygen terminal and fall to the ground.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("This is the end for you...");
            Thread.Sleep(2000);
            Console.Clear();
            Console.Write("Unfortunatly you have failed this mission. Would you like to return to main menu? (y or n):  ");
            string temp = Console.ReadLine();
            switch (temp)
            {
                case "y":
                case "Y":
                    Mainmenu();
                    break;
                case "n":
                case "N":
                    GameEnd();
                    break;
                default:
                    break;
            }
        }
        public static void Lose2()
        {
            Console.WriteLine("\n\nYou got stuck in the thruster, there is no escape.");
            Console.WriteLine("You feel your body being torn apart...");
            Thread.Sleep(2000);
            Console.Clear();
            Console.WriteLine("Achievement unlocked - Blown away\n  -Failed the game gruesomely");
            Console.Write("Unfortunatly you have failed this mission. Would you like to return to main menu? \n1. Yes \n2. No  ");
            int temp = Convert.ToInt32(Console.ReadLine());
            switch (temp)
            {
                case 1:
                    Mainmenu();
                    break;
                case 2:
                    GameEnd();
                    break;
                default:
                    Console.WriteLine("Invalid input.");
                    Thread.Sleep(1000);
                    break;
            }
        }
        public static void lose3()
        {
            Console.WriteLine("\n\nYou thought you could escape did you ?");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Well you failed miserably");
            Thread.Sleep(2000);
            Console.Clear();
            Console.WriteLine("Achievement unlocked - Dicked by the enemy while trying to escape\n  -Failed the game embarrassingly");
            Console.Write("Unfortunatly you have failed this mission. Would you like to return to main menu? (y or n):  ");
            string temp = Console.ReadLine();
            switch (temp)
            {
                case "y":
                case "Y":
                    Mainmenu();
                    break;
                case "n":
                case "N":
                    GameEnd();
                    break;
                default:
                    break;
            }
        }
        public static void Win1(ref string[] inventory)
        {
            
            Console.Clear();
            Console.WriteLine(".");
            Thread.Sleep(250);
            Console.Write(" .");
            Thread.Sleep(250);
            Console.Write(" .");
            Thread.Sleep(250);
            Console.Clear();
            Console.WriteLine("The ship's oxygen is back online.\nDue to the period of time that the oxygen was offline you may experiance light headedness.\nIf so please make your way to a safe seating area.");
            Console.WriteLine("\nPress enter to continue.");
            Console.ReadLine();
            Console.WriteLine("\n\nYou did it! Some how you managed to guess the code that had slipped your mind before all of the oxygen ran out.");
            Console.WriteLine("You slump to the floor relieved that you will live to see another day. Once you feel better you will be able to make your way back to the bridge you will be able to set auto pilot to the nearest space station and receive proper medical care.");
            Console.WriteLine("\nPress enter to continue");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("You finished the game:\n\n                      Achievement Unlocked - Linear Completion!\n                           -Complete the game it was intended to be completed.");
            Console.Write("Would you like to return to main menu to play again? (y or n):  ");
            string temp = Console.ReadLine();
            switch (temp)
            {
                case "y":
                case "Y":
                    Mainmenu();
                    break;
                case "n":
                case "N":
                    GameEnd();
                    break;
                default:
                    break;
            }

            // Rooms not yet in game or may not be needed - Med, Reactor, Storage, Airlock
        }
        // Combat system to be used throughout
        public static void Combat()
        {
            while (enemyHP > 0 && playerHP > 0)
            {
                Random rand = new Random();
                int dodge = rand.Next(1, 101), counter = rand.Next(1, 101), escape = rand.Next(playerHP, playerHP + 15);
                if (escape > 100)
                    escape = 100;
                string Border = new string('=', 44), blank = "+".PadRight(43) + "+", T = "\t\t\t\t   ";
                Console.WriteLine(Border);
                Console.Write("+");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"  Player +{playerHP}".PadRight(31));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{enemy} +{enemyHP} ".PadRight(11));
                Console.ResetColor();
                Console.WriteLine("+\n+".PadRight(45) + "+");
                Console.WriteLine($"{blank}\n{blank}\n{blank}");
                Console.WriteLine($"+  (1) Punch{T}+\n+  (2) Kick{T}+\n+  (3) Tackle{T}+\n+  (4) Escape {escape}% \t\t\t   +");
                Console.WriteLine(Border);               
                Console.Write($"Enter here: ");
                int action = Convert.ToInt16(Console.ReadLine());
                Console.WriteLine();               
                // Enemy attack(counter) chance will be based on your attack choice
                switch (action)
                {
                    case 1: if (dodge > 70) // Punch 70% chance enemy will dodge                      
                        {
                            enemyHP -= 15;
                            Console.WriteLine("You punch the enemy for 15 damage\n");                            
                            Console.Beep(900, 80);
                        }
                        else Console.WriteLine("You missed\n");
                        Thread.Sleep(1000);
                        if (counter > 70) // Enemys next attack chance based from punch attack
                        {
                            playerHP -= 10;
                            Console.WriteLine("Enemy bites you for 10 damage");
                            Console.Beep(500, 200);                           
                        }
                        else Console.WriteLine("Enemy attacks but they missed");
                        break;
                    case 2: if (dodge > 35) // Kick 65% chance enemy will dodge                     
                        {
                            enemyHP -= 30;
                            Console.WriteLine("You kick the enemy for 30 damage\n");                           
                            Console.Beep(900, 80);
                        }
                        else Console.WriteLine("You missed\n");
                        Thread.Sleep(1000);
                        if (counter > 35) // 65% chance they will hit you on next attack
                        {
                            playerHP -= 15;
                            Console.WriteLine("Enemy slashes you for 15 damage");
                            Console.Beep(500, 200);
                        }
                        else Console.WriteLine("Enemy attacks but they missed");
                        break;
                    case 3: if (dodge > 50) // Tackle 50% chance enemy will dodge 
                        {
                            enemyHP -= 35;
                            Console.WriteLine("You tackle the enemy for 35 damage\n");                            
                            Console.Beep(900, 80);
                        }
                        else Console.WriteLine("You missed\n");
                        Thread.Sleep(1000);
                        if (counter > 60) // 40% chance they will hit you
                        {
                            playerHP -= 20;
                            Console.WriteLine("Enemy tears at you and you take 20 damage");
                            Console.Beep(500, 200);
                        }
                        else Console.WriteLine("Enemy tried to grab you but failed");
                        break;
                    case 4: if (escape > enemyHP)
                            Console.WriteLine("You got away (for now) Lucky...");
                        else
                            lose3();
                        break;
                    default: Console.WriteLine("Incorrect input");
                        break;
                }               
                Console.ReadLine();
                Console.WriteLine("Press enter");
                Console.Clear();
            }            
            if (enemyHP <= 0)
            {
                Console.WriteLine("You have slain your enemy...\n\nItem recieved - { MedRoom Key }");
                Console.Beep(500, 100);
                Console.Beep(1000, 100);
                Console.Beep(1500, 100);
                Console.ReadLine();
            }
            else if (playerHP <= 0)
            {
                Console.WriteLine("You have died");
                Console.ReadLine();
                Intro();
            }
        }        
        static void GameEnd()
        {
            Console.WriteLine("You have chosen to exit the game");
            Thread.Sleep(1000);
            Console.WriteLine("Thank you for playing, Goodbye!");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private static int ValidateUserInput(int numOptions)
        {
            bool isValidInput = false;
            int userInput = 0;

            while (!isValidInput)
            {
                string input = Console.ReadLine();

                if (int.TryParse(input, out userInput))
                {
                    if (userInput > 0 && userInput <= numOptions)
                    {
                        isValidInput = true;
                    }
                    else
                    {
                        Console.WriteLine("You must choose a valid option");
                    }
                }
            }

            return userInput;
        }

        private static void ScrollText(String text)
        {

            String[] TextSplit = text.Split("\n");

            for (int i = TextSplit.Length - 1; i > 0; i--)
            {

                Console.Clear();

                for (int j = i; j < TextSplit.Length; j++)
                {
                    Console.WriteLine(TextSplit[j]);
                }

                Thread.Sleep(10);

            }
        }
    } 
} 


