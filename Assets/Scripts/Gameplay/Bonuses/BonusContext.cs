/// <summary>
/// Everything a bonus might need when its effect fires. The processor fills in
/// whichever fields are relevant: <see cref="paddle"/> for a caught bonus,
/// <see cref="segment"/> for each segment a piercing bonus passes through.
/// </summary>
public struct BonusContext
{
    public Bonus bonus;
    public PaddleHealth paddle;
    public PaddleSegment segment;
}
