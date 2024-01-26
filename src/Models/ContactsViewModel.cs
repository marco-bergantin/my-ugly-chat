namespace Auth0Mvc.Models;

public class ContactsViewModel
{
    public required IEnumerable<ContactViewModel> Contacts { get; set; }
    public ContactViewModel? SelectedContact { get; set; }
}

public class ContactViewModel
{
    public required string DisplayName { get; set; }
    public required string UserId { get; set; }
    public required string ChatId { get; set; }
    public DateTime TimestampLatestMessage { get; set; }
}
