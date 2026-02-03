const userNameModalErrorBlock = document.getElementById('userNameModalErrorBlock');
document.getElementById('updateUserNameSubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const updateUserNameForm = document.getElementById('updateUserNameForm');
    const formData = new FormData(updateUserNameForm);

    const response = await fetch(updateUserNameForm.action, { method: 'POST', body: formData });

    if (response.redirected) {
        window.location.href = response.url;
    } else if (response.status === 400 || response.status === 409) {
        userNameModalErrorBlock.textContent = await response.text();
        userNameModalErrorBlock.style.display = 'block';
    } else if (response.ok) {
        window.location.reload();
    } else {
        userNameModalErrorBlock.textContent = "Что-то пошло не так, попробуйте еще раз";
        userNameModalErrorBlock.style.display = 'block';
    }
});

const userNameModalOverlay = document.getElementById('userNameModalOverlay');
userNameModalOverlay.addEventListener('click', e => {
    if (e.target === userNameModalOverlay) {
        userNameModalOverlay.style.display = 'none';
    }
});
document.getElementById('openUserNameModalBtn').addEventListener('click', () => {
    userNameModalOverlay.style.display = 'flex';
});