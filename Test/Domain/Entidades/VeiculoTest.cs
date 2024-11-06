using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTest
    {
        [TestMethod]
        public void TestarGetSetPropriedadesVeiculo()
        {
            // Arrange - variaveis
            var veiculo  = new Veiculo();

            // Act - valida propriedades Set
            veiculo.id = 1;
            veiculo.Nome = "teste";
            veiculo.Marca = "teste";
            veiculo.Ano = 2000;

            // Assert - valida propriedades get
            Assert.AreEqual(1, veiculo.id);
            Assert.AreEqual("teste", veiculo.Nome);
            Assert.AreEqual("teste", veiculo.Marca);
            Assert.AreEqual(2000, veiculo.Ano);

            Assert.IsTrue(true);
            
        }
    }
}