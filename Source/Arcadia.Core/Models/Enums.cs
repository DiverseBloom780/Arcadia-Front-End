namespace Arcadia.Core.Models
{
    public enum LaunchType
    {
        Standalone = 0,
        Emulator = 1,
        Steam = 2,
        GOG = 3,
        EpicGames = 4,
        TeknoParrot = 5
    }

    public enum GameCompletionStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Mastered = 3,
        Abandoned = 4
    }

    public enum TeknoParrotGameType
    {
        Other = 0,
        Racing = 1,
        Shooting = 2,
        Fighting = 3,
        Sports = 4
    }

    public enum WheelMode
    {
        Vertical,
        Horizontal,
        Curved,
        Angled
    }
}