using System.ComponentModel.DataAnnotations;
using Database.Abstractions;

namespace Database.Models.Logging;

public class BayStaffLog : ILog
{
    public long Id { get; set; }
    public LogType LogType { get; set; }
    public TimeSpan LogTime { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = "";
    
    public BayStaff? BayStaff { get; set; }
    public long BayStaffId { get; set; }
}
