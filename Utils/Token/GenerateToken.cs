using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspnetCoreMvcFull.Utils.Token
{
    public class GenerateToken :IGenerateToken
    {
    public string GenerateJwtToken(string username)
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("fedaf7d8863b48e197b9287d492b708e"));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
     new Claim(JwtRegisteredClaimNames.Sub, username),
     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
   };

      var token = new JwtSecurityToken(
          issuer: "yourIssuer", // Certifique-se de que isso bate com a configuração de validação
          audience: "yourAudience", // Certifique-se de que isso bate com a configuração de validação
          claims: claims,
          expires: DateTime.Now.AddMinutes(60),
          signingCredentials: credentials);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
