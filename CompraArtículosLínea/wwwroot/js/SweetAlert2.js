<script>
    @if (TempData["ErrorMessage"] != null)
    {
        <text>
            Swal.fire({
                icon: 'error',
            title: 'Error en el registro',
            text: '@TempData["ErrorMessage"]',
            confirmButtonText: 'Aceptar'
            });
        </text>
    }
</script>
