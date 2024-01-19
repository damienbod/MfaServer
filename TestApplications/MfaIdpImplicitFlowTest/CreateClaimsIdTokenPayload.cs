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

        return System.Text.Json.JsonSerializer.Serialize(new id_token());
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
        public acr acr { get; set; }
        public acr amr { get; set; }
    }
}
