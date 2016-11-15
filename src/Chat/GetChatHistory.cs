namespace Chat
{
  using ServiceStack;

  [Route("/chathistory")]
  public class GetChatHistory : IReturn<GetChatHistoryResponse>
  {
    public string[] Channels { get; set; }
    public long? AfterId { get; set; }
    public int? Take { get; set; }
  }
}