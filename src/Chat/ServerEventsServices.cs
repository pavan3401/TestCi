namespace Chat
{
  using System.Linq;
  using System.Net;

  using ServiceStack;
  using ServiceStack.Configuration;

  public class ServerEventsServices : Service
  {
    public IServerEvents ServerEvents { get; set; }
    public IChatHistory ChatHistory { get; set; }
    public IAppSettings AppSettings { get; set; }

    public void Any(PostRawToChannel request)
    {
      if (!this.IsAuthenticated && this.AppSettings.Get("LimitRemoteControlToAuthenticatedUsers", false))
        throw new HttpError(HttpStatusCode.Forbidden, "You must be authenticated to use remote control.");

      // Ensure the subscription sending this notification is still active
      var sub = this.ServerEvents.GetSubscriptionInfo(request.From);
      if (sub == null)
        throw HttpError.NotFound($"Subscription {request.From} does not exist");

      // Check to see if this is a private message to a specific user
      var msg = PclExportClient.Instance.HtmlEncode(request.Message);
      if (request.ToUserId != null)
      {
        // Only notify that specific user
        this.ServerEvents.NotifyUserId(request.ToUserId, request.Selector, msg);
      }
      else
      {
        // Notify everyone in the channel for public messages
        this.ServerEvents.NotifyChannel(request.Channel, request.Selector, msg);
      }
    }

    public object Any(PostChatToChannel request)
    {
      // Ensure the subscription sending this notification is still active
      var sub = this.ServerEvents.GetSubscriptionInfo(request.From);
      if (sub == null)
        throw HttpError.NotFound("Subscription {0} does not exist".Fmt(request.From));

      var channel = request.Channel;

      // Create a DTO ChatMessage to hold all required info about this message
      var msg = new ChatMessage
                  {
                    Id = this.ChatHistory.GetNextMessageId(channel),
                    Channel = request.Channel,
                    FromUserId = sub.UserId,
                    FromName = sub.DisplayName,
                    Message = PclExportClient.Instance.HtmlEncode(request.Message),
                  };

      // Check to see if this is a private message to a specific user
      if (request.ToUserId != null)
      {
        // Mark the message as private so it can be displayed differently in Chat
        msg.Private = true;
        // Send the message to the specific user Id
        this.ServerEvents.NotifyUserId(request.ToUserId, request.Selector, msg);

        // Also provide UI feedback to the user sending the private message so they
        // can see what was sent. Relay it to all senders active subscriptions 
        var toSubs = this.ServerEvents.GetSubscriptionInfosByUserId(request.ToUserId);
        foreach (var toSub in toSubs)
        {
          // Change the message format to contain who the private message was sent to
          msg.Message = $"@{toSub.DisplayName}: {msg.Message}";
          this.ServerEvents.NotifySubscription(request.From, request.Selector, msg);
        }
      }
      else
      {
        // Notify everyone in the channel for public messages
        this.ServerEvents.NotifyChannel(request.Channel, request.Selector, msg);
      }

      if (!msg.Private)
        this.ChatHistory.Log(channel, msg);

      return msg;
    }

    public object Any(GetChatHistory request)
    {
      var msgs = request.Channels.Map(x =>
                                      this.ChatHistory.GetRecentChatHistory(x, request.AfterId, request.Take))
        .SelectMany(x => x)
        .OrderBy(x => x.Id)
        .ToList();

      return new GetChatHistoryResponse
               {
                 Results = msgs
               };
    }

    public object Any(ClearChatHistory request)
    {
      this.ChatHistory.Flush();
      return HttpResult.Redirect("/");
    }
  }
}