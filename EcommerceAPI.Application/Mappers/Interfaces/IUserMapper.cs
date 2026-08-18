using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IUserMapper
    {

        public void UpdateUserFromRequest(User user, UpdateProfileRequest request);

        /// <summary>Add a comment on  lines R9 to R10Add diff commentMarkdown input:  edit mode selected.WritePreviewAdd a suggestionHeadingBold(control b) control⌃ bBItalic(control i) control⌃ iIQuote(control shift right angle bracket) control⌃ shift⇧ right angle bracket>Code(control e) control⌃ eELink(control k) control⌃ kKUnordered list(control 8) control⌃ 88Numbered list(control shift ampersand) control⌃ shift⇧ ampersand&Task list(control shift l) control⌃ shift⇧ lLMentionReferenceMore itemsSaved repliesAdd FilesPaste, drop, or click to add filesCancelCommentStart a review
        /// Maps a User entity to a UserResponse DTO.
        /// </summary>
        /// <param name="user">The User entity to map.</param>
        /// <returns>The mapped UserResponse DTO.</returns>
        public UserResponse ToUserResponse(User user);
    }
}