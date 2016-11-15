namespace Chat
{
  using System.Collections.Generic;

  using ServiceStack;

  public class GetChatHistoryResponse
  {
    public List<ChatMessage> Results { get; set; }
    public ResponseStatus ResponseStatus { get; set; }
  }
}