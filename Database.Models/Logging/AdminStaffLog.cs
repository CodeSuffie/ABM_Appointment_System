using System.ComponentModel.DataAnnotations;
using Database.Abstractions;

namespace Database.Models.Logging;

public class AdminStaffLog : ILog
{
    public long Id { get; set; }
    public LogType LogType { get; set; }
    public TimeSpan LogTime { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = "";
    
    public AdminStaff? AdminStaff { get; set; }
    public long AdminStaffId { get; set; }
}
