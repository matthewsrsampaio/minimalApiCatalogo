
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

            //Add services to the container.//ConfigureServices
            builder.Services.AddEndpointsApiExplorer();
            //Adiciona um campo para autorização para o usuário colocar um o token
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCatalogo", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = @"JWT Authorization header using the Bearer scheme.
                                   Enter 'Bearer'[space]. Example: \'Bearer 12345abcdef\'",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                    },
                    new string[]{ }
                    }
                });
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            //Registro do serviço de token
            builder.Services.AddSingleton<ITokenService, TokenService>();

            //Adicionado o estilo de eutenticação
            builder.Services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            //Adicionado o serviço de autorização - Pra poder usar os recursos de autorização e autenticação
            builder.Services.AddAuthorization();

            var app = builder.Build();

            //Definir os endpoints
            //-------------------------- T E S T E ---------------------------
            app.MapGet("/", () => "Catalogo de Produtos - 2024").WithTags("Apresentação");

            //-------------------------- L O G I N ---------------------------
            app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
            {
                if(userModel == null)
                    return Results.BadRequest();

                if(userModel.UserName == "macoratti" && userModel.Password == "Numsey#123")
                {
                    var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"], app.Configuration["Jwt:Issuer"], app.Configuration["Jwt:Audience"], userModel);
                    return Results.Ok(new { token = tokenString});
                }
                else
                {
                    return Results.BadRequest("Login inválido");
                }
            }).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status200OK).WithName("Login").WithTags("Autenticacao");

            //-------------------------- C A T E G O R I A ---------------------------
            app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
            {
                db.Categorias.Add(categoria);
                await db.SaveChangesAsync();

                return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
            }).WithTags("Categorias");

            app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.ToListAsync()).WithTags("Categorias").RequireAuthorization();

            app.MapGet("/categorias/{id}", async(int id, AppDbContext db) =>
            {
                return await db.Categorias.FindAsync(id) is Categoria categoria ? Results.Ok(categoria) : Results.NotFound();
            }).WithTags("Categorias");

            app.MapPut("/categorias/{id}", async (int id, Categoria categoria, AppDbContext db) =>
            {
                if(categoria.CategoriaId != id)
                    return Results.BadRequest();

                var categoriaDb = await db.Categorias.FindAsync(id);

                if(categoriaDb is null)
                    return Results.NotFound();

                categoriaDb.Nome = categoria.Nome;
                categoriaDb.Descricao = categoria.Descricao;

                await db.SaveChangesAsync();
                return Results.Ok(categoriaDb);
            }).WithTags("Categorias");

            app.MapDelete("/categorias/{id}", async(int id, AppDbContext db) =>
            {
                var categoria = await db.Categorias.FindAsync(id);

                if (categoria is null)
                    return Results.NotFound();

                db.Categorias.Remove(categoria);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).WithTags("Categorias");

            //-------------------------- P R O D U T O ---------------------------

            app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
            {
                db.Produtos.Add(produto);
                await db.SaveChangesAsync();

                return Results.Created($"/produtos/{produto.ProdutoId}", produto);
            }).WithTags("Produtos");

            app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos.ToListAsync()).WithTags("Produtos").RequireAuthorization();

            app.MapGet("/produtos/{id}", async (int id, AppDbContext db) =>
            {
                return await db.Produtos.FindAsync(id) is Produto produto ? Results.Ok(produto) : Results.NotFound();
            }).WithTags("Produtos");

            app.MapPut("/produtos/{id}", async (int id, Produto produto, AppDbContext db) =>
            {
                if (produto.ProdutoId != id)
                    return Results.BadRequest();

                var produtoDb = await db.Produtos.FindAsync(id);

                if (produtoDb is null)
                    return Results.NotFound();

                produtoDb.Nome = produto.Nome;
                produtoDb.Descricao = produto.Descricao;
                produtoDb.Preco = produto.Preco;
                produtoDb.Imagem = produto.Imagem;
                produtoDb.DataCompra = produto.DataCompra;
                produtoDb.Estoque = produto.Estoque;
                produtoDb.CategoriaId = produto.CategoriaId;

                await db.SaveChangesAsync();

                return Results.Ok(produtoDb);
            }).WithTags("Produtos");

            app.MapDelete("/produtos/{id}", async (int id, AppDbContext db) =>
            {
                var produto = await db.Produtos.FindAsync(id);

                if (produto is null)
                    return Results.NotFound();

                db.Produtos.Remove(produto);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).WithTags("Produtos");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            };

            //Não pode deixar de colocar esses caras aqui senao o autenticação e a autorização não vão funcionar. É importante também deixar na ordem que está aí.
            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
        }
    }
}
