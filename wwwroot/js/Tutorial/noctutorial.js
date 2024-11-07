// Atualiza o modal de visualização de documento
$('#documentModal').on('show.bs.modal', function (event) {
  var button = $(event.relatedTarget);
  var filepath = button.data('filepath');
  var title = button.data('title');
  var filetype = button.data('filetype');

  var modal = $(this);
  modal.find('.modal-title').text(title);
  modal.find('#downloadLink').attr('href', filepath);

  // Limpa o conteúdo anterior do viewer
  var documentViewer = modal.find('#documentViewer');
  documentViewer.empty();

  if (filetype === 'mp4') {
    // Se for vídeo, adiciona um elemento <video>
    documentViewer.append('<video controls style="width: 100%; height: 500px;"><source src="' + filepath + '" type="video/mp4">Seu navegador não suporta o elemento de vídeo.</video>');
  } else {
    // Se for documento, adiciona um <iframe>
    documentViewer.append('<iframe src="' + filepath + '" style="width: 100%; height: 500px;" frameborder="0"></iframe>');
  }
});

// Abrir o modal de remoção e configurar a URL
$('#removeDocumentModal').on('show.bs.modal', function (event) {
  var button = $(event.relatedTarget);
  var filePath = button.data('filepath');

  var modal = $(this);
  modal.find('#confirmRemoveButton').data('filepath', filePath);
});

// Confirmar remoção
$('#confirmRemoveButton').on('click', function () {
  var filePath = $(this).data('filepath');

  fetch('/NocDocument/RemoveDocument', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ filePath: filePath })
  })
    .then(response => response.json())
    .then(data => {
      if (data.success) {
        location.reload();
      } else {
        alert('Erro ao remover o documento.');
      }
    });
});

// Função para filtrar documentos
function filterDocuments() {
  var searchInput = $('#searchInput').val().toLowerCase();
  var categoryFilter = $('#categoryFilter').val();

  $('#documentContainer .card').each(function () {
    var title = $(this).find('.card-title').text().toLowerCase();
    var category = $(this).hasClass(categoryFilter) || categoryFilter === '';

    if (title.includes(searchInput) && category) {
      $(this).show();
    } else {
      $(this).hide();
    }
  });
}

$('#searchInput').on('input', filterDocuments);
$('#categoryFilter').on('change', filterDocuments);

// Atualiza o modal de visualização de documento
$('#documentModal').on('show.bs.modal', function (event) {
  var button = $(event.relatedTarget);
  var filepath = button.data('filepath');
  var title = button.data('title');
  var filetype = button.data('filetype');

  var modal = $(this);
  modal.find('.modal-title').text(title);
  modal.find('#downloadLink').attr('href', filepath);

  // Limpa o conteúdo anterior do viewer
  var documentViewer = modal.find('#documentViewer');
  documentViewer.empty();

  if (filetype === 'mp4') {
    // Se for vídeo, adiciona um elemento <video>
    documentViewer.append('<video controls style="width: 100%; height: 500px;" id="modalVideo"><source src="' + filepath + '" type="video/mp4">Seu navegador não suporta o elemento de vídeo.</video>');
  } else {
    // Se for documento, adiciona um <iframe>
    documentViewer.append('<iframe src="' + filepath + '" style="width: 100%; height: 500px;" frameborder="0"></iframe>');
  }
});

// Pausa o vídeo quando o modal é fechado
$('#documentModal').on('hidden.bs.modal', function () {
  var video = document.getElementById('modalVideo');
  if (video) {
    video.pause();
    video.currentTime = 0; // Reinicia o vídeo para o começo
  }
});

// Função para validar o tipo de arquivo
document.getElementById('file').addEventListener('change', function () {
  var fileInput = this;
  var filePath = fileInput.value;
  var allowedExtensions = /(\.pdf|\.doc|\.docx|\.mp4|\.xls|\.xlsx|\.ppt|\.pptx|\.txt|\.bin|\.xml)$/i;

  if (!allowedExtensions.exec(filePath)) {
    alert('Por favor, escolha um arquivo com uma extensão válida.');
    fileInput.value = '';
    return false;
  }
});

// Submissão do formulário de upload
document.getElementById('submitButton').addEventListener('click', function () {
  var formData = new FormData(document.getElementById('uploadForm'));

  fetch('/NocTutorial/Upload', {
    method: 'POST',
    body: formData
  })
    .then(response => response.json())
    .then(data => {
      if (data.success) {
        alert('Documento adicionado com sucesso!');
        location.reload(); // Recarrega a página para atualizar a lista de documentos
      } else {
        alert('Ocorreu um erro ao adicionar o documento.');
      }
    })
    .catch(error => {
      console.error('Erro:', error);
      alert('Ocorreu um erro ao adicionar o documento.');
    });
});
