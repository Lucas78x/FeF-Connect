document.getElementById('comunicadoForm').addEventListener('submit', async function (event) {
  event.preventDefault(); // Impede o envio padrão do formulário

  const formData = {
    titulo: document.getElementById('comunicadoTitulo').value,
    mensagem: document.getElementById('comunicadoMensagem').value,
    dataPublicacao: document.getElementById('comunicadoData').value
  };

  try {
    const response = await fetch('/Inicio/AddComunicado', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(formData),
    });

    if (response.ok) {
      alert('Comunicado adicionado com sucesso!');
      $('#addComunicadoModal').modal('hide'); // Fechar o modal
      location.reload(); // Recarregar a página
    } else {
      alert('Erro ao adicionar comunicado.');
    }
  } catch (error) {
    console.error('Erro ao enviar o comunicado:', error);
    alert('Erro ao enviar comunicado.');
  }
});

document.getElementById('eventoForm').addEventListener('submit', async function (event) {
  event.preventDefault(); // Impede o envio padrão do formulário

  const formData = {
    descricao: document.getElementById('eventoDescricao').value,
    data: document.getElementById('eventoData').value,
    local: document.getElementById('eventoLocal').value,
  };

  const setor = document.getElementById('Status').value;

  try {
    const response = await fetch(`/Inicio/AddEvento?setor=${encodeURIComponent(setor)}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(formData),
    });

    if (response.ok) {
      alert('Evento adicionado com sucesso!');
      $('#addEventoModal').modal('hide'); // Fechar o modal
      location.reload(); // Recarregar a página
    } else {
      alert('Erro ao adicionar evento.');
    }
  } catch (error) {
    console.error('Erro ao enviar o evento:', error);
    alert('Erro ao enviar evento.');
  }
});
