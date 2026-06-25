/// <summary>
/// Lists the available bonuses and the odds of each. This is the one place to
/// register a new bonus for spawning — the processor and the brick never change.
/// </summary>
public static class BonusCatalog
{
    public static IBonus Random()
    {
        float r = UnityEngine.Random.value;
        if (r < 0.34f)
        {
            return new BulletBonus();
        }
        if (r < 0.67f)
        {
            return new SpeedBoostBonus();
        }
        return new RepairBonus();
    }
}
