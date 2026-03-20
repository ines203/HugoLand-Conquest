using HugoLandCore.Data;
using HugoLandCore.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Net.Mail;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace HugoLandConsole;
/// <summary>
/// Author : Justin Ducharme
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

    public void DisplayMap(Game game, Player player, Map map, in List<Installation>installations, in List<MilitaryDetachment>detachments)// in == (const type &param) in C++
    {
        Console.Clear();

        int nbFortif = installations
            .Count(i => i.Territory.MapId == map.Id && i.PlayerId == player.Id);

        int nbTroop = detachments
            .Where(d => d.PlayerId == player.Id)
            .Sum(d => d.Force);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Turn " + game.TurnNumber + " — Player ");
        if (player.Id == 1)// If player A
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
                string text = "";
                Console.Write("|");

                Territory? territory = map.Territories.FirstOrDefault(t => t.X == j && t.Y == i);

                if (territory.Detachment != null && territory.Detachment.PlayerId == player.Id)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (territory.Installation != null && territory.Installation.PlayerId == player.Id)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.Red;

                if (territory.Installation != null && territory.Installation.Type == InstallationType.Camp)
                    text += "(";
                else if (territory.Installation != null && territory.Installation.Type == InstallationType.Fortification)
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

                if (territory.Installation != null && territory.Installation.Type == InstallationType.Camp)
                    text += ")";
                else if (territory.Installation != null && territory.Installation.Type == InstallationType.Fortification)
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
        int earnings = player.Detachments.Sum(d => d.Force) * 5 + player.Installations.Count(i => i.Type == InstallationType.Fortification) * 10;
        //int earnings = 7;// testing
        int maintenance = 5;// testing
        int nbArmies = 7;// testing
        int nbForces = 35;// testing
        int nbForts = 2;// testing

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("═══════════════════════════════════════════════════");

        Console.Write("Joueur ");
        if (player.Id == 1)// If player A
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

            if (detachment.Territory.Installation.PlayerId == player.Id)
                Console.WriteLine("yours)");
            else
                Console.Write("enemy)");
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

    public char DisplayMoveArmy(Player player, Map map, MilitaryDetachment detachment)
    {
        string? input = null;

        Console.WriteLine($"--- Movement from ({detachment.Territory.X}, {detachment.Territory.Y}) - Energy: {detachment.Energy} ---");
        Console.WriteLine();
        Console.WriteLine("Adjacent territories:");

        bool nMove = DisplayAdjacentTerritory('N', 0, -1,player, map, detachment);
        bool sMove = DisplayAdjacentTerritory('S', 0, 1,player, map, detachment);
        bool wMove = DisplayAdjacentTerritory('W', -1, 0,player, map, detachment);
        bool eMove = DisplayAdjacentTerritory('E', 1, 0,player, map, detachment);

        Console.WriteLine();
        do
        {
            Console.Write("Choice(N / S / W / E): ");
            input = Console.ReadLine();
            if (input == null)
            {
                Removelastline();
                Removelastline();
                Console.WriteLine("Invalid input. Please enter either N / S / W / E");
            }
            else
            {
                input = input.ToUpper();
                switch (input)
                {
                    case "N":
                        if (nMove)
                            return 'N';
                        else
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("North is not accessible");
                        }
                        break;
                    case "S":
                        if (sMove)
                            return 'S';
                        else
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("South is not accessible");
                        }
                        break;
                    case "W":
                        if (wMove)
                            return 'W';
                        else
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("West is not accessible");
                        }
                        break;
                    case "E":
                        if (eMove)
                            return 'E';
                        else
                        {
                            Removelastline();
                            Removelastline();
                            Console.WriteLine("East is not accessible");
                        }
                        break;
                    default:
                        Removelastline();
                        Removelastline();
                        Console.WriteLine("Invalid input. Please enter either N / S / W / E");
                        break;
                }
            }
        } while (true);
    }

    public void DisplayArmyCombat(char attPlayer, float attFactor, float defFactor, MilitaryDetachment attacker, MilitaryDetachment defender)
    {
        char defPlayer = (attPlayer == 'A') ? 'B' : 'A';
        int attNewForce = 0;
        int defNewForce = 0;
        int pillagedSlice = 0;

        Console.WriteLine($"════════════════ COMBAT en ({defender.Territory.X},{defender.Territory.Y}) ════════════════");
        Console.Write($"Attacker ({attPlayer}) : ");
        Console.WriteLine((attacker.Force + " troops").PadLeft(14) + $"Defender ({defPlayer}) : " + defender.Force + "troops");

        Console.Write("Terrain : Plain (x1.0)      Installation : ");
        if (defender.Territory.Installation == null)
            Console.WriteLine("None");
        else if (defender.Territory.Installation.Type == InstallationType.Camp)
            Console.WriteLine("Camp (x1.1)");
        else if (defender.Territory.Installation.Type == InstallationType.Fortification)
            Console.WriteLine("Fortification (x1.2)");

        Console.WriteLine();
        Console.WriteLine($"Attacker factor : x{attFactor:F2}     Effective strength : {(attacker.Force * attFactor):F1}");
        Console.WriteLine($"Defender factor : x{defFactor:F2}     Effective strength : {(attacker.Force * defFactor):F1}");

        bool attVictory = (attacker.Force * attFactor) > (defender.Force * defFactor);
        Console.Write("VICTORY of the ");

        if (attVictory)
        {// Attacker wins
            attNewForce = attacker.Force * (int)0.2f;
            defNewForce = defender.Force * (int)0.6f;
            Console.WriteLine($"attacker ({attPlayer})");
            Console.WriteLine();
            Console.WriteLine($"Loses attacker (winner) : {attacker.Force} x 20% = {attNewForce}   -> {attacker.Force - attNewForce} troops left");
            Console.WriteLine($"Loses defender (loser) : {defender.Force} x 60% = {defNewForce}   -> {defender.Force - defNewForce} troops left");

            pillagedSlice = (defender.Force / 10);
        }
        else
        {// Defender wins
            defNewForce = defender.Force * (int)0.2f;
            attNewForce = attacker.Force * (int)0.6f;
            Console.WriteLine($"defender ({attPlayer})");
            Console.WriteLine();
            Console.WriteLine($"Loses attacker (loser) : {attacker.Force} x 60% = {attNewForce}   -> {attacker.Force - attNewForce} troops left");
            Console.WriteLine($"Loses defender (winner) : {defender.Force} x 20% = {defNewForce}   -> {defender.Force - defNewForce} troops left");

            pillagedSlice = (defender.Force / 10);
        }
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Red;
        if (attNewForce < 10)
            Console.WriteLine($"{attPlayer} has {attacker.Force - attNewForce} troops (<10) -> DESTRUCTION");
        if (defNewForce < 10)
            Console.WriteLine($"{defPlayer} has {defender.Force - defNewForce} troops (<10) -> DESTRUCTION");
        Console.ForegroundColor= ConsoleColor.Gray;

        if (attVictory)
            Console.WriteLine($"Pillage : {pillagedSlice * 5} gold ({defender.Force} / 10 = {pillagedSlice} tranches)");
        else
            Console.WriteLine($"Pillage : {pillagedSlice * 5} gold ({attacker.Force} / 10 = {pillagedSlice} tranches)");

        Console.WriteLine("Energy after battle : 0 (exhaustion)");
        Console.WriteLine("═════════════════════════════════════════════════");
    }

    public void DisplayBeginningTurn (Game game, Player player)
    {
        Console.Clear();
        int nbFortif = player.Installations
            .Count(i => i.Type == InstallationType.Fortification);
        int maintenance = 0;

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($" --- Start of turn {game.TurnNumber} — Player ");
        if (player.Id == 1)// If player A
            Console.Write("A");
        else// If player B
            Console.Write("B");
        Console.WriteLine(" ---");
        Console.WriteLine();

        Console.WriteLine("Energy gained (+2)");
        foreach(MilitaryDetachment detachment in player.Detachments)
        {
            // Calculation maintenance for later
            maintenance += detachment.Force / 10;

            if (detachment.Energy < 4)
                Console.WriteLine($"Army ({detachment.Territory.X},{detachment.Territory.Y}) : {detachment.Energy} -> {detachment.Energy + 2}");
            else if (detachment.Energy < 5)
                Console.WriteLine($"Army ({detachment.Territory.X},{detachment.Territory.Y}) : {detachment.Energy} -> 5");
        }
        Console.WriteLine();

        Console.WriteLine($"Earnings : +{nbFortif * 5} gold  ({nbFortif} owned fortification)");
        Console.WriteLine($"Maintenance : -{maintenance} gold for armies");
        Console.WriteLine($"Balance : {player.Gold} -> {(nbFortif * 5) - maintenance} gold");
        Console.WriteLine();
        Console.WriteLine($" --- Your armies ({player.Detachments.Count}) are ready ---");
        Console.WriteLine("Press Enter to continue");
        Console.ReadLine();
    }

    public char DisplayMenu()
    {
        Console.WriteLine("══════════════════════════════════════");
        Console.WriteLine("      HugoLand: Conquest — Phase 1");
        Console.WriteLine("══════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  [1] New game");
        Console.WriteLine("  [2] Load save");
        Console.WriteLine("  [3] Quit");
        Console.WriteLine();

        do
        {
            Console.Write("  Choice :");
            string input = Console.ReadLine();
            if (input == "1" || input == "2" || input == "3")
                return input[0];
            Removelastline();
            Removelastline();
            Console.WriteLine("Please enter one of the available options");
        } while (true);
        return '0';
    }

    public int DisplaySavesChoice(List<Game> saves)
    {
        int i = 0;
        int iInput = 0;
        bool success = false;
        Console.WriteLine("--- Saved games ---");
        foreach(Game save in saves)
        {
            ++i;
            Console.WriteLine($"  [{i}] Partie {save.Name}  - Turn {save.TurnNumber}, {save.Status}");
        }
        Console.WriteLine();
        //  [1] Partie 2025-03-15 14h30  — Tour 12, En cours
        do
        {
            Console.Write("  Choice :");
            string input = Console.ReadLine();
            success = int.TryParse(input, out iInput);
            if (success)
            {
                if (iInput > 0 && iInput <= i)
                    return iInput;
            }
            Removelastline();
            Removelastline();
            Console.WriteLine("Please enter one of the available options");
        } while (true);
        return 0;
    }

    private void Removelastline()
    {
        int currentLineCursor = Console.CursorTop - 1;
        Console.SetCursorPosition(0, currentLineCursor);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
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
    private bool DisplayAdjacentTerritory(char c, sbyte x, sbyte y,Player player,  Map map, MilitaryDetachment detachment)
    {
        Console.Write($"[{c}] ({detachment.Territory.X + x}, {detachment.Territory.Y + y}) - ");
        Territory? territory = map.Territories.FirstOrDefault(
            t => t.X == detachment.Territory.X + x &&
            t.Y == detachment.Territory.Y + y);
        if (territory != null)
        {
            if (territory.Detachment == null)
                Console.Write("Empty");
            else if (territory.Detachment.PlayerId == player.Id)
                Console.Write("Friendly");
            else
                Console.Write("Enemy " + territory.Detachment.Force.ToString().PadLeft(3));
            Console.Write("Cost : 1 energy  ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Accessible");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("              Inaccessible");
            Console.ForegroundColor = ConsoleColor.Gray;
            return false;
        }
    }
}
