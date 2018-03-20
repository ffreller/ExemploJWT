using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PROJETOJWT.Models;
using PROJETOJWT.Repositorio;
using PROJETOJWT.Util;

namespace PROJETOJWT.Controllers
{
    [Route("api/[controller]")]
    public class LoginController:Controller
    {
       [AllowAnonymous]
       [HttpPost]
       public object Post(
           [FromBody]User usuario,
           [FromServices]UsersDAO usersDAO,
           [FromServices]SigningConfigurations signingConfigurations,
           [FromServices]TokenConfigurations tokenConfigurations){
               bool credencialsValidas = false;
               if(usuario != null && !String.IsNullOrWhiteSpace(usuario.UserID)){
                   var usuarioBase = usersDAO.Find(usuario.UserID);
                   credencialsValidas = (usuarioBase != null && usuario.UserID == usuarioBase.UserID && usuario.AccessKey == usuarioBase.AccessKey);
               }
               if(credencialsValidas){
                   ClaimsIdentity identity = new ClaimsIdentity(
                       new GenericIdentity(usuario.UserID,"Login"),
                       new[]{
                           new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString("N")),
                           new Claim(JwtRegisteredClaimNames.UniqueName,usuario.UserID)

                       }
                   );

                   DateTime dataCriacao = DateTime.Now;
                   DateTime dataExpiracao = dataCriacao+TimeSpan.FromSeconds(tokenConfigurations.Seconds);

                   var handler = new JwtSecurityTokenHandler();
                   var securityToken = handler.CreateToken(new SecurityTokenDescriptor{
                        Issuer = tokenConfigurations.Issuer,
                        Audience = tokenConfigurations.Audience,
                        SigningCredentials = signingConfigurations.SigningCredentials,
                        Subject = identity,
                        NotBefore = dataCriacao,
                        Expires = dataExpiracao
                   });
                   var token = handler.WriteToken(securityToken);

                   return new {
                       authenticated = true,
                       created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                       expriation = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                       accessToken = token,
                       message = "OK"
                   };
               }
               else{
                   return new{
                       Authenticated = false,
                       message = "Falha ao autenticar"

                   };
               }
           }
    
    }
}