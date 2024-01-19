using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MfaIdpImplicitFlowTest;

public static class CreateClaimsIdTokenPayload
{
    public static string GenerateClaims()
    {

        //{
        //    "id_token": {
        //        "acr": {
        //            "essential": true,
        //            "values":["possessionorinherence"]
        //        },
        //        "amr": {
        //            "essential": true,
        //        }
        //    }
        //}

        return System.Text.Json.JsonSerializer.Serialize(new claims());
    }

    public class acr
    {
        public bool essential { get; set; } = true;
        public string[] values { get; set; } = ["possessionorinherence"];
    }

    public class amr
    {
        public bool essential { get; set; } = true;
    }

    public class id_token
    {
        public acr acr { get; set; } = new acr();
        public amr amr { get; set; } = new amr();
    }

    public class claims
    {
        public id_token id_token { get; set; } = new id_token();
    }
}
