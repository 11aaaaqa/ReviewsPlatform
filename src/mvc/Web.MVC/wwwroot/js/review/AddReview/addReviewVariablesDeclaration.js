const errorMessageContainer = document.getElementById('errorMessageContainer');
const reviewTextInput = document.getElementById('reviewTextInput');
const reviewBlock = document.getElementById('reviewBlock');
const shortReviewInput = document.getElementById('shortReviewInput');
const shortReviewBlock = document.getElementById('shortReviewBlock');
const reviewPicturesInput = document.getElementById('reviewPicturesInput');

const pictureExtensions = ['.jpg', '.jpeg', '.png'];
const picturesMaxSize = document.getElementById('picturesMaxSize').value;

const itemPictureInput = document.getElementById('itemPictureInput');
const itemPictureImg = document.getElementById('itemPictureImg');

const reviewTextMaxLength = document.getElementById('reviewTextMaxLength').value;
const shortReviewMaxLength = document.getElementById('shortReviewMaxLength').value;

const reviewPicturesPreviewBlock = document.getElementById('reviewPicturesPreviewBlock');
let filesArray = [];

const stars = document.querySelectorAll('.add-review-star');
const ratingInput = document.getElementById('reviewItemEstimationInput');