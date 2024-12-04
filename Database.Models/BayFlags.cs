namespace Database.Models;

[Flags]
public enum BayFlags
{
    DroppedOff = 1,
    Fetched = 2,
    PickedUp = 4,
}