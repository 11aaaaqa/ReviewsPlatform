const addSubcategoryModalErrorBlock = document.getElementById('addSubcategoryModalErrorBlock');
document.getElementById('addSubcategorySubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const addSubcategoryForm = document.getElementById('addSubcategoryForm');
    const formData = new FormData(addSubcategoryForm);

    const response = await fetch(addSubcategoryForm.action, { method: 'POST', body: formData });

    if (response.redirected) {
        window.location.href = response.url;
    } else if (response.status === 400 || response.status === 409) {
        addSubcategoryModalErrorBlock.textContent = await response.text();
        addSubcategoryModalErrorBlock.style.display = 'block';
    } else if (response.ok) {
        window.location.reload();
    } else {
        addSubcategoryModalErrorBlock.textContent = "Что-то пошло не так, попробуйте еще раз";
        addSubcategoryModalErrorBlock.style.display = 'block';
    }
});

const addSubcategoryModalOverlay = document.getElementById('addSubcategoryModalOverlay');
addSubcategoryModalOverlay.addEventListener('click', e => {
    if (e.target === addSubcategoryModalOverlay) {
        addSubcategoryModalOverlay.style.display = 'none';
    }
});
document.getElementById('openAddSubcategoryModalBtn').addEventListener('click', () => {
    addSubcategoryModalOverlay.style.display = 'flex';
});