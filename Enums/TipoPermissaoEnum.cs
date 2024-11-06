namespace AspnetCoreMvcFull.Enums
{
  public enum TipoPermissaoEnum
  {
    Administrador,
    Gerente_Administrativo,
    Gerente_Operacional,
    Gerente_Comercial,
    SupervisorA,
    SupervisorO,
    SupervisorC,
    Analista_De_Sistemas,
    Suporte_Técnico,
    Financeiro,
    Comercial,
    Tecnico,
    Auxiliar_Tecnico,
    Tecnico_CFTV,
    AuxiliarTCFTV
  }
  public static class EnumExtensions
  {
    public static List<string> GetEnumValues<T>() where T : Enum
    {
      return Enum.GetNames(typeof(T)).ToList();
    }
    public static T? StringToEnum<T>(string value) where T : struct, Enum
    {
      if (string.IsNullOrEmpty(value))
      {
        return null; // Ou você pode lançar uma exceção
      }

      if (Enum.TryParse<T>(value, true, out var result))
      {
        return result; // Retorna o valor correspondente do enum
      }

      return null; // Ou você pode lançar uma exceção se não encontrar
    }
  }

}
