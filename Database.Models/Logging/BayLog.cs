using System.ComponentModel.DataAnnotations;
using Database.Abstractions;

namespace Database.Models.Logging;

public class BayLog : ILog
{
    public long Id { get; set; }
    public LogType LogType { get; set; }
    public TimeSpan LogTime { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = "";
    
    public Bay? Bay { get; set; }
    public long BayId { get; set; }
}