using System;

namespace HugoLandConsole;
/// <summary>
/// Description : Classe responsable de l'affichage de la grille et de l'état du jeu dans la console.
/// Date : 2026-03-10
/// </summary>
internal static class GameDisplay 
{
    /// <summary>
    /// Description : Display the basic game information such as the current player, turn number, gold number, fortification level, and troop count in the console.
    /// </summary>
    /// <param name="player">True if player A, false if player B</param>
    /// <param name="turn">Current turn</param>
    /// <param name="gold">Number of gold</param>
    /// <param name="fortif">Number of fortifications that belongs to the player</param>
    /// <param name="troop">Total number of troops belonging to the player (troops, not armies)</param>
    public static void DisplayMap(bool player, int turn, int gold, int fortif, int troop)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Turn " + turn + " — Player ");
        if (player)// If player A
            Console.Write("A");
        else// If player B
            Console.Write("B");
        Console.Write($"                                    Or: {gold} | Fortif. : {fortif} | Troop : {troop}");
        Console.ForegroundColor = ConsoleColor.Gray;

        
    }
}
