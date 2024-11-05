using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;
public class DBContexto : DbContext
{
    
    private readonly IConfiguration _configuracaoAppSettings;

    public DBContexto(IConfiguration configuracaoAppSettings)
    {
        _configuracaoAppSettings = configuracaoAppSettings;   
    }

    public DbSet<Administrador> Adminitradores {get; set;}
    public DbSet<Veiculo> Veiculos {get; set;}



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador {
                id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var stringConexao = _configuracaoAppSettings.GetConnectionString("ConexaoSQLServer")?.ToString();
        if(!string.IsNullOrEmpty(stringConexao))
        {
        optionsBuilder.UseSqlServer(stringConexao);
        }
    }

};
