const updateAvatarInput = document.getElementById('updateAvatarInput');
const errorMessageContainer = document.getElementById('errorMessageContainer');
const avatarExtensions = ['.jpg', '.jpeg', '.png'];

document.getElementById('updateAvatarBtn').addEventListener('click', () => {
    updateAvatarInput.click();
});

updateAvatarInput.addEventListener('change', async function (e) {
    const file = this.files[0];
    if (!file.type.startsWith('image/')) {
        this.value = '';
        errorMessageContainer.textContent = 'Неверный формат';
        errorMessageContainer.style.display = 'block';
        return;
    }

    const fileName = file.name.toLowerCase();
    if (!avatarExtensions.some(x => fileName.endsWith(x))) {
        this.value = '';
        errorMessageContainer.textContent = 'Неверный формат';
        errorMessageContainer.style.display = 'block';
        return;
    }

    const updateAvatarForm = document.getElementById('updateAvatarForm');
    const formData = new FormData(updateAvatarForm);

    const response = await fetch(updateAvatarForm.action,
        {
            method: 'POST',
            body: formData
        });

    if (response.redirected) {
        window.location.href = response.url;
    }
    else if (response.status === 400) {
        errorMessageContainer.textContent = await response.text();
        errorMessageContainer.style.display = 'block';
        this.value = '';
    }
    else if (response.ok) {
        window.location.reload();
    }
    else if (response.status === 404) {
        errorMessageContainer.textContent = "Размер файла превышает 2 мб";
        errorMessageContainer.style.display = 'block';
        this.value = '';
    }
});