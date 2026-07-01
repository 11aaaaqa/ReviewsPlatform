using System.Text;
using System.Text.Json;
using Web.MVC.Constants;
using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.Api_responses.comment;
using Web.MVC.Models.View_models.Comment;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Services
{
    public class DisplayConverter(IHttpClientFactory httpClientFactory, ImageConverter imageConverter)
    {
        public async Task<List<CommentDisplay>> GetCommentListAsync(List<CommentResponse> comments)
        {
            List<Guid> userIds = comments.Select(x => x.UserId).ToList();

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Default);
            using StringContent jsonContent = new(JsonSerializer.Serialize(userIds), Encoding.UTF8, "application/json");
            var usersResponse = await httpClient.PostAsync("/api/User/get-users-by-ids", jsonContent);
            usersResponse.EnsureSuccessStatusCode();
            var users = await usersResponse.Content.ReadFromJsonAsync<List<UserResponse>>();

            List<UserDisplay> usersDisplay = new();
            foreach (var user in users!)
            {
                usersDisplay.Add(new UserDisplay
                {
                    Id = user.Id, Roles = user.Roles, Email = user.Email, IsAvatarDefault = user.IsAvatarDefault,
                    IsEmailVerified = user.IsEmailVerified, RegistrationDate = user.RegistrationDate,
                    UserName = user.UserName, AvatarSrc = imageConverter.GetImageSrc(user.AvatarSource)
                });
            }

            List<CommentDisplay> commentsDisplay = new();
            foreach (var comment in comments)
            {
                var user = usersDisplay.Single(x => x.Id == comment.UserId);
                commentsDisplay.Add(new CommentDisplay
                {
                    User = user, CommentStatus = comment.CommentStatus, ConsideredByUserId = comment.ConsideredByUserId,
                    CreatedAt = comment.CreatedAt, Id = comment.Id, ParentCommentId = comment.ParentCommentId,
                    RepliesCount = comment.RepliesCount, RejectionReason = comment.RejectionReason,
                    ReviewId = comment.ReviewId, Text = comment.Text
                });
            }
            return commentsDisplay;
        }
    }
}
