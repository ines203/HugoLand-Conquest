using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;

namespace HugoLand.Core.Services;

public class ArmyService
{
    private readonly HugoLandContext _context;
    private CombatService combatService;
    private InstallationService installationService;

    public ArmyService(HugoLandContext context, CombatService combat, InstallationService installation)
    {
        _context = context;
        combatService = combat;
        installationService = installation;
    }

    public void TryMove(int armyId, int direction)
    {
        var army = _context.Detachments.Find(armyId);
        if (army == null || army.Force < 10) return;

        int width = army.Player!.Game!.Map!.Width;
        int old_territory_id = army.TerritoryId!.Value;

        int new_territory_id = direction switch
        {
            0 => old_territory_id - width, // North
            1 => old_territory_id + 1, // East
            2 => old_territory_id + width, // South
            3 => old_territory_id - 1, // West
            _ => old_territory_id // Nothing
        };

        var new_territory = _context.Territories.Find(new_territory_id);
        if (new_territory == null) return;

        if (army.Energy < new_territory.TerrainEnergyCost) return;

        var old_territory = army.Territory;

        // Chercher ennemi et allié directement dans la BD
        var enemyOnTarget = _context.Detachments
            .FirstOrDefault(d => d.TerritoryId == new_territory_id
                              && d.PlayerId != army.PlayerId);

        var allyOnTarget = _context.Detachments
            .FirstOrDefault(d => d.TerritoryId == new_territory_id
                              && d.PlayerId == army.PlayerId
                              && d.Id != army.Id);

        // Déduire l'énergie et marquer comme ayant agi
        army.Energy -= new_territory.TerrainEnergyCost;
        army.Actionable = false;

        // Détacher l'ancienne case
        if (army.Territory != null)
            army.Territory.Detachment = null;

        // Placer sur la nouvelle case
        army.Territory = new_territory;
        army.TerritoryId = new_territory_id;
        new_territory.Detachment = army;

        // Détruire camp si quitté
        if (old_territory != null)
            installationService.CheckDestructionCamp(old_territory.Installation);

        if (enemyOnTarget != null)
        {
            // COMBAT — CombatService gère la retraite
            combatService.ResolveCombat(army, enemyOnTarget, direction, old_territory_id);
            return;
        }

        // Fusion avec allié
        if (allyOnTarget != null)
            TryMerge(army);

        // Capture fortification ennemie non défendue
        var enemyFort = _context.Installations
            .FirstOrDefault(i => i.TerritoryId == new_territory_id
                              && i.PlayerId != army.PlayerId);
        if (enemyFort != null)
        {
            enemyFort.PlayerId = army.PlayerId;
            enemyFort.IsOccupied = true;
        }

        // Historique
        _context.PlayerActions.Add(new HugoLandCore.Models.PlayerAction
        {
            Description = $"Move army from ({old_territory_id}) to ({new_territory_id})",
            Timestamp = DateTime.Now,
            PlayerId = army.PlayerId
        });

        _context.SaveChanges();
    }

    public void TrySplit(int armyId, int direction, int split_qty)
    {
        var army = _context.Detachments.Find(armyId);

        if (army == null) return;
        if (army.Force < 11) return;
        if (split_qty < 10) return;
        if (army.Force - split_qty < 1) return;

        int width = army.Player!.Game!.Map!.Width;
        int new_territory_id = direction switch
        {
            0 => army.TerritoryId!.Value - width,
            1 => army.TerritoryId!.Value + 1,
            2 => army.TerritoryId!.Value + width,
            3 => army.TerritoryId!.Value - 1,
            _ => army.TerritoryId!.Value
        };

        var new_territory = _context.Territories.Find(new_territory_id);
        if (new_territory == null) return;

        if (army.Energy < new_territory.TerrainEnergyCost) return;

        army.Force -= split_qty;

        var new_detachment = MilitaryDetachment.Create(new_territory_id, split_qty, army.Energy, false);
        new_detachment.PlayerId = army.PlayerId;
        new_detachment.Player = army.Player;
        new_detachment.Territory = new_territory;
        new_detachment.Energy -= new_territory.TerrainEnergyCost;

        new_territory.Detachment = new_detachment;
        _context.Detachments.Add(new_detachment);
        army.Actionable = false;

        // Historique split
        _context.PlayerActions.Add(new HugoLandCore.Models.PlayerAction
        {
            Description = $"Split army: {split_qty} troops sent to ({new_territory_id})",
            Timestamp = DateTime.Now,
            PlayerId = army.PlayerId
        });

        _context.SaveChanges();

        // Fusion si allié présent
        var allyOnTarget = _context.Detachments
            .FirstOrDefault(d => d.TerritoryId == new_territory_id
                              && d.PlayerId == army.PlayerId
                              && d.Id != new_detachment.Id);
        if (allyOnTarget != null)
            TryMerge(new_detachment);

        // Combat si ennemi présent
        var enemyOnTarget = _context.Detachments
            .FirstOrDefault(d => d.TerritoryId == new_territory_id
                              && d.PlayerId != army.PlayerId);
        if (enemyOnTarget != null)
            combatService.ResolveCombat(new_detachment, enemyOnTarget, direction);
    }

    public void GainEnergy(int armyId)
    {
        var army = _context.Detachments.Find(armyId);
        if (army == null) return;

        if (army.Force >= 10)
        {
            army.Actionable = true;
            army.Energy = Math.Min(5, army.Energy + 2);
        }
        else
        {
            army.Energy = 5;
            army.Actionable = false;
        }

        _context.SaveChanges();
    }

    public void TryMerge(MilitaryDetachment army)
    {
        var allies = _context.Detachments
            .Where(d => d.TerritoryId == army.TerritoryId && d.Id != army.Id
                     && d.PlayerId == army.PlayerId)
            .ToList();

        foreach (var target in allies)
        {
            army.Force += target.Force;
            army.Energy = Math.Min(army.Energy, target.Energy);

            if (target.Territory != null)
                target.Territory.Detachment = null;

            target.TerritoryId = null;
            _context.Detachments.Remove(target);
        }

        _context.SaveChanges();
    }

    public void ReinforceArmy(Player player, MilitaryDetachment army, int amount)
    {
        int cost = amount * 2;

        if (army.Force < 10) return;
        if (army.Territory?.Installation == null) return;
        if (player.Gold < cost) return;

        army.Force += amount;
        player.Gold -= cost;
        army.Actionable = false;

        _context.PlayerActions.Add(new HugoLandCore.Models.PlayerAction
        {
            Description = $"Reinforce army at ({army.Territory?.X},{army.Territory?.Y}): +{amount} troops, -{cost} gold",
            Timestamp = DateTime.Now,
            PlayerId = player.Id
        });

        _context.SaveChanges();
    }
}
