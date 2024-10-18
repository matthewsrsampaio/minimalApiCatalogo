using Microsoft.AspNetCore.Authorization;
using MinimalApiCatalogo.Models;
using MinimalApiCatalogo.Services;
using System.Security.Cryptography.X509Certificates;

namespace MinimalApiCatalogo.ApiEndpoints
{
    public static class AutenticacaoEndpoints
    {

        public static void MapAutenticacaoEndpoints(this WebApplication app)
        {
            app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
            {
                if (userModel == null)
                    return Results.BadRequest();

                if (userModel.UserName == "matthews" && userModel.Password == "123456")
                {
                    var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"], app.Configuration["Jwt:Issuer"], app.Configuration["Jwt:Audience"], userModel);
                    return Results.Ok(new { token = tokenString });
                }
                else
                {
                    return Results.BadRequest("Login inválido");
                }
            }).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status200OK).WithName("Login").WithTags("Autenticacao");
        }
        
    }
}
