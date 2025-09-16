using System;

namespace Case3MessengerBackend
{
    public class UserChat
    {
            public int UserId { get; set; }
            public User User { get; set; }

            public int ChatId { get; set; }
            public Chat Chat { get; set; }
    }
}
