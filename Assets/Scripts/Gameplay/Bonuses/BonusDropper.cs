using System.Collections.Generic;

/// <summary>
/// Decides which bonuses drop when a brick dies. Each bonus rolls its own
/// independent chance. Bonuses are split into two groups (bullets, and the rest);
/// at most one bonus from a group drops per event, so a bullet and a power-up can
/// drop together but two bullets can't. If more than one in a group wins at once,
/// a random one drops now and the others are owed a guaranteed drop next time.
/// </summary>
public static class BonusDropper
{
    private static readonly System.Func<IBonus>[] Bullets =
    {
        () => new BulletBonus(),
        () => new SlowBulletBonus(),
    };

    private static readonly System.Func<IBonus>[] Powerups =
    {
        () => new SpeedBoostBonus(),
        () => new RepairBonus(),
    };

    // Indices within each group that are owed a guaranteed drop next roll.
    private static readonly HashSet<int> _owedBullets = new HashSet<int>();
    private static readonly HashSet<int> _owedPowerups = new HashSet<int>();

    /// <summary>Rolls both groups and returns the bonuses to spawn now (0, 1 or 2).</summary>
    public static List<IBonus> Roll()
    {
        List<IBonus> drops = new List<IBonus>();

        IBonus bullet = RollGroup(Bullets, _owedBullets);
        if (bullet != null)
        {
            drops.Add(bullet);
        }

        IBonus powerup = RollGroup(Powerups, _owedPowerups);
        if (powerup != null)
        {
            drops.Add(powerup);
        }

        return drops;
    }

    static IBonus RollGroup(System.Func<IBonus>[] group, HashSet<int> owed)
    {
        IBonus[] candidates = new IBonus[group.Length];
        List<int> winners = new List<int>();
        for (int i = 0; i < group.Length; i++)
        {
            candidates[i] = group[i]();
            // A bonus owed from last time is guaranteed; otherwise roll its chance.
            if (owed.Contains(i) || UnityEngine.Random.value < candidates[i].DropChance)
            {
                winners.Add(i);
            }
        }

        owed.Clear();
        if (winners.Count == 0)
        {
            return null;
        }

        int chosen = winners[UnityEngine.Random.Range(0, winners.Count)];
        foreach (int i in winners)
        {
            if (i != chosen)
            {
                owed.Add(i);
            }
        }
        return candidates[chosen];
    }
}
