using HugoLandCore.Data;
using HugoLandCore.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace HugoLandConsole;
/// <summary>
/// Description : Classe responsable de l'affichage de la grille et de l'état du jeu dans la console.
/// Date : 2026-03-10
/// </summary>
internal class GameDisplay 
{
    private readonly HugoLandContext _context;

    public GameDisplay(HugoLandContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Description : Display the basic game information such as the current player, turn number, gold number, fortification level, and troop count in the console.
    /// </summary>
    /// <param name="player">True if player A, false if player B</param>
    /// <param name="turn">Current turn</param>
    /// <param name="gold">Number of gold</param>
    /// <param name="fortif">Number of fortifications that belongs to the player</param>
    /// <param name="troop">Total number of troops belonging to the player (troops, not armies)</param>
    /// <param name="MapWidth">Width of the map (number of columns)</param>
    /// <param name="MapHeight">Height of the map (number of rows)</param>
    public void DisplayMap(Game game, Player player, Map map, in List<Installation>installations, in List<MilitaryDetachment>detachments)// in == (const type &param) in C++
    {
        Console.Clear();

        int nbFortif = installations
            .Count(i => i.Territory.MapId == map.Id /*&& i.PlayerId == player.Id*/);

        int nbTroop = detachments
            .Where(d => d.PlayerId == player.Id)
            .Sum(d => d.Force);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Turn " + game.TurnNumber + " — Player ");
        if (player.Id == 0)// If player A
            Console.Write("A");
        else// If player B
            Console.Write("B");

        Console.WriteLine($"                                    Or: {player.Gold} | Fortif. : {nbFortif} | Troop : {nbTroop}");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;

        //       0    1    2    3    4    5    6    7    8    9   10   11   12   13   14
        Console.Write("  ");
        for (int j = 0; j < map.Width; j++)
        {
            Console.Write(j.ToString().PadLeft(6));
        }

        Console.WriteLine();
        for (int i = 0; i < map.Height; ++i)
        {
            //    +----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
            DisplayHorizontalLine(map.Width);

            //  0 |    |    |    |    |    |    |    |    |    |    |    |    |    |    |    |
            Console.Write(i.ToString().PadLeft(3));
            Console.Write(" ");
            for (int j = 0; j < map.Height; j++)
            {
                string text = "     ";
                Console.Write("|");

                Territory? territory = map.Territories.FirstOrDefault(t => t.X == j && t.Y == i);

                if (territory.Detachment.PlayerId == player.Id)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.Red;

                if (territory.Installation.Type == InstallationType.Camp)
                    text += "(";
                else if (territory.Installation.Type == InstallationType.Fortification)
                    text += "[";
                else
                    text += " ";

                if (territory.Detachment != null)
                {
                    if (territory.Detachment.Force <= 999)
                        text += territory.Detachment.Force.ToString();
                    else
                        text += "999";
                }

                if (territory.Installation.Type == InstallationType.Camp)
                    text += ")";
                else if (territory.Installation.Type == InstallationType.Fortification)
                    text += "]";
                else
                    text += " ";

                Console.Write(text.PadLeft(5));

                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine("|");
        }
        //    +----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
        DisplayHorizontalLine(map.Width);
        Console.WriteLine();
    }

    public void DisplayPlayerState(Player player, Game game)
    {
        //int earnings = player.Detachments.Sum(d => d.Force) * 5 + player.Installations.Count(i => i.Type == InstallationType.Fortification) * 10;
        int earnings = 7;// testing
        int maintenance = 5;// testing
        int nbArmies = 7;// testing
        int nbForces = 35;// testing
        int nbForts = 2;// testing

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("═══════════════════════════════════════════════════");

        Console.Write("Joueur ");
        if (player.Id == 0)// If player A
            Console.Write("A");
        else// If player B
            Console.Write("B");
        Console.WriteLine(" - Tour " + game.TurnNumber);

        Console.WriteLine("Gold: " + player.Gold.ToString().PadLeft(10) +
            "Earnings: +" + earnings.ToString().PadLeft(3) +
            "/ turn     Maintenance: -" + maintenance.ToString().PadLeft(4) + "/ turn");

        Console.WriteLine("Armies: " + nbArmies.ToString().PadLeft(8) + 
            "Total force: " + nbForces.ToString().PadLeft(3) +
            "     Fortifications: " + nbForts);

        Console.WriteLine("Debt: " + player.DebtTurns.ToString().PadLeft(2) + " turn(s)");

        Console.WriteLine("═══════════════════════════════════════════════════");
    }

    /*
    public void ShowArmyChoice(Player player, Map map, List<MilitaryDetachment> detachments)
    {
        bool success = false;
        int x = 0;
        int y = 0;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine();
        do
        {
            Console.WriteLine("Choose an army to move (x y): ");
            string input = Console.ReadLine();
            string[] parts = input.Split(' ');

            if (parts.Length != 2)
            {
                success = false;
                Removelastline();
                Removelastline();
                Console.WriteLine("Invalid input. Please enter two integers separated by a space.");
            }
            else
            {
                success = int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y);
                if (success == false)
                {
                    Removelastline();
                    Removelastline();
                    Console.WriteLine("Invalid input. Please enter two integers separated by a space.");
                }
                else
                {
                    if (x > map.Width || x <= 0 || y > map.Height || y <= 0)
                    {
                        Removelastline();
                        Removelastline();
                        Console.WriteLine("Invalid input. Coordinates must be within the map boundaries.");
                        success = false;
                    }
                    else// if the input is valid, we can check if there is an army at the coordinates
                    {
                        MilitaryDetachment? detachment = detachments.FirstOrDefault(d => d.PlayerId == player.Id && d.Territory.X == x && d.Territory.Y == y);
                        if (detachment == null)
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("Invalid input. There is no army at the given coordinates.");
                            success = false;
                        }
                        else
                        {
                            Console.WriteLine("You have chosen the army at (" + x + ", " + y + ") with force " + detachment.Force);
                            success = true;
                        }
                    }
                }
            }
        } while (success == false);
    }
    */

    public int DisplayArmyActions(Player player, MilitaryDetachment detachment)
    {
        bool success = false;
        // Tests to see if the different actions are available for the player, based on the state of the army and the territory
        bool opt1 = (detachment.Energy > 0);
        bool opt2 = (detachment.Territory.Installation == null && player.Gold >= 20);
        bool opt3 = ((detachment.Territory.Installation == null || detachment.Territory.Installation.Type != InstallationType.Fortification) && player.Gold >= 50);
        bool opt4 = (player.Gold > 1 && detachment.Territory.Installation != null);
        bool opt5 = (detachment.Force > 10);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"--- Army in ({detachment.Territory.X}, {detachment.Territory.Y})" +
            $" - {detachment.Force} troops, {detachment.Energy} energy ---");
        Console.ForegroundColor= ConsoleColor.Gray;
        Console.Write("Installation : ");
        if (detachment.Territory.Installation == null)
            Console.WriteLine("none");
        else
        {
            if (detachment.Territory.Installation.Type == InstallationType.Camp)
                Console.Write("Camp (");
            else
                Console.Write("Fortification (");
            /*
            if (detachment.Territory.Installation.PlayerId == player.Id)
                Console.WriteLine("yours)");
            else
                Console.Write("enemy)");
            */
        }
        Console.WriteLine("  Actions available:");
        Console.Write("    [1] Move                      (1 energy)");
        if (!opt1)
            Console.WriteLine("   — Impossible: Not enough energy");
        else
            Console.WriteLine();

        Console.Write("    [2] Build a camp              (20 gold)");
        if (!opt2)
            Console.WriteLine("   — Impossible: Installation already present or not enough gold");
        else
            Console.WriteLine();

        Console.Write("    [3] Convert to fortification  (50 gold)");
        if (!opt3)
            Console.WriteLine("   — Impossible: Already a fortification or not enough gold");
        else
            Console.WriteLine();

        Console.Write("    [4] Reinforce                 (2 gold/soldier)");
        if (!opt4)
            Console.WriteLine("   — Impossible: Not on an installation or not enough gold");
        else
            Console.WriteLine();

        Console.Write("    [5] Split the army)");
        if (!opt5)
            Console.WriteLine("   — Impossible: Not enough soldiers");
        else
            Console.WriteLine();

        Console.WriteLine("    [6] Pass (do nothing)");
        Console.WriteLine();
        do
        {
            Console.Write("  Choice: ");
            success = int.TryParse(Console.ReadLine(), out int choice);
            if (success == false || choice < 1 || choice > 6)
            {
                Removelastline();
                Removelastline();
                Console.WriteLine("Invalid input. Please enter a number between 1 and 6.");
                success = false;
            }
            else
            {
                switch (choice)
                {
                    case 1:
                        if (!opt1)
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("Invalid choice. Not enough energy.");
                            success = false;
                            break;
                        }
                        return choice;

                    case 2:
                        if (!opt2)
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("Invalid choice. Installation already present or not enough gold.");
                            success = false;
                            break;
                        }
                        return choice;

                    case 3:
                        if (!opt3)
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("Invalid choice. Already a fortification or not enough gold.");
                            success = false;
                            break;
                        }
                        return choice;

                    case 4:
                        if (!opt4)
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("Invalid choice. Not on an installation or not enough gold.");
                            success = false;
                            break;
                        }
                        return choice;

                    case 5:
                        if (!opt5)
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("Invalid choice. Not enough soldiers.");
                            success = false;
                            break;
                        }
                        return choice;

