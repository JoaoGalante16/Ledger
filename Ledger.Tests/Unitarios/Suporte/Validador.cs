using System.ComponentModel.DataAnnotations;

namespace Ledger.Tests.Unitarios.Suporte;

public static class Validador
{
    public static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }

    public static bool TemErroEm(object objeto, string propriedade)
    {
        var valor = objeto.GetType().GetProperty(propriedade)!.GetValue(objeto);
        var contexto = new ValidationContext(objeto) { MemberName = propriedade };
        return !Validator.TryValidateProperty(valor, contexto, new List<ValidationResult>());
    }
}
