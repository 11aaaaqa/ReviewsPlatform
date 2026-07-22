function showLoader() {
    if (document.getElementById('loadSpinnerContainer')) {
        document.getElementById('loadSpinnerContainer').innerHTML = '<div class="load-spinner"></div>';
    }
}

function hideLoader() {
    if (document.getElementById('loadSpinnerContainer')) {
        document.getElementById('loadSpinnerContainer').innerHTML = '';
    }
}