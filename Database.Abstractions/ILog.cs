using Database.Models;

namespace Database.Abstractions;

public interface ILog
{
    public long Id { get; set; }
    public LogType LogType { get; set; }
    public TimeSpan LogTime { get; set; }
    public string Description { get; set; }
}