                    case 6:
                        return choice;

                    default:
                        Removelastline();
                        Removelastline();
                        Console.WriteLine("Something went wrong, try again.");
                        success = false;
                        break;
                }
            }
        } while (success == false);
        return 0;// This line will never be reached, but it is necessary to compile the code
    }

    public void DisplayMoveArmy(Player player, MilitaryDetachment detachment)
    {
        Console.WriteLine($"--- Movement from ({detachment.Territory.X}, {detachment.Territory.Y}) - Energy: {detachment.Energy} ---");

        /*
        ---Déplacement depuis(3, 2) — Énergie: 5-- -

  Territoires adjacents:
        [N] (3, 1) — Vide Coût : 1 énergie   ✓ Accessible
        [S](3, 3) — Vide Coût : 1 énergie   ✓ Accessible
        [E](4, 2) — Ennemi B12        Coût: 1 énergie   ✓ Accessible(COMBAT)
        [O](2, 2) — Vide Coût : 1 énergie   ✓ Accessible

  Choix(N / S / E / O ou X pour annuler) :
        */
    }

    private void Removelastline()
    {
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop - 1);
    }

    private void DisplayHorizontalLine(in int MapWidth)
    {
        //    +----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
        Console.Write("    ");
        for (int j = 0; j < MapWidth; j++)
        {
            Console.Write("+-----");
        }
        Console.WriteLine("+");
    }
}
