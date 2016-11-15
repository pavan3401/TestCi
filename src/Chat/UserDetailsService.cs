namespace Chat
{
  using ServiceStack;

  [Authenticate]
  public class UserDetailsService : Service
  {
    public object Get(GetUserDetails request)
    {
      var session = this.GetSession();
      return session.ConvertTo<GetUserDetailsResponse>();
    }
  }
}