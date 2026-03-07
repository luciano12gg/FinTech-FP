namespace FinTech_FP;

public class CuentaBancaria
{

    private readonly IServicioAuditoria _servicioAuditoria;

    private decimal _saldo;

    public decimal Saldo
    {
        get => _saldo;
        set
        {
            if (value < 0) throw new ArgumentException("el saldo no puede ser menor a 0");
            _saldo = value;
        }
    }

    public CuentaBancaria(decimal saldo,  IServicioAuditoria servicioAuditoria)
    {
        _servicioAuditoria = servicioAuditoria ?? throw new ArgumentNullException(nameof(servicioAuditoria));
        Saldo = saldo;
        
    }

    public void RetirarEfectivo(decimal cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("no se puede retirar cantidades negativas");
        if (cantidad > 600) throw new InvalidOperationException("no puedes retirar mas de 600 euros");

        decimal comision = cantidad switch
        {
            < 50 => 0m,
            <= 200 => 1m,
            _ => 3m
        };

        decimal total = cantidad + comision;

        if (Saldo < total) throw new InvalidOperationException("no tienes suficiente dinero en la cuenta");

        _saldo -= total;
        
        if (comision > 0) _servicioAuditoria.NotificarRetirada(cantidad, comision);

    }
}