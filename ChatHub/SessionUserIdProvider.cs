using Microsoft.AspNetCore.SignalR;

public class SessionUserIdProvider : IUserIdProvider
{
  public string GetUserId(HubConnectionContext connection)
  {
    // Extrai o ID da sessão
    return connection.GetHttpContext().Session.GetString("FuncionarioId");
  }
}
