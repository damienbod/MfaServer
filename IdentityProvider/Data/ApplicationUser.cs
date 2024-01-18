using Microsoft.AspNetCore.Identity;

namespace OpeniddictServer.Data;

public class ApplicationUser : IdentityUser 
{ 
    public string EntraIdOid { get; set; }
}
