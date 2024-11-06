using AspnetCoreMvcFull.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;

namespace AspnetCoreMvcFull.Utils.PDF
{
  public class RelatorioService : IRelatorioService
  {
    public byte[] GerarRelatorio(RequisicoesModel requisicao)
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var pdfWriter = new PdfWriter(memoryStream))
        {
          using (var pdfDocument = new PdfDocument(pdfWriter))
          {
            Document document = new Document(pdfDocument, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(50, 50, 50, 50);

            try
            {
              string caminhoLogo = Path.Combine("wwwroot", "img", "icons", "GrupoFF.png");
              ImageData imageData = ImageDataFactory.Create(caminhoLogo);
              iText.Layout.Element.Image logo = new iText.Layout.Element.Image(imageData);
              logo.ScaleAbsolute(100, 50); // Ajuste o tamanho do logotipo
              logo.SetHorizontalAlignment(HorizontalAlignment.CENTER);
              document.Add(logo);
            }
            catch (Exception ex)
            {
              Console.WriteLine("Erro ao carregar o logotipo: " + ex.Message);
            }

            // Informações da empresa
            Paragraph informacoes = new Paragraph("F & F Comunicações\nRua Senhor do Bonfim, 544 LOJA 02\nMonte Gordo - Camaçari/BA\nCEP: 42839-852 Tel: (71) 4062-8609")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12)
                .SetMarginBottom(10);
            document.Add(informacoes);

            // Título do relatório
            Paragraph titulo = new Paragraph("Relatório de Requisição")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(22)
                .SetBold();
            document.Add(titulo);

            document.Add(new Paragraph("\n"));

            // Seção de dados da requisição
            Paragraph secaoTitulo = new Paragraph("Dados da Requisição")
                .SetFontSize(16)
                .SetBold()
                .SetMarginBottom(10);
            document.Add(secaoTitulo);

            Table tabela = new Table(2).UseAllAvailableWidth();
            tabela.SetWidth(UnitValue.CreatePercentValue(100));

            void AdicionarCelula(string cabecalho, string conteudo)
            {
              Cell headerCell = new Cell().Add(new Paragraph(cabecalho).SetFontSize(12).SetBold().SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                  .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.GRAY)
                  .SetPadding(5)
                  .SetTextAlignment(TextAlignment.LEFT);
              tabela.AddCell(headerCell);

              Cell contentCell = new Cell().Add(new Paragraph(conteudo).SetFontSize(12))
                  .SetPadding(5)
                  .SetTextAlignment(TextAlignment.LEFT);
              tabela.AddCell(contentCell);
            }

            AdicionarCelula("ID da Requisição:", requisicao.Id.ToString());
            AdicionarCelula("Descrição:", requisicao.Descricao);
            AdicionarCelula("Status:", requisicao.Status.ToString());
            AdicionarCelula("ID do Funcionário:", requisicao.FuncionarioId.ToString());
            AdicionarCelula("Setor:", requisicao.Setor.ToString().Replace("_", " "));
            AdicionarCelula("Nome do Requisitante:", requisicao.RequisitanteNome);
            AdicionarCelula("ID do Requisitante:", requisicao.RequisitanteId.ToString());
            AdicionarCelula("Data de Início:", requisicao.DataInicio.HasValue ? requisicao.DataInicio.Value.ToString("dd/MM/yyyy HH:mm") : "Não definida");

            DateTime dataMinima = new DateTime(1900, 1, 1, 0, 0, 0);
            AdicionarCelula("Data de Finalização:", requisicao.DataFinalizacao != dataMinima ? requisicao.DataFinalizacao.Value.ToString("dd/MM/yyyy HH:mm") : "");
            AdicionarCelula("Descrição da Solução:", string.IsNullOrEmpty(requisicao.DescricaoSolucao) ? "" : requisicao.DescricaoSolucao);

            document.Add(tabela);

            document.Add(new Paragraph("\n"));

            // Seção de rodapé
            Paragraph rodape = new Paragraph("Relatório gerado em: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY);
            document.Add(rodape);

            document.Close();
          }
        }
        return memoryStream.ToArray();
      }
    }

    public byte[] GerarRelatorioFerias(FeriasModel ferias)
    {
      using (var memoryStream = new MemoryStream())
      {
        // Criando um escritor de PDF e documento
        using (var writer = new PdfWriter(memoryStream))
        {
          var pdf = new PdfDocument(writer);
          var doc = new Document(pdf);

          try
          {
            string caminhoLogo = Path.Combine("wwwroot", "img", "icons", "GrupoFF.png");
            ImageData imageData = ImageDataFactory.Create(caminhoLogo);
            var logo = new Image(imageData);
            logo.ScaleToFit(100, 50); // Ajusta o tamanho do logotipo
            logo.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            doc.Add(logo);
          }
          catch (Exception ex)
          {
            Console.WriteLine("Erro ao carregar o logotipo: " + ex.Message);
          }

          // Carregando as fontes
          var fontInformacao = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
          var fontTitulo = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
          var fontConteudo = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
          var fontCabecalho = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

          // Informações da empresa
          var infoEmpresa = new Paragraph("F & F Comunicações\nRua Senhor do Bonfim, 544 LOJA 02\nMonte Gordo - Camaçari/BA\nCEP: 42839-852 Tel: (71) 4062-8609")
              .SetFont(fontInformacao)
              .SetFontSize(12)
              .SetTextAlignment(TextAlignment.CENTER)
              .SetMarginBottom(10);
          doc.Add(infoEmpresa);

          // Título do relatório
          var titulo = new Paragraph("Relatório de Férias")
              .SetFont(fontTitulo)
              .SetFontSize(22)
              .SetTextAlignment(TextAlignment.CENTER)
              .SetMarginBottom(10);
          doc.Add(titulo);

          // Seção de dados de férias
          var secaoTitulo = new Paragraph("Dados das Férias")
              .SetFont(fontTitulo)
              .SetFontSize(16)
              .SetFontColor(ColorConstants.DARK_GRAY)
              .SetMarginBottom(10);
          doc.Add(secaoTitulo);

          // Tabela para os dados das férias
          var tabela = new Table(new float[] { 1, 3 }).UseAllAvailableWidth();

          void AdicionarCelula(string cabecalho, string conteudo)
          {
            Cell cellHeader = new Cell().Add(new Paragraph(cabecalho).SetFont(fontCabecalho).SetFontSize(12).SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(ColorConstants.GRAY)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT);
            tabela.AddCell(cellHeader);

            Cell cellContent = new Cell().Add(new Paragraph(conteudo).SetFont(fontConteudo).SetFontSize(12))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT);
            tabela.AddCell(cellContent);
          }

          // Adicionando os dados à tabela
          AdicionarCelula("ID da Férias:", ferias.Id.ToString());
          AdicionarCelula("Nome do Funcionário:", ferias.NomeFuncionario);
          AdicionarCelula("Data de Início:", ferias.DataInicio.ToString("dd/MM/yyyy"));
          AdicionarCelula("Data de Fim:", ferias.DataFim.ToString("dd/MM/yyyy"));
          AdicionarCelula("Observações:", string.IsNullOrEmpty(ferias.Observacoes) ? "Nenhuma" : ferias.Observacoes);

          doc.Add(tabela);
          doc.Add(new Paragraph("\n"));

          // Seção de rodapé
          var rodape = new Paragraph("Relatório gerado em: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
              .SetFont(fontConteudo)
              .SetFontSize(10)
              .SetFontColor(ColorConstants.GRAY)
              .SetTextAlignment(TextAlignment.RIGHT);
          doc.Add(rodape);

          doc.Close();
        }

        return memoryStream.ToArray();
      }
    }
  }
}
