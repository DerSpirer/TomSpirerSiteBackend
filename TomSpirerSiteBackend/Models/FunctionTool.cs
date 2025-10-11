using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TomSpirerSiteBackend.Models;

public class FunctionTool
{
    public string name { get; set; }
    public string description { get; set; }
    public Type parameterType { get; set; }
}

public class LeaveMessageToolParams
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    [Description("The name of the sender")]
    public string fromName { get; set; }
        
    [Required]
    [EmailAddress]
    [Description("The email of the sender")]
    public string fromEmail { get; set; }
        
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    [Description("The subject of the message")]
    public string subject { get; set; }
        
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    [Description("The content of the message as plain text")]
    public string body { get; set; }
}