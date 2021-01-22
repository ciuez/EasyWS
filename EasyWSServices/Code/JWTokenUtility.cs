using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EasyWSServices.Code
{
    public class JWTokenUtility
    {
        public static string GenerateToken(Int64 user_id, string username)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Sid, user_id.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddYears(1)).ToUnixTimeSeconds().ToString()),
            };

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(System.Text.UTF8Encoding.UTF8.GetBytes(Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("jwt_key").Value)), SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
