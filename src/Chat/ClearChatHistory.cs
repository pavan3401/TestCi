namespace Chat
{
  using ServiceStack;

  [Route("/reset")]
  public class ClearChatHistory : IReturnVoid { }
}