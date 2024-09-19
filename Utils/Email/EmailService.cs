using AspnetCoreMvcFull.Enums;
using System.Net;
using System.Net.Mail;

namespace AspnetCoreMvcFull.Utils.Email
{
  public class EmailService : IEmailService
  {

    public void EnviarEmail(string requisitanteNome, string requisicao, string requisitanteEmail, string relato, TipoStatusEnum status)
    {
      string smtpServer = "mail.ff.tv.br";//Environment.GetEnvironmentVariable("SMTP_Server");
      int smtpPort = 587;
      string senderEmail = "lpr.pdl@ffcomunicacoes.net.br";//Environment.GetEnvironmentVariable("EMAIL_USER");
      string senderPassword = "Pdl22172510!@";//Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

      string subject = GerarAssuntoEmail(status, relato);
      string body = GerarCorpoEmail(status, requisitanteNome, requisicao);

      try
      {
        SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
          Credentials = new NetworkCredential(senderEmail, senderPassword),
          EnableSsl = true
        };

        MailMessage mailMessage = new MailMessage(senderEmail, requisitanteEmail, subject, body);
        smtpClient.Send(mailMessage);
        Console.WriteLine("E-mail enviado com sucesso!");
      }
      catch (Exception ex)
      {
        Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
      }
    }

    private string GerarAssuntoEmail(TipoStatusEnum status, string relato)
    {
      return status switch
      {
        TipoStatusEnum.Pendente => $"Requisição Pendente - {relato}",
        TipoStatusEnum.Andamento => $"Requisição em Andamento - {relato}",
        TipoStatusEnum.Aguardando => $"Requisição Aguardando- {relato}",
        TipoStatusEnum.Cancelado => $"Requisição Cancelada - {relato}",
        TipoStatusEnum.Concluido => $"Requisição Concluída - {relato}",
        _ => "Status de Requisição Não Definido"
      };
    }

    private string GerarCorpoEmail(TipoStatusEnum status, string requisitanteNome, string requisicaoNome)
    {
      return status switch
      {
        TipoStatusEnum.Pendente =>
            $"Prezado(a) {requisitanteNome},\n\nA sua requisição '{requisicaoNome}' está pendente.\n\nAtenciosamente,\nGrupo F&F",

        TipoStatusEnum.Andamento =>
            $"Prezado(a) {requisitanteNome},\n\nInformamos que a sua requisição '{requisicaoNome}' está atualmente em andamento.\n\nAtenciosamente,\nGrupo F&F",

        TipoStatusEnum.Aguardando =>
            $"Prezado(a) {requisitanteNome},\n\nA sua requisição '{requisicaoNome}' encontra-se no estado 'Aguardando' e depende de uma ação ou aprovação.\n\nAtenciosamente,\nGrupo F&F",

        TipoStatusEnum.Cancelado =>
            $"Prezado(a) {requisitanteNome},\n\nInformamos que a sua requisição '{requisicaoNome}' foi cancelada.\n\nAtenciosamente,\nGrupo F&F",

        TipoStatusEnum.Concluido =>
            $"Prezado(a) {requisitanteNome},\n\nA sua requisição '{requisicaoNome}' foi concluída com sucesso.\n\nAtenciosamente,\nGrupo F&F",

        _ =>
            $"Prezado(a) {requisitanteNome},\n\nNão foi possível determinar o status da sua requisição '{requisicaoNome}'. Entre em contato para mais informações.\n\nAtenciosamente,\nGrupo F&F"
      };
    }
  }
}
