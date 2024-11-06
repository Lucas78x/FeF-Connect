document.addEventListener('DOMContentLoaded', function () {
  const addFolderBtn = document.getElementById('addFolderBtn');
  const modal = document.getElementById('addFolderModal');
  const closeModal = document.getElementsByClassName('close')[0];
  const saveFolderBtn = document.getElementById('saveFolderBtn');
  const folderItems = document.getElementById('folderItems');
  const uploadSection = document.getElementById('uploadSection');
  const uploadBtn = document.getElementById('uploadBtn');

  // Abrir modal
  addFolderBtn.onclick = function () {
    modal.style.display = 'block';
  };

  // Fechar modal
  closeModal.onclick = function () {
    modal.style.display = 'none';
  };

  // Adicionar nova pasta
  saveFolderBtn.onclick = async function () {
    const folderName = document.getElementById('folderName').value;
    if (folderName) {
      try {
        const response = await fetch('/Financeiro/CriarPasta', { 
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ folderName }),
        });

        if (response.ok) {
          const listItem = document.createElement('li');
          listItem.textContent = folderName;
          listItem.onclick = function () {
            uploadSection.style.display = 'block';
          };
          folderItems.appendChild(listItem);
          modal.style.display = 'none'; 
          document.getElementById('folderName').value = ''; 
          location.reload();
        } else {
          throw new Error('Erro ao criar pasta.');
        }
      } catch (error) {
        alert(error.message);
      }
    }
  };

  // Adicionar comprovante de pagamento
  uploadBtn.onclick = async function () {
    const selectedFile = document.getElementById('receiptFile').files[0];
    const folderName = folderItems.querySelector('li.selected') ?
      folderItems.querySelector('li.selected').textContent : ''; // Supondo que você tenha um método para selecionar a pasta
    const formData = new FormData();

    formData.append('file', selectedFile);
    formData.append('folderName', folderName);

    try {
      const response = await fetch('/SeuController/UploadComprovante', { // Altere para o seu controlador e ação
        method: 'POST',
        body: formData,
      });

      if (response.ok) {
        alert('Comprovante salvo com sucesso!');
        document.getElementById('receiptFile').value = ''; // Limpar o campo de arquivo
      } else {
        throw new Error('Erro ao salvar comprovante.');
      }
    } catch (error) {
      alert(error.message);
    }
  };

  // Fechar modal se clicar fora dele
  window.onclick = function (event) {
    if (event.target == modal) {
      modal.style.display = 'none';
    }
  };
});
