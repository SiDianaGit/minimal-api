using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enums;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

public class Startup
{

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "123456";   
    }

    private string key = "";
    public IConfiguration Configuration {get; set;} = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        //var builder = WebApplication.CreateBuilder(args);

        //Inicio configuracao JWT

        services.AddAuthentication(option => {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option => {
            option.TokenValidationParameters = new TokenValidationParameters{
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
        //fim configuracao autenticacao TokenJWT
        services.AddAuthorization();


        services.AddScoped<iAdministradorServico, AdministradorServico>();
        services.AddScoped<iVeiculoServico, VeiculoServico>();
        services.AddEndpointsApiExplorer();
        //configuracao de swagger 
        services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
                Name = "Authorization", //passando o token no header de cada endpoint
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer" 
                        }
                    },
                    new string[] {}
                }
            });
        });


        services.AddDbContext<DBContexto>(options =>  { 
                options.UseSqlServer(Configuration.GetConnectionString("ConexaoSQLServer"));//connectionString
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();


        app.UseEndpoints(endpoints => {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion


            #region Administradores
            string GerarTokenJwt(Administrador administrador){
                if(string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // faz a criptografia

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            endpoints.MapPost("/administrador/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) => {
                var adm = administradorServico.Login(loginDTO);
                if(adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministradorLogado{
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Adminitradores");

            endpoints.MapPost("/administrador", ([FromBody] AdministradorDTO administradorDTO, iAdministradorServico administradorServico) => {
                var validacao = new ErrosDeValidacao{
                        Mensagens = new List<string>()
                };

                if(string.IsNullOrEmpty(administradorDTO.Email))  validacao.Mensagens.Add("O Email não pode ser vazio.");
                if(string.IsNullOrEmpty(administradorDTO.Senha))  validacao.Mensagens.Add("A senha deve ser informadao.");
                if(administradorDTO.Perfil == null)  validacao.Mensagens.Add("O Perfil não pode ser vazio.");

                if(validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

                var administrador = new Administrador{
                    Email = administradorDTO.Email,
                    Senha = administradorDTO.Senha,
                    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                };
            #pragma warning restore CS8601 // Possible null reference assignment.
                administradorServico.Incluir(administrador);

                return Results.Created($"/administrador/{administrador.id}", administrador);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Adminitradores");

            endpoints.MapGet("/Adminitrador", ([FromQuery] int? pagina, iAdministradorServico administradorServico) => 
            {
                var adms = new List<AdministradorModelView>();
                var administrador = administradorServico.Todos(pagina);
                foreach(var adm in administrador)
                {
                    adms.Add(new AdministradorModelView
                    {
                        Id = adm.id,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }
                return Results.Ok(adms);
            }).RequireAuthorization().WithTags("Adminitradores");

            endpoints.MapGet("/Adminitrador/{id}", ([FromRoute] int id, iAdministradorServico administradorServico) => 
            {
                var administrador = administradorServico.BuscaPorId(id);
                if(administrador == null) return Results.NotFound();

                var adms = new List<AdministradorModelView>();
                adms.Add(new AdministradorModelView
                    {
                        Id = administrador.id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });
                return Results.Ok(adms);
                //return Results.Ok(administrador);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Adminitradores");

            #endregion


            #region Veiculos
            ErrosDeValidacao ValidaDTO(VeiculoDTO veiculoDTO)
            {
                var validacao = new ErrosDeValidacao{
                    Mensagens = new List<string>()
                };

                if(string.IsNullOrEmpty(veiculoDTO.Nome))  validacao.Mensagens.Add("O Nome não pode ser vazio.");
                if(string.IsNullOrEmpty(veiculoDTO.Marca))  validacao.Mensagens.Add("A Marca deve ser informada.");
                if(veiculoDTO.Ano < 1950)  validacao.Mensagens.Add("Permitido somente Ano superior a 1950.");

                return validacao;
                
            }

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) => 
            {
                var validacao = ValidaDTO(veiculoDTO);
                if(validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

                var veiculo = new Veiculo{
                    Nome = veiculoDTO.Nome,
                    Marca = veiculoDTO.Marca,
                    Ano = veiculoDTO.Ano
                };
                veiculoServico.Incluir(veiculo);

                return Results.Created($"/veiculo/{veiculo.id}", veiculo);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm,Editor"})
            .WithTags("Veiculos");

            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, iVeiculoServico veiculoServico) => 
            {
                var veiculo = veiculoServico.Todos(pagina);

                return Results.Ok(veiculo);
            }).RequireAuthorization().WithTags("Veiculos");

            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) => 
            {
                var veiculo = veiculoServico.BuscaPorId(id);

                if(veiculo == null) return Results.NotFound();

                return Results.Ok(veiculo);
            }).RequireAuthorization().WithTags("Veiculos");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) => 
            {
            
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();

                var validacao = ValidaDTO(veiculoDTO);
                if(validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);
                
                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Marca = veiculoDTO.Marca;
                veiculo.Ano = veiculoDTO.Ano;
                
                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);

            }).RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Veiculos");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) => 
            {
                
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();
                
                veiculoServico.Apagar(veiculo);

                return Results.NoContent();

            }).RequireAuthorization().WithTags("Veiculos");
            #endregion

        });
    }

}
