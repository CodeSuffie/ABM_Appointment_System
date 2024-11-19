namespace Database.Models;

public enum BayStatus
{
    Closed,
    Free,
    Claimed,                // No work has started on the Trip yet
    DroppingOffStarted,
    WaitingFetchStart,      // If Dropping Off was finished sooner than the Fetch Started
    FetchStarted,           // Always after DropOffStarted
    FetchFinished,          // If Fetch was finished sooner than Dropping Off
    WaitingFetch,           // If Dropping Off was finished sooner than the Fetch
    PickUpStarted,
}