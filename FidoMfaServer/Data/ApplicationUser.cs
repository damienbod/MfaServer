using Microsoft.AspNetCore.Identity;

namespace FidoMfaServer.Data;

public class ApplicationUser : IdentityUser 
{ 
    public string EntraIdOid { get; set; }
}
