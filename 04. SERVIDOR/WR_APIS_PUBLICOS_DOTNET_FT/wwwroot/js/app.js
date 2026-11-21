(function () {
  'use strict';

  function zonaEstado() {
    var z = document.querySelector('[data-estado]');
    if (!z) {
      z = document.createElement('div');
      z.className = 'estado';
      z.setAttribute('data-estado', '');
      (document.querySelector('.hoja') || document.body).appendChild(z);
    }
    return z;
  }

  function mostrarProgreso(texto) {
    var z = zonaEstado();
    z.innerHTML = '<div class="progreso" role="status"><span class="rueda"></span><span></span></div>';
    z.lastChild.lastChild.textContent = texto;
  }

  function bloquear() {
    setTimeout(function () {
      document.body.classList.add('ocupado');
      document.querySelectorAll('button').forEach(function (b) { b.disabled = true; });
    }, 0);
  }

  document.addEventListener('submit', function (e) {
    var f = e.target;
    if (!(f instanceof HTMLFormElement)) { return; }
    if (f.dataset.confirmar && !window.confirm(f.dataset.confirmar)) {
      e.preventDefault();
      return;
    }
    if (f.hasAttribute('data-filtro')) { return; }
    if (f.dataset.progreso) { mostrarProgreso(f.dataset.progreso); }
    bloquear();
  });

  window.addEventListener('pageshow', function (e) {
    if (e.persisted) { window.location.reload(); }
  });

  document.querySelectorAll('input[data-solo-numeros]').forEach(function (campo) {
    campo.addEventListener('input', function () {
      var limpio = campo.value.replace(/\D+/g, '');
      if (limpio !== campo.value) { campo.value = limpio; }
    });
  });

  document.querySelectorAll('input[data-solo-decimal]').forEach(function (campo) {
    campo.addEventListener('input', function () {
      var limpio = campo.value.replace(/[^\d.,]/g, '');
      var i = limpio.search(/[.,]/);
      if (i >= 0) { limpio = limpio.slice(0, i + 1) + limpio.slice(i + 1).replace(/[.,]/g, ''); }
      if (limpio !== campo.value) { campo.value = limpio; }
    });
  });

  document.querySelectorAll('form[data-filtro]').forEach(function (f) {
    var temporizador;
    var texto = f.querySelector('input[type="search"], input[type="text"]');
    var enviar = function () { f.requestSubmit(); };

    f.querySelectorAll('select').forEach(function (s) { s.addEventListener('change', enviar); });
    if (texto) {
      texto.addEventListener('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(enviar, 400);
      });
      
      if (new URLSearchParams(window.location.search).has('texto') && texto.value) {
        texto.focus();
        texto.setSelectionRange(texto.value.length, texto.value.length);
      }
    }
  });
})();
