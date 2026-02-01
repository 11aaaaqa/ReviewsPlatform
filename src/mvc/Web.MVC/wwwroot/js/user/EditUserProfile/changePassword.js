const passwordErrorBlock = document.getElementById('passwordErrorBlock');
document.getElementById('checkPasswordSubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const checkPasswordForm = document.getElementById('checkPasswordForm');
    const formData = new FormData(checkPasswordForm);

    const response = await fetch(checkPasswordForm.action, { method: 'POST', body: formData });

    if (response.ok) {
        const result = await response.json();
        if (result) {
            document.getElementById('updatePasswordOldPassword').value = document.getElementById('checkPasswordInput').value;
            passwordErrorBlock.style.display = 'none';
            document.getElementById('checkPasswordForm').style.display = 'none';
            document.getElementById('updatePasswordForm').style.display = 'block';
        } else {
            document.getElementById('checkPasswordInput').value = '';
            passwordErrorBlock.textContent = 'Неверный пароль';
            passwordErrorBlock.style.display = 'block';
        }
    }
    else if (response.status === 400) {
        document.getElementById('checkPasswordInput').value = '';

        const json = await response.json();
        passwordErrorBlock.innerHTML = '';

        json.errorMessages.forEach(error => {
            const div = document.createElement('div');
            div.textContent = error;
            passwordErrorBlock.appendChild(div);
        });
        passwordErrorBlock.style.display = 'block';
    } else {
        document.getElementById('checkPasswordInput').value = '';

        passwordErrorBlock.textContent = "Что-то пошло не так, попробуйте еще раз";
        passwordErrorBlock.style.display = 'block';
    }
});

document.getElementById('updatePasswordSubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const updatePasswordForm = document.getElementById('updatePasswordForm');
    const formData = new FormData(updatePasswordForm);

    const response = await fetch(updatePasswordForm.action, { method: 'POST', body: formData });

    if (response.ok) {
        window.location.reload();
    } else if (response.status === 400) {
        document.getElementById('updatePasswordInput').value = '';
        document.getElementById('updatePasswordConfirmInput').value = '';

        const json = await response.json();
        passwordErrorBlock.innerHTML = '';

        json.errorMessages.forEach(error => {
            const div = document.createElement('div');
            div.textContent = error;
            passwordErrorBlock.appendChild(div);
        });
        passwordErrorBlock.style.display = 'block';
    } else {
        document.getElementById('updatePasswordInput').value = '';
        document.getElementById('updatePasswordConfirmInput').value = '';

        passwordErrorBlock.textContent = "Что-то пошло не так, попробуйте еще раз";
        passwordErrorBlock.style.display = 'block';
    }
});

const passwordModalOverlay = document.getElementById('passwordModalOverlay');
passwordModalOverlay.addEventListener('click', e => {
    if (e.target === passwordModalOverlay) {
        passwordModalOverlay.style.display = 'none';
    }
});
document.getElementById('openPasswordModalBtn').addEventListener('click', () => {
    passwordModalOverlay.style.display = 'flex';
});