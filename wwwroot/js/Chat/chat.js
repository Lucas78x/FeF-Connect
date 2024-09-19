// chat.js

$(document).ready(function () {
  const dataContainer = document.getElementById('data-container');
  const usuarioAtualId = dataContainer.getAttribute('data-usuario-atual-id');
  const remetenteNome = dataContainer.getAttribute('data-remetente-nome');
  let destinatarioId = "";
  let connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

  initializeConnection();
  setupEventListeners();

  function initializeConnection() {
    connection.on("ReceiveMessage", handleReceiveMessage);
    connection.on("MessageReadNotification", handleMessageReadNotification);

    connection.start().then(function () {
      $("#enviarBtn").prop("disabled", false);
    }).catch(function (err) {
      console.error(err.toString());
    });
  }

  function setupEventListeners() {
    $(".list-group-item").click(handleContactClick);
    $("#enviarBtn").click(sendMessage);
    $("#conteudo").keypress(handleEnterKey);
    $("#chatWindow").on("scroll", handleChatScroll);
    $("#salvarEdicao").click(saveEdit);
  }

  function handleReceiveMessage(remetenteId, destinatarioId, conteudo, remetenteNome, mensagemId) {
    let mensagemHtml;

    if (remetenteId === usuarioAtualId) {
      mensagemHtml = createSentMessageHtml(conteudo, mensagemId);
    } else if (destinatarioId === usuarioAtualId) {
      mensagemHtml = createReceivedMessageHtml(conteudo, remetenteNome, mensagemId);
      playNotificationSound();
    } else {
      return;
    }

    $("#chatWindow").append(mensagemHtml);
    $("#chatWindow").scrollTop($("#chatWindow")[0].scrollHeight);
  }

  function handleMessageReadNotification(mensagemId, destinatarioId) {
    if (destinatarioId === usuarioAtualId) {
      const mensagemElement = $(`.message.sent[data-mensagem-id='${mensagemId}']`);
      if (mensagemElement.length) {
        mensagemElement.append('<span class="message-read-indicator">✔️ Lida</span>');
        mensagemElement.removeClass("new-message");
      }
    }
  }

  function handleContactClick() {
    destinatarioId = $(this).data("destinatario-id");
    $("#chatWindow").empty();
    $(".list-group-item").removeClass("active");
    $(this).addClass("active");
    loadMessages(destinatarioId);
  }

  function handleEnterKey(e) {
    if (e.which === 13) {
      e.preventDefault();
      sendMessage();
    }
  }
  function playNotificationSound() {
    notificationSound.play();
  }

  function handleChatScroll() {
    $(".message.new-message").each(function () {
      const mensagemId = $(this).data("mensagem-id");
      if (isElementInView($(this), $("#chatWindow")) && !$(this).hasClass("marked-as-read")) {
        marcarComoLida(mensagemId);
      }
    });
  }

  function createSentMessageHtml(conteudo, mensagemId) {
    return `<div class="message sent" data-mensagem-id="${mensagemId}">
                    <strong>Você:</strong> ${conteudo}
                </div>`;
  }

  function createReceivedMessageHtml(conteudo, remetenteNome, mensagemId) {
    return `<div class="message received new-message" data-mensagem-id="${mensagemId}">
                    <strong>${remetenteNome}</strong>: ${conteudo}
                </div>`;
  }

  function loadMessages(destinatarioId) {
    $.get(`/Chat/ObterMensagens?destinatarioId=${destinatarioId}`, function (data) {
      $("#chatWindow").empty();
      data.forEach(function (mensagem) {
        const mensagemHtml = createMessageHtml(mensagem);
        $("#chatWindow").append(mensagemHtml);
      });
      $("#chatWindow").scrollTop($("#chatWindow")[0].scrollHeight);
    });
  }

  function createMessageHtml(mensagem) {
    const dataEnvio = new Date(mensagem.dataEnvio);
    const formattedDate = formatDate(dataEnvio);

    if (mensagem.remetenteId === usuarioAtualId) {
      return `<div class="message sent" data-mensagem-id="${mensagem.id}">
                        <strong>Você:</strong> ${mensagem.conteudo}
                        <div class="timestamp">${formattedDate}</div>
                        ${mensagem.lida ? '<span class="message-read-indicator">✔️ Lida</span>' : ''}
                    </div>`;
    } else {
      return `<div class="message received new-message" data-mensagem-id="${mensagem.id}">
                        <strong>${mensagem.remetente.nome}</strong>: ${mensagem.conteudo}
                        <div class="timestamp">${formattedDate}</div>
                    </div>`;
    }
  }

  function formatDate(date) {
    const now = new Date();
    const diff = now - date;
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (days === 0) {
      if (hours === 0) {
        if (minutes === 0) {
          return seconds < 60 ? "Agora" : `${seconds} segundos atrás`;
        }
        return `${minutes} minutos atrás`;
      }
      return `${hours} horas atrás`;
    } else if (days === 1) {
      return "Ontem";
    } else if (days < 7) {
      return `${days} dias atrás`;
    } else {
      const options = { day: '2-digit', month: '2-digit', year: 'numeric' };
      return date.toLocaleDateString(undefined, options);
    }
  }

  function sendMessage() {
    const conteudo = $("#conteudo").val();
    if (destinatarioId) {
      $.post("/Chat/EnviarMensagem", { destinatarioId, conteudo })
        .done(function (data) {
          if (data.success) {
            connection.invoke("SendMessage", destinatarioId, conteudo, data.mensagemId, data.usuarioAtualId, remetenteNome)
              .then(function () {
                $("#conteudo").val("");
              })
              .catch(function (err) {
                console.error(err.toString());
              });
          } else {
            alert("Erro ao enviar a mensagem.");
          }
        })
        .fail(function () {
          alert("Erro ao enviar a mensagem.");
        });
    } else {
      alert("Selecione um contato para enviar a mensagem.");
    }
  }

  function marcarComoLida(mensagemId) {
    const messageElement = $(`.message[data-mensagem-id='${mensagemId}']`);

    if (!messageElement.hasClass("marked-as-read") && !messageElement.find('.message-read-indicator').length) {
      $.post("/Chat/MarcarComoLida", { mensagemId })
        .done(function () {
          messageElement.removeClass("new-message");
          messageElement.addClass("marked-as-read");

          connection.invoke("MessageRead", mensagemId)
            .catch(function (err) {
              console.error(err.toString());
            });
        })
        .fail(function () {
          alert("Erro ao marcar como lida.");
        });
    }
  }

  function isElementInView(element, container) {
    const elementTop = element.offset().top;
    const elementBottom = elementTop + element.outerHeight();
    const containerTop = container.offset().top;
    const containerBottom = containerTop + container.height();

    return elementBottom <= containerBottom && elementTop >= containerTop;
  }

  function prepararParaEditar(mensagemId, conteudo) {
    $("#mensagemId").val(mensagemId);
    $("#novoConteudo").val(conteudo);
    $("#editarMensagem").show();
  }

  function saveEdit() {
    const mensagemId = $("#mensagemId").val();
    const novoConteudo = $("#novoConteudo").val();

    $.post("/Chat/AtualizarMensagem", { mensagemId, novoConteudo })
      .done(function () {
        $("#editarMensagem").hide();
        loadMessages(destinatarioId);
      })
      .fail(function () {
        alert("Erro ao atualizar a mensagem.");
      });
  }
});
