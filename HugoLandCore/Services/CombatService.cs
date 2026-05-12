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

    public void SetArmyService(ArmyService armySvc) => armyService = armySvc;

    // Appelé depuis TryMove avec old_territory_id connu
    public void ResolveCombat(MilitaryDetachment attacker, MilitaryDetachment defender,
                              int direction, int attackerCameFromId)
    {
        int attForce = attacker.Force;
        int defForce = defender.Force;
        int defTerritoryId = defender.TerritoryId!.Value;

        // Facteurs aléatoires
        float factorAtt = _rnd.Next(8, 13) / 10f;
        float factorDef = _rnd.Next(9, 14) / 10f;
        float effAtt = attForce * factorAtt;
        float effDef = defForce * factorDef * defender.Territory!.TerrainBonus;
        if (defender.Territory.Installation != null)
            effDef *= defender.Territory.Installation.InstallationBonus;

        bool attackerWins = effAtt > effDef;

        // Pertes
        if (attackerWins)
        {
            attacker.Force -= (int)Math.Floor(attForce * 0.20);
            defender.Force -= (int)Math.Floor(defForce * 0.60);
            attacker.Player!.Gold += (defForce / 10) * 5;
        }
        else
        {
            attacker.Force -= (int)Math.Floor(attForce * 0.60);
            defender.Force -= (int)Math.Floor(defForce * 0.20);
            defender.Player!.Gold += (attForce / 10) * 5;
        }

        attacker.Energy = 0;
        defender.Energy = 0;

        // Enregistrer le combat
        _context.CombatEvents.Add(new HugoLandCore.Models.CombatEvent
        {
            AttackerPlayerId = attacker.PlayerId,
            DefenderPlayerId = defender.PlayerId,
            TerritoryId = defTerritoryId,
            GameId = attacker.Player!.GameId,
            Timestamp = DateTime.Now,
            Result = attackerWins
                ? $"Attacker wins. Att:{attForce}->{attacker.Force} Def:{defForce}->{defender.Force}"
                : $"Defender wins. Att:{attForce}->{attacker.Force} Def:{defForce}->{defender.Force}"
        });

        if (attackerWins)
        {
            // Attaquant gagne — il reste sur la case
            // Défenseur doit reculer DEPUIS defTerritoryId
            if (defender.Force < 10)
            {
                // Dissolution du défenseur
                var defTer = _context.Territories.Find(defTerritoryId);
                if (defTer != null) defTer.Detachment = null;
                defender.TerritoryId = null;
                defender.Territory = null;
                _context.Detachments.Remove(defender);
            }
            else
            {
                // Retraite du défenseur vers une case adjacente libre
                RetreatFrom(defender, defTerritoryId, excludeId: attackerCameFromId);
            }
        }
        else
        {
            // Défenseur gagne — attaquant retourne sur sa case d'origine
            // Remettre la case du défenseur à lui
            var defTer2 = _context.Territories.Find(defTerritoryId);
            if (defTer2 != null) defTer2.Detachment = defender;
            defender.Territory = defTer2;
            defender.TerritoryId = defTerritoryId;

            if (attacker.Force < 10)
            {
                // Dissolution de l'attaquant
                // Sa case actuelle (defTerritoryId) appartient au défenseur — l'attaquant n'y est plus
                attacker.TerritoryId = null;
                attacker.Territory = null;
                _context.Detachments.Remove(attacker);
            }
            else
            {
                // Retour de l'attaquant sur sa case d'origine
                var camFrom = _context.Territories.Find(attackerCameFromId);
                if (camFrom != null)
                {
                    // Vider la case cible pour l'attaquant
                    if (camFrom.Detachment != null && camFrom.Detachment.PlayerId == attacker.PlayerId)
                    {
                        // Allié présent — fusionner
                        attacker.TerritoryId = attackerCameFromId;
                        attacker.Territory = camFrom;
                        armyService?.TryMerge(attacker);
                    }
                    else
                    {
                        camFrom.Detachment = attacker;
                        attacker.TerritoryId = attackerCameFromId;
                        attacker.Territory = camFrom;
                    }
                }
                else
                {
                    // Case d'origine introuvable → dissolution
                    attacker.TerritoryId = null;
                    attacker.Territory = null;
                    _context.Detachments.Remove(attacker);
                }
            }
        }

        _context.SaveChanges();
    }

    // Surcharge pour TrySplit (sans old_territory_id explicite)
    public void ResolveCombat(MilitaryDetachment attacker, MilitaryDetachment defender, int direction)
    {
        int width = attacker.Player!.Game!.Map!.Width;
        int cameFrom = direction switch
        {
            0 => attacker.TerritoryId!.Value + width,
            1 => attacker.TerritoryId!.Value - 1,
            2 => attacker.TerritoryId!.Value - width,
            3 => attacker.TerritoryId!.Value + 1,
            _ => attacker.TerritoryId!.Value
        };
        ResolveCombat(attacker, defender, direction, cameFrom);
    }

    private void RetreatFrom(MilitaryDetachment loser, int fromId, int excludeId)
    {
        int width = loser.Player!.Game!.Map!.Width;
        int[] dirs = { fromId - width, fromId + 1, fromId + width, fromId - 1 };
        dirs = dirs.OrderBy(_ => _rnd.Next()).ToArray();

        // Libérer la case actuelle
        var curTer = _context.Territories.Find(fromId);
        if (curTer != null) curTer.Detachment = null;

        bool retreated = false;
        foreach (int cid in dirs)
        {
            if (cid == excludeId) continue;
            var t = _context.Territories.Find(cid);
            if (t == null) continue;
            if (t.Terrain == TerrainType.Mer) continue;
            if (t.Detachment != null && t.Detachment.PlayerId != loser.PlayerId) continue;

            loser.Territory = t;
            loser.TerritoryId = cid;

            if (t.Detachment != null && t.Detachment.PlayerId == loser.PlayerId)
                armyService?.TryMerge(loser);
            else
                t.Detachment = loser;

            retreated = true;
            break;
        }

        if (!retreated)
        {
            loser.TerritoryId = null;
            loser.Territory = null;
            _context.Detachments.Remove(loser);
        }
    }
}
