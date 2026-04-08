using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;

namespace HugoLand.Core.Services;

public class CombatService
{
    private readonly HugoLandContext _context;
    private ArmyService? armyService;
    private InstallationService installationService;
    private Random _rnd = new Random();

    public CombatService(HugoLandContext context, InstallationService insta)
    {
        _context = context;
        installationService = insta;
    }

    public void SetArmyService(ArmyService armySvc)
    {
        armyService = armySvc;
    }

    /// <summary>
    /// Résout un combat entre l'attaquant et le défenseur.
    /// Applique les pertes, le pillage, et gère la retraite.
    /// </summary>
    public void ResolveCombat(MilitaryDetachment army_self, MilitaryDetachment army_enemy, int direction)
    {
        // Forces avant combat (pour le calcul des pertes et du pillage)
        int attForceAvant = army_self.Force;
        int defForceAvant = army_enemy.Force;

        // Facteurs aléatoires : attaquant 0.8-1.2, défenseur 0.9-1.3
        float factorAtt = _rnd.Next(8, 13) / 10f;
        float factorDef = _rnd.Next(9, 14) / 10f;

        float eff_force_self = attForceAvant * factorAtt;
        float eff_force_enemy = defForceAvant * factorDef
            * army_enemy.Territory!.TerrainBonus;

        if (army_enemy.Territory.Installation != null)
            eff_force_enemy *= army_enemy.Territory.Installation.InstallationBonus;

        bool attackerWins = eff_force_self > eff_force_enemy;

        // Pertes calculées sur la force AVANT combat (règle du TP)
        if (attackerWins)
        {
            army_self.Force -= (int)Math.Floor(attForceAvant * 0.20);
            army_enemy.Force -= (int)Math.Floor(defForceAvant * 0.60);
        }
        else
        {
            army_self.Force -= (int)Math.Floor(attForceAvant * 0.60);
            army_enemy.Force -= (int)Math.Floor(defForceAvant * 0.20);
        }

        // Épuisement post-combat
        army_self.Energy = 0;
        army_enemy.Energy = 0;

        // Pillage : 5 or par tranche de 10 soldats (force avant combat du perdant)
        if (attackerWins)
        {
            int pillageTranches = defForceAvant / 10;
            army_self.Player!.Gold += pillageTranches * 5;

            TryRetreat(army_self, army_enemy, attackerWins, direction);
        }
        else
        {
            int pillageTranches = attForceAvant / 10;
            army_enemy.Player!.Gold += pillageTranches * 5;

            TryRetreat(army_enemy, army_self, attackerWins, direction);
        }

        _context.SaveChanges();
    }

    private void TryRetreat(MilitaryDetachment winner, MilitaryDetachment loser, bool attackerWins, int direction)
    {
        if (armyService == null)
            return;

        if (loser.TerritoryId == null || loser.Territory == null)
            return;

        // Perdant avec < 10 soldats → dissolution
        if (loser.Force < 10)
        {
            // Détruire le camp si présent
            if (loser.Territory.Installation?.Type == InstallationType.Camp)
                installationService.CheckDestructionCamp(loser.Territory.Installation);

            // Détacher du territoire avant suppression
            if (loser.Territory != null)
                loser.Territory.Detachment = null;
            loser.TerritoryId = null;

            _context.Detachments.Remove(loser);
            return;
        }

        // Retraite : l'attaquant recule vers son territoire d'origine
        // le défenseur recule vers un territoire adjacent sans ennemi
        List<int> directions = new List<int> { 0, 1, 2, 3 };
        directions = directions.OrderBy(x => _rnd.Next()).ToList();

        bool retreated = false;

        if (attackerWins)
        {
            // Attaquant gagne → perdant (défenseur) recule vers un territoire adjacent aléatoire
            foreach (int dir in directions)
            {
                int retreatTerritoryId = GetAdjacentTerritoryId(loser, dir);
                var retreatTerritory = _context.Territories.Find(retreatTerritoryId);

                if (retreatTerritory == null)
                    continue;

                // Pas d'ennemi sur ce territoire
                if (retreatTerritory.Detachment != null
                    && retreatTerritory.Detachment.PlayerId != loser.PlayerId)
                    continue;

                // Déplacer le perdant
                if (loser.Territory != null)
                    loser.Territory.Detachment = null;

                loser.Territory = retreatTerritory;
                loser.TerritoryId = retreatTerritoryId;
                retreatTerritory.Detachment = loser;

                // Fusion si allié présent
                armyService.TryMerge(loser);

                retreated = true;
                break;
            }
        }
        else
        {
            // Défenseur gagne → perdant (attaquant) recule vers son territoire d'origine
            int originalDir = direction switch
            {
                0 => 2,
                1 => 3,
                2 => 0,
                3 => 1,
                _ => 2
            };

            int retreatTerritoryId = GetAdjacentTerritoryId(loser, originalDir);
            var retreatTerritory = _context.Territories.Find(retreatTerritoryId);

            if (retreatTerritory != null)
            {
                if (loser.Territory != null)
                    loser.Territory.Detachment = null;

                loser.Territory = retreatTerritory;
                loser.TerritoryId = retreatTerritoryId;
                retreatTerritory.Detachment = loser;

                // Fusion si allié présent
                armyService.TryMerge(loser);

                retreated = true;
            }
        }

        if (!retreated)
        {
            // Aucun territoire disponible → destruction
            if (loser.Territory != null)
                loser.Territory.Detachment = null;
            loser.TerritoryId = null;
            _context.Detachments.Remove(loser);
        }
    }

    private int GetAdjacentTerritoryId(MilitaryDetachment detachment, int direction)
    {
        int width = detachment.Player!.Game!.Map!.Width;
        return direction switch
        {
            0 => detachment.TerritoryId!.Value - width, // N
            1 => detachment.TerritoryId!.Value + 1,     // E
            2 => detachment.TerritoryId!.Value + width, // S
            3 => detachment.TerritoryId!.Value - 1,     // O
            _ => detachment.TerritoryId!.Value
        };
    }
}