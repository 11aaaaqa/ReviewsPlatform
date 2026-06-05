if (ratingInput.value)
    highlightStars(ratingInput.value);

stars.forEach(star => {
    star.addEventListener('mouseenter', function () {
        const value = star.dataset.value;
        highlightStars(value);
    });
    star.addEventListener('mouseleave', function () {
        highlightStars(ratingInput.value);
    });
    star.addEventListener('click', function () {
        ratingInput.value = star.dataset.value;
        highlightStars(star.dataset.value);
    });
});

function highlightStars(rating) {
    stars.forEach(star => {
        if (star.dataset.value <= rating) {
            star.classList.add('selected');
        } else {
            star.classList.remove('selected');
        }
    });
}