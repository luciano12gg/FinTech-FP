using Moq;

namespace FinTech_FP.tests;

public class CuentaBancariaTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void RetirarEfectivo_CantidadMenorOIgualACero_LanzaArgumentException(decimal cantidad)
        {
            var mockAuditoria = new Mock<IServicioAuditoria>();
            var cuenta = new CuentaBancaria(1000m, mockAuditoria.Object);

            Assert.Throws<ArgumentException>(() => cuenta.RetirarEfectivo(cantidad));
            Assert.Equal(1000m, cuenta.Saldo);
        }

        [Theory]
        [InlineData(600.01)]
        [InlineData(700)]
        public void RetirarEfectivo_CantidadMayorA600_LanzaInvalidOperationException(decimal cantidad)
        {
            var mockAuditoria = new Mock<IServicioAuditoria>();
            var cuenta = new CuentaBancaria(1000m, mockAuditoria.Object);

            Assert.Throws<InvalidOperationException>(() => cuenta.RetirarEfectivo(cantidad));
            Assert.Equal(1000m, cuenta.Saldo);
        }

        [Theory]
        [InlineData(0.01, 1000, 999.99)]
        [InlineData(49.99, 1000, 950.01)]
        public void RetirarEfectivo_TramoMenorDe50_SinComision(decimal cantidad, decimal saldoInicial, decimal saldoEsperado)
        {
            var mockAuditoria = new Mock<IServicioAuditoria>();
            var cuenta = new CuentaBancaria(saldoInicial, mockAuditoria.Object);

            cuenta.RetirarEfectivo(cantidad);

            Assert.Equal(saldoEsperado, cuenta.Saldo);
            mockAuditoria.Verify(x => x.NotificarRetirada(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
        }

        [Theory]
        [InlineData(50, 1000, 949)]
        [InlineData(50.01, 1000, 948.99)]
        [InlineData(199.99, 1000, 799.01)]
        [InlineData(200, 1000, 799)]
        public void RetirarEfectivo_TramoEntre50y200_ComisionDe1Euro(decimal cantidad, decimal saldoInicial, decimal saldoEsperado)
        {
            var mockAuditoria = new Mock<IServicioAuditoria>();
            var cuenta = new CuentaBancaria(saldoInicial, mockAuditoria.Object);

            cuenta.RetirarEfectivo(cantidad);

            Assert.Equal(saldoEsperado, cuenta.Saldo);
            mockAuditoria.Verify(x => x.NotificarRetirada(cantidad, 1m), Times.Once);
        }

        [Theory]
        [InlineData(200.01, 1000, 796.99)]
        [InlineData(599.99, 1000, 397.01)]
        [InlineData(600, 1000, 397)]
        public void RetirarEfectivo_TramoMayorDe200_Hasta600_ComisionDe3Euros(decimal cantidad, decimal saldoInicial, decimal saldoEsperado)
        {
            var mockAuditoria = new Mock<IServicioAuditoria>();
            var cuenta = new CuentaBancaria(saldoInicial, mockAuditoria.Object);

            cuenta.RetirarEfectivo(cantidad);

            Assert.Equal(saldoEsperado, cuenta.Saldo);
            mockAuditoria.Verify(x => x.NotificarRetirada(cantidad, 3m), Times.Once);
        }

        [Theory]
        [InlineData(50, 50)]
        [InlineData(200, 200)]
        [InlineData(600, 600)]
        public void RetirarEfectivo_SaldoInsuficiente_LanzaInvalidOperationException(decimal cantidad, decimal saldoInicial)
        {
            var mockAuditoria = new Mock<IServicioAuditoria>();
            var cuenta = new CuentaBancaria(saldoInicial, mockAuditoria.Object);

            Assert.Throws<InvalidOperationException>(() => cuenta.RetirarEfectivo(cantidad));
            Assert.Equal(saldoInicial, cuenta.Saldo);
            mockAuditoria.Verify(x => x.NotificarRetirada(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
        }
    }