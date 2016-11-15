namespace Chat
{
  using System.Collections.Generic;

  public interface IChatHistory
  {
    long GetNextMessageId(string channel);

    void Log(string channel, ChatMessage msg);

    List<ChatMessage> GetRecentChatHistory(string channel, long? afterId, int? take);

    void Flush();
  }
}