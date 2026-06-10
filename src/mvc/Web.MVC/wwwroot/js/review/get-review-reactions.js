const emailNotificationModal = document.getElementById('emailNotificationModal');
if (emailNotificationModal) {
    document.getElementById('closeEmailNotificationModalBtn').addEventListener('click', () => {
        emailNotificationModal.style.display = 'none';
    });
}

const likeI = document.getElementById('likeI');
const dislikeI = document.getElementById('dislikeI');
const likeBtn = document.getElementById('likeBtn');
const dislikeBtn = document.getElementById('dislikeBtn');
const showEmailConfirmationModalOnReaction = document.getElementById('showEmailConfirmationModalOnReaction').value === 'true';
if (showEmailConfirmationModalOnReaction) {
    likeBtn.addEventListener('click', async function () {
        emailNotificationModal.style.display = 'flex';
        await fetch('/settings/email/request-confirmation', { method: 'POST' });
    });
    dislikeBtn.addEventListener('click', async function () {
        emailNotificationModal.style.display = 'flex';
        await fetch('/settings/email/request-confirmation', { method: 'POST' });
    });
} else {
    likeBtn.addEventListener('click', async function () {
        if (dislikeBtn.classList.contains('is-active')) {
            dislikeI.classList.remove('fa-solid');
            dislikeI.classList.add('fa-regular');
            dislikeBtn.classList.remove('is-active');
            const dislikesCountSpan = document.getElementById('dislikesCountSpan');
            dislikesCountSpan.textContent = parseInt(dislikesCountSpan.textContent) - 1;
        }

        const likesCountSpan = document.getElementById('likesCountSpan');
        if (likeBtn.classList.contains('is-active')) {
            likeI.classList.remove('fa-solid');
            likeI.classList.add('fa-regular');
            likeBtn.classList.remove('is-active');
            likesCountSpan.textContent = parseInt(likesCountSpan.textContent) - 1;
        } else {
            likeI.classList.remove('fa-regular');
            likeI.classList.add('fa-solid');
            likeBtn.classList.add('is-active');
            likesCountSpan.textContent = parseInt(likesCountSpan.textContent) + 1;
        }

        const reviewId = this.getAttribute('data-reviewId');
        const reactionType = this.getAttribute('data-reactionType');

        const response = await fetch(`/reviews/${reviewId}/react?reactionType=${reactionType}`,
            {
                method: 'POST'
            });
        if (response.status === 401) {
            const encodedCurrentUrl = document.getElementById('encodedCurrentUrl').value;
            window.location.href = `/account/signin?returnUrl=${encodedCurrentUrl}`;
        }
    });
    dislikeBtn.addEventListener('click', async function () {
        if (likeBtn.classList.contains('is-active')) {
            likeI.classList.remove('fa-solid');
            likeI.classList.add('fa-regular');
            likeBtn.classList.remove('is-active');
            const likesCountSpan = document.getElementById('likesCountSpan');
            likesCountSpan.textContent = parseInt(likesCountSpan.textContent) - 1;
        }

        const dislikesCountSpan = document.getElementById('dislikesCountSpan');
        if (dislikeBtn.classList.contains('is-active')) {
            dislikeI.classList.remove('fa-solid');
            dislikeI.classList.add('fa-regular');
            dislikeBtn.classList.remove('is-active');
            dislikesCountSpan.textContent = parseInt(dislikesCountSpan.textContent) - 1;
        } else {
            dislikeI.classList.remove('fa-regular');
            dislikeI.classList.add('fa-solid');
            dislikeBtn.classList.add('is-active');
            dislikesCountSpan.textContent = parseInt(dislikesCountSpan.textContent) + 1;
        }

        const reviewId = this.getAttribute('data-reviewId');
        const reactionType = this.getAttribute('data-reactionType');

        const response = await fetch(`/reviews/${reviewId}/react?reactionType=${reactionType}`,
            {
                method: 'POST'
            });
        if (response.status === 401) {
            const encodedCurrentUrl = document.getElementById('encodedCurrentUrl').value;
            window.location.href = `/account/signin?returnUrl=${encodedCurrentUrl}`;
        }
    });
}