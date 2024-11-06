
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;


namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
    {
        private DBContexto CriarContextoTeste()
        {

            // Configurar o ConfigurationBuilder
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            // Obter a string de conexao
            //var connectionString = Configuration.GetConnectionString("ConexaoSQLServer");

            //var options = new DbContextOptionsBuilder<DBContexto>()
            //    .UseSqlServer(connectionString)
            //    .Options;
            //return new DBContexto((Microsoft.Extensions.Configuration.IConfiguration)options);
            return new DBContexto(configuration);

            //"Server=localhost\\SQLEXPRESS;Initial Catalog=Agenda_Test; Trusted_Connection=True; Integrated Security=True; TrustServerCertificate=True; User Id=SiDiana; Password=SiDiana;"
        }


        [TestMethod]
        public void TestSalvarAdm()
        {
            // Arrange - variaveis
            var context = CriarContextoTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE [Agenda_Test].[dbo].[Adminitradores]");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";
            
            var administradorServico = new AdministradorServico(context);

            // Act - valida propriedades Set
            administradorServico.Incluir(adm);

            // Assert - valida propriedades get
            Assert.AreEqual(1, administradorServico.Todos(1).Count());

        }
    }
}