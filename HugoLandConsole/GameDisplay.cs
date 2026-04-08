using HugoLandCore.Data;
using HugoLandCore.Models;

namespace HugoLandConsole;

/// <summary>
/// Author : Justin Ducharme
/// Description : Class responsible for the display in the console.
/// Date : 2026-03-10
/// </summary>
internal class GameDisplay
{
    private readonly HugoLandContext _context;

    public GameDisplay(HugoLandContext context)
    {
        _context = context;
    }

    // ==============================================
    //  MENU PRINCIPAL
    // ==============================================

    public char DisplayMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("==========================================");
        Console.WriteLine("   Welcome to HugoLand: Conquest - Phase 1");
        Console.WriteLine("==========================================");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("  [1] New game");
        Console.WriteLine("  [2] Load save");
        Console.WriteLine("  [3] Quit");
        Console.WriteLine();
        Console.ResetColor();

        do
        {
            Console.Write("  Choice: ");
            string? input = Console.ReadLine();
            if (input == "1" || input == "2" || input == "3")
                return input[0];
            RemoveLastLine();
            RemoveLastLine();
            Console.WriteLine("Please enter one of the available options.");
        } while (true);
    }

    public (string name1, string name2) DisplayCreatePlayers()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("==========================================");
        Console.WriteLine("    Creating 2 players for the new Game");
        Console.WriteLine("==========================================");
        Console.ResetColor();
        Console.WriteLine();

        Console.Write("  Enter player 1 name: ");
        string name1 = Console.ReadLine()?.Trim() ?? "Player 1";
        if (string.IsNullOrEmpty(name1)) name1 = "Player 1";

        Console.Write("  Enter player 2 name: ");
        string name2 = Console.ReadLine()?.Trim() ?? "Player 2";
        if (string.IsNullOrEmpty(name2)) name2 = "Player 2";

        return (name1, name2);
    }

    public int DisplaySavesChoice(List<Game> saves)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("==========================================");
        Console.WriteLine("           Saved Games");
        Console.WriteLine("==========================================");
        Console.ResetColor();
        Console.WriteLine();

        int i = 0;
        foreach (Game save in saves)
        {
            ++i;
            string status = save.Status == GameStatus.InProgress ? "In progress" : "Finished";
            Console.WriteLine($"  [{i}] {save.Name}  -- Turn {save.TurnNumber}, {status}");
        }
        Console.WriteLine();

        do
        {
            Console.Write("  Choice (or X to go back): ");
            string? input = Console.ReadLine();
            if (input?.ToUpper() == "X") return -1;
            if (int.TryParse(input, out int iInput) && iInput > 0 && iInput <= i)
                return iInput;
            RemoveLastLine();
            RemoveLastLine();
            Console.WriteLine("Please enter one of the available options.");
        } while (true);
    }

    // ==============================================
    //  BANDEAU JOUEUR + CARTE
    // ==============================================

    public void DisplayMap(Game game, Player player, Map map,
        in List<Installation> installations, in List<MilitaryDetachment> detachments)
    {
        Console.Clear();

        // -- Bandeau joueur --
        bool isP1 = player.Id == game.Players.Min(p => p.Id);
        Console.ForegroundColor = isP1 ? ConsoleColor.Cyan : ConsoleColor.Red;
        Console.Write($"  Turn {game.TurnNumber}  --  {player.Name}");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  \u2605 {player.Gold} gold");
        Console.ResetColor();

        // -- Section Carte --
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("\u2500\u2500 Carte ");
        Console.WriteLine(new string('\u2500', Math.Max(0, 44 - 8)));
        Console.ResetColor();

        // En-tête colonnes
        Console.Write("  ");
        for (int j = 0; j < map.Width; j++)
            Console.Write(j.ToString().PadLeft(6));
        Console.WriteLine();

        // Grille
        for (int i = 0; i < map.Height; i++)
        {
            DrawHorizontalLine(map.Width);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(i.ToString().PadLeft(3));
            Console.ResetColor();
            Console.Write(" ");

            for (int j = 0; j < map.Width; j++)
            {
                Console.Write("|");
                Territory? t = map.Territories.FirstOrDefault(t => t.X == j && t.Y == i);
                if (t == null) { Console.Write("     "); continue; }

                string text = BuildCellText(t, player);
                SetCellColor(t, player);
                Console.Write(text.PadLeft(5));
                Console.ResetColor();
            }
            Console.WriteLine("|");
        }
        DrawHorizontalLine(map.Width);

        // Légende
        Console.WriteLine();
        bool p1IsFirst = game.Players.Min(p => p.Id) == player.Id;
        string p1Label = p1IsFirst ? player.Name : game.Players.First(p => p.Id != player.Id).Name;
        string p2Label = p1IsFirst ? game.Players.First(p => p.Id != player.Id).Name : player.Name;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  J1 " + p1Label.PadRight(12));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("  J2 " + p2Label.PadRight(12));
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("  \u231B Fort");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("   \u25B3 Camp");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("   \u00B7 Empty   ## soldiers");
        Console.ResetColor();
        Console.WriteLine();
    }

    private string BuildCellText(Territory t, Player player)
    {
        string open = t.Installation?.Type == InstallationType.Camp ? "\u25B3"
                     : t.Installation?.Type == InstallationType.Fortification ? "\u231B"
                     : " ";

        string mid = "";
        if (t.Detachment != null)
        {
            string letter = t.Detachment.PlayerId == 1 ? "1" : "2";
            if (t.Detachment.Force < 10) letter = letter.ToLower();
            mid = letter + " " + Math.Min(t.Detachment.Force, 999).ToString();
        }
        else if (t.Installation != null)
        {
            mid = "";
        }
        else
        {
            mid = "\u00B7";
        }

        return open + mid;
    }

    private void SetCellColor(Territory t, Player player)
    {
        if (t.Detachment != null)
            Console.ForegroundColor = t.Detachment.PlayerId == player.Id
                ? ConsoleColor.Cyan : ConsoleColor.Red;
        else if (t.Installation != null)
            Console.ForegroundColor = t.Installation.PlayerId == player.Id
                ? ConsoleColor.Cyan : ConsoleColor.Red;
        else
            Console.ForegroundColor = ConsoleColor.DarkGray;
    }

    // ==============================================
    //  LISTE DES ARMÉES
    // ==============================================

    public void DisplayArmyList(Player player, List<MilitaryDetachment> actionableArmies)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"\u2500\u2500 Armies of {player.Name} ");
        Console.WriteLine(new string('\u2500', Math.Max(0, 44 - 12 - player.Name.Length)));
        Console.ResetColor();

        for (int i = 0; i < actionableArmies.Count; i++)
        {
            var army = actionableArmies[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  \u25BA [{i + 1}]  ({army.Territory!.X},{army.Territory!.Y})");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"  \u2020  {army.Force} soldiers");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  \u00AE Energy {army.Energy}");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\u2500\u2500 Actions " + new string('\u2500', 34));
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"  Armies available: {actionableArmies.Count}");
        Console.WriteLine("  [0]    End my turn");
        Console.WriteLine("  [-1]   Forfeit");
        Console.ResetColor();
        Console.WriteLine();
    }

    public int DisplayArmySelection(Player player, List<MilitaryDetachment> actionableArmies)
    {
        do
        {
            Console.Write($"  Choose an army (1-{actionableArmies.Count}), 0 to end turn, -1 forfeit  > ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0 || choice == -1) return choice;
                if (choice >= 1 && choice <= actionableArmies.Count) return choice;
            }
            RemoveLastLine();
            RemoveLastLine();
            Console.WriteLine("  Invalid choice.");
        } while (true);
    }

    // ==============================================
    //  ACTIONS D'UNE ARMÉE
    // ==============================================

    public int DisplayArmyActions(Player player, MilitaryDetachment detachment)
    {
        if (detachment.Territory == null)
            return 6;

        bool opt1 = detachment.Energy > 0 && detachment.Force >= 10;
        bool opt2 = detachment.Territory.Installation == null && player.Gold >= 20 && detachment.Force >= 10;
        bool opt3 = detachment.Territory.Installation?.Type == InstallationType.Camp && player.Gold >= 50 && detachment.Force >= 10;
        bool opt4 = player.Gold >= 2 && detachment.Territory.Installation != null && detachment.Force >= 10;
        bool opt5 = detachment.Force >= 11;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\u2500\u2500 Army at ({detachment.Territory.X},{detachment.Territory.Y}) -- {detachment.Force} soldiers, {detachment.Energy} energy " + new string('\u2500', 5));
        Console.ResetColor();

        Console.Write("  Installation: ");
        if (detachment.Territory.Installation == null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("none");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string typeStr = detachment.Territory.Installation.Type == InstallationType.Camp ? "Camp" : "Fortification";
            string own = detachment.Territory.Installation.PlayerId == player.Id ? "yours" : "enemy";
            Console.WriteLine($"{typeStr} ({own})");
        }
        Console.ResetColor();
        Console.WriteLine();

        PrintAction("1", "Move", "1 energy", opt1, "Not enough energy");
        PrintAction("2", "Build a camp", "20 gold", opt2, "Installation already present or not enough gold");
        PrintAction("3", "Convert to fortification", "50 gold", opt3, "No camp here or not enough gold");
        PrintAction("4", "Reinforce", "2 gold/troop", opt4, "Not on an installation or not enough gold");
        PrintAction("5", "Split army", ">=11 troops", opt5, "Not enough soldiers");
        PrintAction("6", "Pass", "", true, "");

        Console.WriteLine();

        bool success = false;
        do
        {
            Console.Write("  Choice: ");
            success = int.TryParse(Console.ReadLine(), out int choice);
            if (!success || choice < 1 || choice > 6)
            {
                RemoveLastLine(); RemoveLastLine();
                Console.WriteLine("  Invalid input. Enter a number between 1 and 6.");
                success = false;
            }
            else
            {
                bool[] opts = { opt1, opt2, opt3, opt4, opt5, true };
                if (!opts[choice - 1])
                {
                    RemoveLastLine(); RemoveLastLine();
                    Console.WriteLine("  This action is not available.");
                    success = false;
                }
                else return choice;
            }
        } while (!success);
        return 0;
    }

    private void PrintAction(string key, string label, string cost, bool available, string reason)
    {
        if (available)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"  [{key}]  {label,-28}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(cost);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  [{key}]  {label,-28}");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"-- {reason}");
        }
        Console.ResetColor();
    }

    // ==============================================
    //  DÉPLACEMENT
    // ==============================================

    public char DisplayMoveArmy(Player player, Map map, MilitaryDetachment detachment)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\u2500\u2500 Movement from ({detachment.Territory!.X},{detachment.Territory.Y}) -- Energy: {detachment.Energy} " + new string('\u2500', 10));
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  Adjacent territories:");
        Console.WriteLine();

        bool nMove = DisplayAdjacentTerritory('N', 0, -1, player, map, detachment);
        bool sMove = DisplayAdjacentTerritory('S', 0, 1, player, map, detachment);
        bool oMove = DisplayAdjacentTerritory('O', -1, 0, player, map, detachment);
        bool eMove = DisplayAdjacentTerritory('E', 1, 0, player, map, detachment);

        Console.WriteLine();
        do
        {
            Console.Write("  Choice (N/S/O/E): ");
            string? input = Console.ReadLine();
            if (input == null) { RemoveLastLine(); RemoveLastLine(); Console.WriteLine("  Invalid input."); continue; }

            input = input.ToUpper();
            switch (input)
            {
                case "N": if (nMove) return 'N'; RemoveLastLine(); RemoveLastLine(); Console.WriteLine("  North is not accessible."); break;
                case "S": if (sMove) return 'S'; RemoveLastLine(); RemoveLastLine(); Console.WriteLine("  South is not accessible."); break;
                case "O": if (oMove) return 'W'; RemoveLastLine(); RemoveLastLine(); Console.WriteLine("  West is not accessible."); break;
                case "E": if (eMove) return 'E'; RemoveLastLine(); RemoveLastLine(); Console.WriteLine("  East is not accessible."); break;
                default: RemoveLastLine(); RemoveLastLine(); Console.WriteLine("  Invalid input. Enter N, S, O or E."); break;
            }
        } while (true);
    }

    private bool DisplayAdjacentTerritory(char c, sbyte dx, sbyte dy, Player player, Map map, MilitaryDetachment detachment)
    {
        int nx = detachment.Territory!.X + dx;
        int ny = detachment.Territory!.Y + dy;

        Console.Write($"  [{c}] ({nx},{ny}) -- ");
        Territory? t = map.Territories.FirstOrDefault(t => t.X == nx && t.Y == ny);

        if (t == null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Out of bounds");
            Console.ResetColor();
            return false;
        }

        if (t.Detachment == null) { Console.ForegroundColor = ConsoleColor.Gray; Console.Write("Empty    "); }
        else if (t.Detachment.PlayerId == player.Id) { Console.ForegroundColor = ConsoleColor.Cyan; Console.Write($"Friendly "); }
        else { Console.ForegroundColor = ConsoleColor.Red; Console.Write($"Enemy {t.Detachment.Force,3} "); }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  Cost: {t.TerrainEnergyCost} energy  ");

        if (detachment.Energy >= t.TerrainEnergyCost)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\u2713 Accessible");
            Console.ResetColor();
            return true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\u2717 Not enough energy");
            Console.ResetColor();
            return false;
        }
    }

    // ==============================================
    //  COMBAT
    // ==============================================

    public void DisplayArmyCombat(char attPlayer, float attFactor, float defFactor,
        MilitaryDetachment attacker, MilitaryDetachment defender)
    {
        char defPlayer = attPlayer == 'A' ? 'B' : 'A';
        bool attVictory = (attacker.Force * attFactor) > (defender.Force * defFactor);

        int attLosses = attVictory
            ? (int)Math.Floor(attacker.Force * 0.20)
            : (int)Math.Floor(attacker.Force * 0.60);
        int defLosses = attVictory
            ? (int)Math.Floor(defender.Force * 0.60)
            : (int)Math.Floor(defender.Force * 0.20);

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"\u2554\u2550\u2550\u2550\u2550\u2550\u2550 COMBAT at ({defender.Territory!.X},{defender.Territory.Y}) \u2550\u2550\u2550\u2550\u2550\u2550\u2557");
        Console.ResetColor();

        Console.WriteLine($"  Attacker ({attPlayer}): {attacker.Force} soldiers");
        Console.WriteLine($"  Defender ({defPlayer}): {defender.Force} soldiers");

        Console.Write("  Terrain: Plain (x1.0)   Installation: ");
        if (defender.Territory.Installation == null) Console.WriteLine("none");
        else if (defender.Territory.Installation.Type == InstallationType.Camp) Console.WriteLine("Camp (x1.1)");
        else Console.WriteLine("Fortification (x1.2)");

        Console.WriteLine();
        Console.WriteLine($"  Attacker factor: x{attFactor:F2}  ->  Effective force: {attacker.Force * attFactor:F1}");
        Console.WriteLine($"  Defender factor: x{defFactor:F2}  ->  Effective force: {defender.Force * defFactor:F1}");
        Console.WriteLine();

        if (attVictory)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  VICTORY of the attacker ({attPlayer})");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  VICTORY of the defender ({defPlayer})");
        }
        Console.ResetColor();

        Console.WriteLine($"  Losses {attPlayer}: {attacker.Force} x {(attVictory ? "20" : "60")}% = {attLosses}  ->  {attacker.Force - attLosses} remaining");
        Console.WriteLine($"  Losses {defPlayer}: {defender.Force} x {(attVictory ? "60" : "20")}% = {defLosses}  ->  {defender.Force - defLosses} remaining");

        int attRemains = attacker.Force - attLosses;
        int defRemains = defender.Force - defLosses;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        if (attRemains < 10) Console.WriteLine($"  {attPlayer} has {attRemains} soldiers (<10) -> DESTROYED");
        if (defRemains < 10) Console.WriteLine($"  {defPlayer} has {defRemains} soldiers (<10) -> DESTROYED");
        Console.ResetColor();

        int pillageTranches = attVictory ? defender.Force / 10 : attacker.Force / 10;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  Pillage: {pillageTranches * 5} gold  ({(attVictory ? defender.Force : attacker.Force)} / 10 = {pillageTranches} tranche(s))");
        Console.ResetColor();
        Console.WriteLine($"  Energy after battle: 0 (exhaustion)");

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\u255A\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255D");
        Console.ResetColor();
        Console.WriteLine();
    }

    // ==============================================
    //  DÉBUT DE TOUR
    // ==============================================

    public void DisplayBeginningTurn(Game game, Player player)
    {
        Console.Clear();
        bool isP1 = player.Id == game.Players.Min(p => p.Id);

        Console.ForegroundColor = isP1 ? ConsoleColor.Cyan : ConsoleColor.Red;
        Console.Write($"  Turn {game.TurnNumber}  --  {player.Name}");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  \u2605 {player.Gold} gold");
        Console.ResetColor();
        Console.WriteLine();

        // Énergie
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\u2500\u2500 Energy recovery (+2, max 5) " + new string('\u2500', 15));
        Console.ResetColor();

        foreach (var d in player.Detachments.Where(d => d.Territory != null && d.Force >= 10))
        {
            int before = d.Energy;
            int after = Math.Min(5, before + 2);
            Console.WriteLine($"  Army ({d.Territory!.X},{d.Territory!.Y}): {before} -> {after}");
        }
        Console.WriteLine();

        // Économie
        int nbFortif = player.Installations.Count(i => i.Type == InstallationType.Fortification);
        int earnings = nbFortif * 5;
        int maintenance = player.Detachments
            .Where(d => d.Force >= 10)
            .Sum(d => (int)Math.Ceiling(d.Force / 10.0));
        int balance = player.Gold + earnings - maintenance;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\u2500\u2500 Economy " + new string('\u2500', 34));
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Earnings  : +{earnings} gold  ({nbFortif} occupied fortification(s))");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Maintenance: -{maintenance} gold");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  Balance   : {player.Gold} -> {balance} gold");
        if (player.DebtTurns > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  Debt: {player.DebtTurns} turn(s)  [5 turns = defeat]");
        }
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"  Your armies ({player.Detachments.Count(d => d.Force >= 10)}) are ready.");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  Press Enter to continue");
        Console.ReadLine();
    }

    // ==============================================
    //  RÈGLEMENTS
    // ==============================================

    public void DisplayRules()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("  \u2694  RULES \u2014 HugoLand: Conquest  \u2694");
        Console.WriteLine(new string('\u2500', 50));
        Console.ResetColor();
        Console.WriteLine();

        Section("OBJECTIVE");
        Console.WriteLine("  Destroy all enemy armies (\u226510 soldiers) to win.");

        Section("MAP  (6\u00D76)");
        Console.WriteLine("  Energy cost per terrain:");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    Plain \u2192 1  |  Forest \u2192 2  |  Mountain \u2192 3  |  Sea \u2192 impassable");
        Console.ResetColor();

        Section("ARMIES");
        Console.WriteLine("  Force   : soldiers (active if \u226510).");
        Console.WriteLine("  Energy  : max 5 \u2014 recovery +2/turn.");
        Console.WriteLine("  Actions : move \u00B7 split (\u226511) \u00B7 build \u00B7 reinforce \u00B7 pass.");

        Section("INSTALLATIONS");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("  Camp (\u25B3)          \u2192  20 gold  \u2014 destroyed if army leaves.");
        Console.WriteLine("  Fortification (\u231B) \u2192  50 gold  \u2014 generates +5 gold/turn if occupied (\u226510).");
        Console.ResetColor();

        Section("ECONOMY");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  Earnings    : +5 gold per occupied fortification (start of turn).");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Maintenance : -(Force \u00F7 10) gold per army (automatic).");
        Console.WriteLine("  Debt        : gold < 0 \u2192 counter++.  5 consecutive turns = DEFEAT.");
        Console.WriteLine("  Recruit     : 2 gold / soldier  on an installation.");
        Console.ResetColor();

        Section("COMBAT");
        Console.WriteLine("  Triggered automatically when entering an enemy territory.");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Attacker  : Force \u00D7 [0.8\u20131.2]");
        Console.WriteLine("  Defender  : Force \u00D7 [0.9\u20131.3] \u00D7 terrain bonus \u00D7 installation bonus");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("  Pillage   : +(Force \u00F7 10) \u00D7 5 gold if enemy is destroyed.");
        Console.ResetColor();

        Section("VICTORY / DEFEAT");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  \u2713 Total conquest : opponent has no armies left (\u226510 soldiers).");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  \u2717 Bankruptcy     : 5 consecutive turns with gold < 0.");
        Console.ResetColor();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  [Press any key to continue]");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    private void Section(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\u250C\u2500\u2500 ");
        Console.Write(title);
        Console.ResetColor();
        Console.WriteLine();
    }

    // ==============================================
    //  RENFORCEMENT
    // ==============================================

    public int DisplayReinforcement(Player player)
    {
        Console.WriteLine();
        do
        {
            Console.Write("  How many troops to recruit?: ");
            if (int.TryParse(Console.ReadLine(), out int n) && n > 0 && player.Gold >= n * 2)
                return n;
            RemoveLastLine();
            RemoveLastLine();
            Console.WriteLine("  Invalid number or not enough gold.");
        } while (true);
    }

    // ==============================================
    //  ÉTAT JOUEUR (bandeau stats)
    // ==============================================

    public void DisplayPlayerState(Player player, Game game)
    {
        int maintenance = player.Detachments
            .Where(d => d.Force >= 10)
            .Sum(d => (int)Math.Ceiling(d.Force / 10.0));
        int nbFortif = player.Installations.Count(i => i.Type == InstallationType.Fortification);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("===================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  {player.Name} -- Turn {game.TurnNumber}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Gold: {player.Gold,-8}  Earnings: +{nbFortif * 5}/turn   Maintenance: -{maintenance}/turn");
        Console.WriteLine($"  Armies: {player.Detachments.Count(d => d.Force >= 10),-6}  Total force: {player.Detachments.Sum(d => d.Force),-5}  Fortifications: {nbFortif}");
        if (player.DebtTurns > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  Debt: {player.DebtTurns} turn(s)");
        }
        Console.WriteLine("===================================================");
        Console.ResetColor();
    }

    // ==============================================
    //  UTILITAIRES
    // ==============================================

    private void DrawHorizontalLine(int width)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    ");
        for (int j = 0; j < width; j++)
            Console.Write("+-----");
        Console.WriteLine("+");
        Console.ResetColor();
    }

    private void RemoveLastLine()
    {
        int cur = Console.CursorTop - 1;
        Console.SetCursorPosition(0, cur);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, cur);
    }
}