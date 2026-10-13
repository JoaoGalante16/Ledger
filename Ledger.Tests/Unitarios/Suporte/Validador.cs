using System.ComponentModel.DataAnnotations;

namespace Ledger.Tests.Unitarios.Suporte;

public static class Validador
{
    // validacao que a Api faz antes de chegar no controller
    public static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }

    //validacao feita por propriedade
    public static bool TemErroEm(object objeto, string propriedade)
    {
        var valor = objeto.GetType().GetProperty(propriedade)!.GetValue(objeto);
        var contexto = new ValidationContext(objeto) { MemberName = propriedade };
        return !Validator.TryValidateProperty(valor, contexto, new List<ValidationResult>());
    }
}
