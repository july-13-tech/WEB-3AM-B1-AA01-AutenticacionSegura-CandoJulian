document.querySelectorAll('[data-alternar-contrasena]').forEach(function (boton) {
    boton.addEventListener('click', function () {
        var campo = document.getElementById(boton.dataset.alternarContrasena);
        var mostrar = campo.type === 'password';
        campo.type = mostrar ? 'text' : 'password';
        boton.querySelector('i').className = mostrar ? 'bi bi-eye-slash' : 'bi bi-eye';
    });
});