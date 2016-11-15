namespace Chat
{
  using System.Collections.Generic;
  using System.Linq;

  using ServiceStack;

  public class MemoryChatHistory : IChatHistory
  {
    public int DefaultLimit { get; set; }

    public IServerEvents ServerEvents { get; set; }

    public MemoryChatHistory()
    {
      this.DefaultLimit = 100;
    }

    Dictionary<string, List<ChatMessage>> MessagesMap = new Dictionary<string, List<ChatMessage>>();

    public long GetNextMessageId(string channel)
    {
      return this.ServerEvents.GetNextSequence("chatMsg");
    }

    public void Log(string channel, ChatMessage msg)
    {
      List<ChatMessage> msgs;
      if (!this.MessagesMap.TryGetValue(channel, out msgs))
        this.MessagesMap[channel] = msgs = new List<ChatMessage>();

      msgs.Add(msg);
    }

    public List<ChatMessage> GetRecentChatHistory(string channel, long? afterId, int? take)
    {
      List<ChatMessage> msgs;
      if (!this.MessagesMap.TryGetValue(channel, out msgs))
        return new List<ChatMessage>();

      var ret = msgs.Where(x => x.Id > afterId.GetValueOrDefault())
        .Reverse()  //get latest logs
        .Take(take.GetValueOrDefault(this.DefaultLimit))
        .Reverse(); //reverse back

      return ret.ToList();
    }

    public void Flush()
    {
      this.MessagesMap = new Dictionary<string, List<ChatMessage>>();
    }
  }
}