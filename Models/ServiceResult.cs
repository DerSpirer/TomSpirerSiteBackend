namespace TomSpirerSiteBackend.Models;

public class ServiceResult<T>
{
    public bool success { get; set; }
    public string? message { get; set; }
    public T? data { get; set; }
}