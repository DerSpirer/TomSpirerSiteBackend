using System.ComponentModel.DataAnnotations;

namespace TomSpirerSiteBackend.Models.DTOs;

public class GenerateResponseRequest
{
    [Required]
    [MinLength(1)]
    public List<Message> messages { get; set; }
}