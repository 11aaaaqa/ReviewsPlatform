document.getElementById('chooseItemPictureBtn').addEventListener('click', () => {
    itemPictureInput.click();
});

itemPictureInput.addEventListener('change', function (e) {
    const file = this.files[0];
    if (!validatePicturesSize(file)) {
        errorMessageContainer.textContent = `Размер изображений не должен превышать ${picturesMaxSize} МБ`;
        errorMessageContainer.style.display = 'block';
        return;
    }
    errorMessageContainer.style.display = 'none';

    if (!file.type.startsWith('image/')) {
        this.value = '';
        errorMessageContainer.textContent = 'Неверный формат изображения';
        errorMessageContainer.style.display = 'block';
        return;
    }

    const fileName = file.name.toLowerCase();
    if (!pictureExtensions.some(x => fileName.endsWith(x))) {
        this.value = '';
        errorMessageContainer.textContent = 'Неверный формат изображения';
        errorMessageContainer.style.display = 'block';
        return;
    }

    errorMessageContainer.style.display = 'none';
    if (itemPictureImg.src && itemPictureImg.src.startsWith('blob:')) {
        URL.revokeObjectURL(itemPictureImg.src);
    }
    itemPictureImg.src = URL.createObjectURL(file);
    itemPictureImg.style.display = 'block';
});