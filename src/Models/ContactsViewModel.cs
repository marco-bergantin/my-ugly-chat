using MyUglyChat.DAL;
using MyUglyChat.Services;

namespace MyUglyChat.Models;

public class ContactsViewModel
{
    public required IEnumerable<ContactViewModel> Contacts { get; set; }
    public ContactViewModel? SelectedContact { get; set; }

    public static ContactsViewModel FromContactList(ContactList contactList, string userId) => new()
    {
        Contacts = contactList?.Contacts?.OrderByDescending(c => c.TimestampLatestMessage)
                                         .Select(c => new ContactViewModel
                                         {
                                             DisplayName = c.DisplayName,
                                             UserId = c.UserId,
                                             ChatId = RecentChatsService.GetChatId(userId, c.UserId),
                                             TimestampLatestMessage = c.TimestampLatestMessage
                                         })
                    ?? []
    };
}

public class ContactViewModel
{
    public required string DisplayName { get; set; }
    public required string UserId { get; set; }
    public required string ChatId { get; set; }
    public DateTime TimestampLatestMessage { get; set; }
}
