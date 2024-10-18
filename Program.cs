
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApiCatalogo.ApiEndpoints;
using MinimalApiCatalogo.AppServicesExtensions;
using MinimalApiCatalogo.Context;
using MinimalApiCatalogo.Models;
using MinimalApiCatalogo.Services;
using System.Reflection;
using System.Text;

namespace MinimalApiCatalogo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddApiSwagger();
            builder.AddPersistence();
            builder.Services.AddCors();
            builder.AddAutenticationJwt();

            //------ Adicionado o servi�o de autoriza��o - Pra poder usar os recursos de autoriza��o e autentica��o --------
            builder.Services.AddAuthorization();

            var app = builder.Build();

            //Definir os endpoints
            //-------------------------- T E S T E ---------------------------
            app.MapGet("/", () => "Catalogo de Produtos - 2024").WithTags("Apresenta��o");

            //-------------------------- L O G I N ---------------------------
            app.MapAutenticacaoEndpoints();

            //-------------------------- C A T E G O R I A ---------------------------
            app.MapCategoriasEndpoints();

            //-------------------------- P R O D U T O ---------------------------
            app.MapProdutosEndpoints();

            // Configure the HTTP request pipeline.
            var environment = app.Environment;
            app.UseExceptionHandling(environment).UseSwaggerMiddleware().UseAppCors();

            //----------  N�o pode deixar de colocar esses caras aqui senao o autentica��o e a autoriza��o n�o v�o funcionar. ---------- 
            //---------- � importante tamb�m deixar na ordem que est� a�. ---------- 
            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
        }
    }
}
