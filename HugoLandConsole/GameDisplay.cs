using HugoLandCore.Data;
using HugoLandCore.Models;
using System;

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
        int nbFortif = 2;
        //int nbFortif = installations
        //    .Count(i => i.Territory.MapId == map.Id && i.PlayerId == player.Id);

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

        //
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
                Console.Write("|");
                Console.Write("     ");
            }
            Console.WriteLine("|");
        }
        //    +----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
        DisplayHorizontalLine(map.Width);
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
