using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Web.MVC.Constants;
using Web.MVC.DTOs.comment;
using Web.MVC.Models.Api_responses.comment;
using Web.MVC.Models.View_models.Comment;
using Web.MVC.Services;

namespace Web.MVC.Controllers
{
    public class CommentController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly DisplayConverter displayConverter;
        public CommentController(IHttpClientFactory httpClientFactory, DisplayConverter displayConverter)
        {
            this.httpClientFactory = httpClientFactory;
            this.displayConverter = displayConverter;
        }

        [Authorize(Roles = RoleNames.Verified)]
        [HttpPost]
        [Route("comments/add")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto model)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
                using StringContent jsonContent = new(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var addCommentResponse = await httpClient.PostAsync("/api/Comment/add", jsonContent);
                if (!addCommentResponse.IsSuccessStatusCode)
                {
                    if (addCommentResponse.StatusCode == HttpStatusCode.Forbidden)
                        ModelState.AddModelError(string.Empty, "Вы не можете выполнить данное действие. Узнать причину можно на странице профиля");
                    else
                        ModelState.AddModelError(string.Empty, "Что-то пошло не так");

                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(new { errors });
                }

                string userIdStr = User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                Guid userId = new Guid(userIdStr);
                return RedirectToAction("GetUserById", "User", new { userId }); //сделать редирект на страницу с предложенными комментариями
            }

            return BadRequest(new { errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
        }


        [Route("reviews/{reviewId}/comments")]
        public async Task<IActionResult> GetCommentsByReviewId([FromRoute] Guid reviewId, int pageSize, int pageNumber)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
            var commentsResponse = await httpClient.GetAsync(
                $"/api/Comment/get-by-review-id/{reviewId}?pageSize={pageSize}&pageNumber={pageNumber}");
            commentsResponse.EnsureSuccessStatusCode();
            var commentsResult = await commentsResponse.Content.ReadFromJsonAsync<CommentsResultResponse>();

            List<CommentDisplay> commentsDisplay = await displayConverter.GetCommentListAsync(commentsResult!.Comments);
            var commentsResultDisplay = new CommentsResultDisplay
                { IsNextPageExisted = commentsResult.IsNextPageExisted, Comments = commentsDisplay };

            return new JsonResult(commentsResultDisplay);
        }

        [Route("comments/{commentId}/replies")]
        public async Task<IActionResult> GetCommentRepliesByCommentId([FromRoute] Guid commentId, int pageSize, int pageNumber)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
            var commentsResponse = await httpClient.GetAsync(
                $"/api/Comment/get-replies/{commentId}?pageSize={pageSize}&pageNumber={pageNumber}");
            commentsResponse.EnsureSuccessStatusCode();
            var commentsResult = await commentsResponse.Content.ReadFromJsonAsync<CommentsResultResponse>();

            List<CommentDisplay> commentsDisplay = await displayConverter.GetCommentListAsync(commentsResult!.Comments);
            var commentsResultDisplay = new CommentsResultDisplay
                { IsNextPageExisted = commentsResult.IsNextPageExisted, Comments = commentsDisplay };

            return new JsonResult(commentsResultDisplay);
        }

        [Authorize]
        [HttpPost]
        [Route("comments/{commentId}/remove")]
        public async Task<IActionResult> RemoveComment([FromRoute] Guid commentId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DefaultWithToken);
            var removeResponse = await httpClient.DeleteAsync($"/api/Comment/remove/{commentId}");
            removeResponse.EnsureSuccessStatusCode();

            return Ok();
        }
    }
}
