document.addEventListener('DOMContentLoaded', function () {
  const contextMenu = document.getElementById('context-menu');
  const addFolderBtn = document.getElementById('add-folder');
  const folderContainer = document.querySelector('.folders');

  document.addEventListener('contextmenu', function (e) {
    e.preventDefault();
    const x = e.clientX;
    const y = e.clientY;

    contextMenu.style.top = `${y}px`;
    contextMenu.style.left = `${x}px`;
    contextMenu.style.display = 'block';
  });

  document.addEventListener('click', function (e) {
    if (!contextMenu.contains(e.target)) {
      contextMenu.style.display = 'none';
    }
  });

  document.getElementById('create-folder').addEventListener('click', function () {
    const folderName = prompt('Digite o nome da nova pasta:');
    if (folderName) {
      // Chame um método para adicionar a nova pasta (via AJAX ou outro método)
      fetch(`/Relatorios/AddFolder?folderName=${encodeURIComponent(folderName)}`, {
        method: 'POST'
      }).then(response => {
        if (response.ok) {
          location.reload();
        } else {
          alert('Erro ao criar pasta.');
        }
      });
    }
  });

  document.getElementById('upload-file').addEventListener('click', function () {
    const folderName = prompt('Digite o nome da pasta para upload:');
    if (folderName) {
      const fileInput = document.createElement('input');
      fileInput.type = 'file';
      fileInput.accept = '*/*';
      fileInput.addEventListener('change', function () {
        const file = fileInput.files[0];
        if (file) {
          const formData = new FormData();
          formData.append('fileUpload', file);
          formData.append('folderName', folderName);
          formData.append('fileName', file.name);

          fetch('/Relatorios/AddFile', {
            method: 'POST',
            body: formData
          }).then(response => response.json()).then(result => {
            if (result.success) {
              location.reload();
            } else {
              alert('Erro ao enviar arquivo.');
            }
          });
        }
      });
      fileInput.click();
    }
  });
});
