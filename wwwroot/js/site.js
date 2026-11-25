﻿// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    var CLAVE = 'menuLateralColapsado';
    var boton = document.getElementById('btnMenuColapsar');
    var menu = document.getElementById('menuLateral');
    if (!boton || !menu) return;

    function aplicar(colapsado) {
        document.body.classList.toggle('menu-colapsado', colapsado);
        var icono = boton.querySelector('i');
        if (icono) icono.className = colapsado ? 'bi bi-chevron-right' : 'bi bi-chevron-left';
        boton.setAttribute('aria-label', colapsado ? 'Expandir menú' : 'Contraer menú');
        menu.querySelectorAll('.nav-link').forEach(function (enlace) {
            if (colapsado) enlace.setAttribute('title', enlace.textContent.trim());
            else enlace.removeAttribute('title');
        });
        try { localStorage.setItem(CLAVE, colapsado ? '1' : '0'); } catch (e) {}
    }

    var guardado = null;
    try { guardado = localStorage.getItem(CLAVE); } catch (e) {}
    aplicar(guardado === '1');

    boton.addEventListener('click', function () {
        aplicar(!document.body.classList.contains('menu-colapsado'));
    });
})();
