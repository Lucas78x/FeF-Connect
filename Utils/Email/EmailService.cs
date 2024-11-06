using AspnetCoreMvcFull.Enums;
using AspnetCoreMvcFull.Models;
using System.Net;
using System.Net.Mail;

namespace AspnetCoreMvcFull.Utils.Email
{
  public class EmailService : IEmailService
  {

    public void EnviarEmail(string requisitanteNome, string requisicao, string requisitanteEmail, string relato, TipoStatusEnum status, byte[] pdfBytes)
    {
      string smtpServer = "mail.ff.tv.br"; // Environment.GetEnvironmentVariable("SMTP_Server");
      int smtpPort = 587;
      string senderEmail = "lucasbastos@grupoff.net.br"; // Environment.GetEnvironmentVariable("EMAIL_USER");
      string senderPassword = "Aaa!@123#*"; // Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

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

        if (pdfBytes != null && pdfBytes.Length > 0)
        {

          MemoryStream pdfStream = new MemoryStream(pdfBytes);
          Attachment pdfAttachment = new Attachment(pdfStream, "Requisicao.pdf", "application/pdf");
          mailMessage.Attachments.Add(pdfAttachment);
        }
      

        smtpClient.Send(mailMessage);
        Console.WriteLine("E-mail enviado com sucesso!");
      }
      catch (Exception ex)
      {
        Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
      }
    }
    public void EnviarEmailFeriasProgramadas( FeriasModel ferias, string funcionarioEmail, byte[] pdfBytes)
    {
      string smtpServer = "mail.ff.tv.br"; // Environment.GetEnvironmentVariable("SMTP_Server");
      int smtpPort = 587;
      string senderEmail = "lucasbastos@grupoff.net.br"; // Environment.GetEnvironmentVariable("EMAIL_USER");
      string senderPassword = "Aaa!@123#*"; // Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

      string subject = GerarAssuntoEmailFerias( ferias);
      string body = GerarCorpoEmailFerias(ferias);

      try
      {
        SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
          Credentials = new NetworkCredential(senderEmail, senderPassword),
          EnableSsl = true
        };

        MailMessage mailMessage = new MailMessage(senderEmail, funcionarioEmail, subject, body);

        if (pdfBytes != null && pdfBytes.Length > 0)
        {

          MemoryStream pdfStream = new MemoryStream(pdfBytes);
          Attachment pdfAttachment = new Attachment(pdfStream, "Ferias.pdf", "application/pdf");
          mailMessage.Attachments.Add(pdfAttachment);
        }

        smtpClient.Send(mailMessage);
        Console.WriteLine("E-mail enviado com sucesso!");
      }
      catch (Exception ex)
      {
        Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
      }
    }

    private string GerarAssuntoEmailFerias( FeriasModel ferias)
    {
      return $"Programação de Férias - {ferias.NomeFuncionario} ({ferias.DataInicio.ToString("dd/MM/yyyy")} a {ferias.DataFim.ToString("dd/MM/yyyy")})";
    }

    private string GerarCorpoEmailFerias( FeriasModel ferias)
    {
      return $"Prezado(a) {ferias.NomeFuncionario},\n\n" +
            $"Gostaríamos de informar a programação das suas férias:\n\n" +
            $"Detalhes da Programação de Férias:\n" +
            $"Nome: {ferias.NomeFuncionario}\n" +
            $"Data de Início: {ferias.DataInicio.ToString("dd/MM/yyyy")}\n" +
            $"Data de Fim: {ferias.DataFim.ToString("dd/MM/yyyy")}\n" +
            $"Observações: {ferias.Observacoes}\n\n" +
            "Se você tiver alguma dúvida ou precisar de mais informações, não hesite em nos contatar.\n\n" +
            "Atenciosamente,\n" +
            "Grupo F&F";
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
