// Função para validar o tipo de arquivo
document.getElementById('file').addEventListener('change', function () {
  var fileInput = this;
  var filePath = fileInput.value;
  var allowedExtensions = /(\.pdf|\.doc|\.docx|\.mp4|\.xls|\.xlsx|\.ppt|\.pptx|\.txt)$/i;

  if (!allowedExtensions.exec(filePath)) {
    alert('Por favor, escolha um arquivo com uma extensão válida.');
    fileInput.value = '';
    return false;
  }
});

// Submissão do formulário de upload
document.getElementById('submitButton').addEventListener('click', function () {
  var formData = new FormData(document.getElementById('uploadForm'));

  fetch('/Document/Upload', {
    method: 'POST',
    body: formData
  })
    .then(response => response.json())
    .then(data => {
      if (data.success) {
        location.reload();
      } else {
        alert('Erro ao fazer upload do documento.');
      }
    });
});

// Abrir o modal de visualização
$('#documentModal').on('show.bs.modal', function (event) {
  var button = $(event.relatedTarget);
  var filePath = button.data('filepath');
  var title = button.data('title');
  var fileType = button.data('filetype');

  var modal = $(this);
  modal.find('.modal-title').text(title);

  if (fileType === 'pdf') {
    modal.find('#documentViewer').html('<embed src="' + filePath + '" type="application/pdf" width="100%" height="600px" />');
  } else if (fileType === 'mp4') {
    modal.find('#documentViewer').html('<video src="' + filePath + '" controls width="100%" height="600px"></video>');
  } else {
    modal.find('#documentViewer').html('<iframe src="' + filePath + '" width="100%" height="600px"></iframe>');
  }

  modal.find('#downloadLink').attr('href', filePath);
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

  fetch('/Document/RemoveDocument', {
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

// Filtra os documentos pelo tipo de arquivo
$('#fileTypeFilter').on('change', function () {
  var selectedFileType = $(this).val().toLowerCase();
  if (selectedFileType) {
    $('#documentContainer .card').hide();
    $('#documentContainer .card.' + selectedFileType).show();
  } else {
    $('#documentContainer .card').show();
  }
});

// Barra de busca
$('#searchInput').on('keyup', function () {
  var searchText = $(this).val().toLowerCase();
  $('#documentContainer .card').each(function () {
    var title = $(this).find('.card-title').text().toLowerCase();
    if (title.includes(searchText)) {
      $(this).show();
    } else {
      $(this).hide();
    }
  });
});
