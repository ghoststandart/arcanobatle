/// <summary>
/// Shared flag the loading screen uses to hold gameplay until the scene is ready.
/// While false, the ball stays parked and the score is hidden, so the game only
/// becomes visible/active once the loading overlay reveals it. Defaults true so a
/// game scene played directly (no overlay) starts normally.
/// </summary>
public static class GameBoot
{
    public static bool Ready = true;
}
