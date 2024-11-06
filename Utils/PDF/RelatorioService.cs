using AspnetCoreMvcFull.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace AspnetCoreMvcFull.Utils.PDF
{
  public class RelatorioService : IRelatorioService
  {
    public byte[] GerarRelatorio(RequisicoesModel requisicao)
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var doc = new Document(PageSize.A4, 50, 50, 50, 50))
        {
          PdfWriter.GetInstance(doc, memoryStream);
          doc.Open();

          try
          {
            string caminhoLogo = Path.Combine("wwwroot", "img", "icons", "GrupoFF.png");
            Image logo = Image.GetInstance(caminhoLogo);
            logo.ScaleAbsolute(100, 50); // Ajuste o tamanho do logotipo
            logo.Alignment = Element.ALIGN_CENTER;
            doc.Add(logo);
          }
          catch (Exception ex)
          {
            Console.WriteLine("Erro ao carregar o logotipo: " + ex.Message);
          }

          Font fontInformacao = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
          Paragraph informacoes = new Paragraph("F & F Comunicações\nRua Senhor do Bonfim, 544 LOJA 02\nMonte Gordo - Camaçari/BA\nCEP: 42839-852 Tel: (71) 4062-8609", fontInformacao);
          informacoes.Alignment = Element.ALIGN_CENTER;
          informacoes.SpacingBefore = 10;
          informacoes.SpacingAfter = 10;
          doc.Add(informacoes);

          Font fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLACK);
          Paragraph titulo = new Paragraph("Relatório de Requisição", fontTitulo);
          titulo.Alignment = Element.ALIGN_CENTER;
          doc.Add(titulo);

          doc.Add(new Paragraph("\n"));

          Font fontSeccaoTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.DARK_GRAY);
          Paragraph secaoTitulo = new Paragraph("Dados da Requisição", fontSeccaoTitulo);
          doc.Add(secaoTitulo);

          doc.Add(new Paragraph("\n"));

          PdfPTable tabela = new PdfPTable(2);
          tabela.WidthPercentage = 100;
          tabela.SetWidths(new float[] { 1, 3 });

          Font fontCabecalho = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
          Font fontConteudo = FontFactory.GetFont(FontFactory.HELVETICA, 12);

          void AdicionarCelula(string cabecalho, string conteudo)
          {
            PdfPCell cellHeader = new PdfPCell(new Phrase(cabecalho, fontCabecalho));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.VerticalAlignment = Element.ALIGN_MIDDLE;
            cellHeader.BackgroundColor = BaseColor.GRAY;
            cellHeader.Padding = 5;
            tabela.AddCell(cellHeader);

            PdfPCell cellContent = new PdfPCell(new Phrase(conteudo, fontConteudo));
            cellContent.HorizontalAlignment = Element.ALIGN_LEFT;
            cellContent.VerticalAlignment = Element.ALIGN_MIDDLE;
            cellContent.Padding = 5;
            tabela.AddCell(cellContent);
          }

          AdicionarCelula("ID da Requisição:", requisicao.Id.ToString());
          AdicionarCelula("Descrição:", requisicao.Descricao);
          AdicionarCelula("Status:", requisicao.Status.ToString());
          AdicionarCelula("ID do Funcionário:", requisicao.FuncionarioId.ToString());
          var setor = requisicao.Setor.ToString().Replace("_", " ");
          AdicionarCelula("Setor:", setor);
          AdicionarCelula("Nome do Requisitante:", requisicao.RequisitanteNome);
          AdicionarCelula("ID do Requisitante:", requisicao.RequisitanteId.ToString());
          AdicionarCelula("Data de Início:", requisicao.DataInicio.HasValue ? requisicao.DataInicio.Value.ToString("dd/MM/yyyy HH:mm") : "Não definida");
          DateTime dataMinima = new DateTime(1900, 1, 1, 0, 0, 0);

          if (requisicao.DataFinalizacao != dataMinima)
          {
            AdicionarCelula("Data de Finalização:", requisicao.DataFinalizacao.HasValue ? requisicao.DataFinalizacao.Value.ToString("dd/MM/yyyy HH:mm") : "");
          }
          else
          {
            AdicionarCelula("Data de Finalização:","");
          }
          AdicionarCelula("Descrição da Solução:", string.IsNullOrEmpty(requisicao.DescricaoSolucao) ? "" : requisicao.DescricaoSolucao);

          doc.Add(tabela);

          doc.Add(new Paragraph("\n"));

          // Seção de Rodapé
          Font fontRodape = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.GRAY);
          Paragraph rodape = new Paragraph("Relatório gerado em: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fontRodape);
          rodape.Alignment = Element.ALIGN_RIGHT;
          doc.Add(rodape);

          doc.Close();
        }

        return memoryStream.ToArray();
      }
    }
    public byte[] GerarRelatorioFerias(FeriasModel ferias)
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var doc = new Document(PageSize.A4, 50, 50, 50, 50))
        {
          PdfWriter.GetInstance(doc, memoryStream);
          doc.Open();

          try
          {
            string caminhoLogo = Path.Combine("wwwroot", "img", "icons", "GrupoFF.png");
            Image logo = Image.GetInstance(caminhoLogo);
            logo.ScaleAbsolute(100, 50); // Ajuste o tamanho do logotipo
            logo.Alignment = Element.ALIGN_CENTER;
            doc.Add(logo);
          }
          catch (Exception ex)
          {
            Console.WriteLine("Erro ao carregar o logotipo: " + ex.Message);
          }

          // Informações da empresa
          Font fontInformacao = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
          Paragraph informacoes = new Paragraph("F & F Comunicações\nRua Senhor do Bonfim, 544 LOJA 02\nMonte Gordo - Camaçari/BA\nCEP: 42839-852 Tel: (71) 4062-8609", fontInformacao);
          informacoes.Alignment = Element.ALIGN_CENTER;
          informacoes.SpacingBefore = 10;
          informacoes.SpacingAfter = 10;
          doc.Add(informacoes);

          // Título do relatório
          Font fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLACK);
          Paragraph titulo = new Paragraph("Relatório de Férias", fontTitulo);
          titulo.Alignment = Element.ALIGN_CENTER;
          doc.Add(titulo);

          doc.Add(new Paragraph("\n"));

          // Seção de dados de férias
          Font fontSeccaoTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.DARK_GRAY);
          Paragraph secaoTitulo = new Paragraph("Dados das Férias", fontSeccaoTitulo);
          secaoTitulo.SpacingBefore = 10;
          secaoTitulo.SpacingAfter = 10;
          doc.Add(secaoTitulo);

          PdfPTable tabela = new PdfPTable(2);
          tabela.WidthPercentage = 100;
          tabela.SetWidths(new float[] { 1, 3 });

          Font fontCabecalho = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
          Font fontConteudo = FontFactory.GetFont(FontFactory.HELVETICA, 12);

          // Função para adicionar células à tabela
          void AdicionarCelula(string cabecalho, string conteudo)
          {
            PdfPCell cellHeader = new PdfPCell(new Phrase(cabecalho, fontCabecalho));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.VerticalAlignment = Element.ALIGN_MIDDLE;
            cellHeader.BackgroundColor = BaseColor.GRAY;
            cellHeader.Padding = 5;
            tabela.AddCell(cellHeader);

            PdfPCell cellContent = new PdfPCell(new Phrase(conteudo, fontConteudo));
            cellContent.HorizontalAlignment = Element.ALIGN_LEFT;
            cellContent.VerticalAlignment = Element.ALIGN_MIDDLE;
            cellContent.Padding = 5;
            tabela.AddCell(cellContent);
          }

          // Adicionando os dados das férias à tabela
          AdicionarCelula("ID da Férias:", ferias.Id.ToString());
          AdicionarCelula("Nome do Funcionário:", ferias.NomeFuncionario);
          AdicionarCelula("Data de Início:", ferias.DataInicio.ToString("dd/MM/yyyy"));
          AdicionarCelula("Data de Fim:", ferias.DataFim.ToString("dd/MM/yyyy"));
          AdicionarCelula("Observações:", string.IsNullOrEmpty(ferias.Observacoes) ? "Nenhuma" : ferias.Observacoes);

          doc.Add(tabela);
          doc.Add(new Paragraph("\n"));

          // Seção de Rodapé
          Font fontRodape = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.GRAY);
          Paragraph rodape = new Paragraph("Relatório gerado em: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fontRodape);
          rodape.Alignment = Element.ALIGN_RIGHT;
          doc.Add(rodape);

          doc.Close();
        }

        return memoryStream.ToArray();
      }
    }

  }

}
