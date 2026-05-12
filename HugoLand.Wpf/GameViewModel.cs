using System.ComponentModel;
using System.Runtime.CompilerServices;
using HugoLandCore.Models;

namespace HugoLandWpf;

/// <summary>
/// ViewModel pour le panneau de jeu — implémente INotifyPropertyChanged
/// 
/// </summary>
public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    //Joueur actif

    private string _playerName = "";
    public string PlayerName
    {
        get => _playerName;
        set { _playerName = value; Notify(); Notify(nameof(TurnHeader)); }
    }

    private int _turnNumber;
    public int TurnNumber
    {
        get => _turnNumber;
        set { _turnNumber = value; Notify(); Notify(nameof(TurnHeader)); }
    }

    public string TurnHeader => $"{PlayerName}  —  Turn {TurnNumber}";

    private int _gold;
    public int Gold
    {
        get => _gold;
        set { _gold = value; Notify(); Notify(nameof(GoldText)); Notify(nameof(IsInDebt)); }
    }

    public string GoldText => $"{_gold} gold";
    public bool IsInDebt => _gold < 0;

    private int _debtTurns;
    public int DebtTurns
    {
        get => _debtTurns;
        set { _debtTurns = value; Notify(); Notify(nameof(DebtText)); Notify(nameof(HasDebt)); }
    }

    public string DebtText => $"⚠ Debt: {_debtTurns}/5 turns";
    public bool HasDebt => _debtTurns > 0;

    private bool _isPlayer1;
    public bool IsPlayer1
    {
        get => _isPlayer1;
        set { _isPlayer1 = value; Notify(); }
    }

    //Stats armées

    private int _armyCount;
    public int ArmyCount
    {
        get => _armyCount;
        set { _armyCount = value; Notify(); }
    }

    private int _totalForce;
    public int TotalForce
    {
        get => _totalForce;
        set { _totalForce = value; Notify(); }
    }

    private int _fortressCount;
    public int FortressCount
    {
        get => _fortressCount;
        set { _fortressCount = value; Notify(); }
    }

    private int _earnings;
    public int Earnings
    {
        get => _earnings;
        set { _earnings = value; Notify(); Notify(nameof(EarningsText)); }
    }

    private int _maintenance;
    public int Maintenance
    {
        get => _maintenance;
        set { _maintenance = value; Notify(); Notify(nameof(MaintenanceText)); }
    }

    public string EarningsText   => $"+{_earnings}";
    public string MaintenanceText => $"/-{_maintenance}";

    // Armée sélectionnée

    private string _selectedArmyInfo = "Click on one of your armies";
    public string SelectedArmyInfo
    {
        get => _selectedArmyInfo;
        set { _selectedArmyInfo = value; Notify(); }
    }

    private bool _canMove;
    public bool CanMove
    {
        get => _canMove;
        set { _canMove = value; Notify(); }
    }

    private bool _canBuildCamp;
    public bool CanBuildCamp
    {
        get => _canBuildCamp;
        set { _canBuildCamp = value; Notify(); }
    }

    private bool _canFortify;
    public bool CanFortify
    {
        get => _canFortify;
        set { _canFortify = value; Notify(); }
    }

    private bool _canReinforce;
    public bool CanReinforce
    {
        get => _canReinforce;
        set { _canReinforce = value; Notify(); }
    }

    private bool _canSplit;
    public bool CanSplit
    {
        get => _canSplit;
        set { _canSplit = value; Notify(); }
    }

    private bool _canPass;
    public bool CanPass
    {
        get => _canPass;
        set { _canPass = value; Notify(); }
    }

    //Titre carte

    private string _mapTitle = "Battle Map";
    public string MapTitle
    {
        get => _mapTitle;
        set { _mapTitle = value; Notify(); }
    }

    // Journal

    private string _combatLog = "";
    public string CombatLog
    {
        get => _combatLog;
        set { _combatLog = value; Notify(); }
    }

    // Méthode de mise à jour

    public void UpdateFromGame(Game game, Player player)
    {
        PlayerName  = player.Name;
        TurnNumber  = game.TurnNumber;
        Gold        = player.Gold;
        DebtTurns   = player.DebtTurns;
        IsPlayer1   = player.Id == game.Players.Min(p => p.Id);
        ArmyCount   = player.Detachments.Count(d => d.Force >= 10);
        TotalForce  = player.Detachments.Sum(d => d.Force);
        FortressCount = player.Installations.Count(i => i.Type == InstallationType.Fortification);
        Earnings    = FortressCount * 5;
        Maintenance = player.Detachments.Where(d => d.Force >= 10)
                           .Sum(d => (int)Math.Ceiling(d.Force / 10.0));
        MapTitle    = $"Battle Map  {game.Map!.Width}×{game.Map.Height}  |  Turn {game.TurnNumber}  |  {player.Name}'s turn";
    }

    public void UpdateSelectedArmy(MilitaryDetachment? army, Player player)
    {
        if (army?.Territory == null)
        {
            SelectedArmyInfo = "Click on one of your armies";
            CanMove = CanBuildCamp = CanFortify = CanReinforce = CanSplit = CanPass = false;
            return;
        }

        bool hasInst = army.Territory.Installation != null;
        bool isCamp  = army.Territory.Installation?.Type == InstallationType.Camp;

        SelectedArmyInfo =
            $"({army.Territory.X},{army.Territory.Y})  {army.Force} troops\n" +
            $"Energy: {army.Energy}/5  |  {army.Territory.Terrain}" +
            (hasInst ? $"\n{army.Territory.Installation!.Type}" : "");

        CanMove      = army.Energy > 0;
        CanBuildCamp = !hasInst && player.Gold >= 20;
        CanFortify   = isCamp && player.Gold >= 50;
        CanReinforce = hasInst && player.Gold >= 2;
        CanSplit     = army.Force >= 11;
        CanPass      = true;
    }

    public void AddLog(string message)
    {
        CombatLog = $"› {message}\n{CombatLog}";
        if (CombatLog.Length > 800)
            CombatLog = CombatLog[..800];
    }
}
