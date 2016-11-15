namespace Chat
{
  using ServiceStack;

  [Route("/channels/{Channel}/chat")]
  public class PostChatToChannel : IReturn<ChatMessage>
  {
    public string From { get; set; }
    public string ToUserId { get; set; }
    public string Channel { get; set; }
    public string Message { get; set; }
    public string Selector { get; set; }
  }
}