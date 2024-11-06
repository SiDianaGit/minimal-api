
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class AdministradorTest
    {
        [TestMethod]
        public void TestarGetSetPropriedadesAdm()
        {
            // Arrange - variaveis
            var adm = new Administrador();

            // Act - valida propriedades Set
            adm.id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            // Assert - valida propriedades get
            Assert.AreEqual(1, adm.id);
            Assert.AreEqual("teste@teste.com", adm.Email);
            Assert.AreEqual("teste", adm.Senha);
            Assert.AreEqual("Adm", adm.Perfil);

            Assert.IsTrue(true);
            
        }

    }

}