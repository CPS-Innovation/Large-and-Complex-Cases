using System.Text.Json;

namespace CPS.ComplexCases.API.Functions.Tactical
{
  public static class HtmlHelpers
  {
    public static string LoginForm()
    {
      return string.Concat(@$"
                  <html>
                      <head>
                        <style>
                          body {{
                            font-family: Arial, sans-serif;
                          }}
                          div {{
                            margin: 100px auto;
                            width: 400px;
                          }}
                          input {{
                            margin-bottom: 20px;
                            width: 100%;
                            height: 4rem;
                            font-size: 2rem;
                            padding: 30px;
                          }}
                          select {{
                            margin-bottom: 20px;
                            width: 100%;
                            height: 3.5rem;
                            font-size:1.5rem;
                            padding: 15px;
                          }}
                          input[type=submit]{{
                            padding:0;
                          }}
                        </style>
                      </head>
                      <body>
                        <div>
                          <form method='post'>
                              <input name='username' placeholder='User name'/>
                              <br />
                              <input name='password' placeholder='Password' type='password' />
                              <br />
                              <input type='submit' value='Log in' />
                          </form>
                        </div>
                      </body>
                  </html>");
    }

    public static string LoginFormResult(string username, bool isLoggedInOk, string rawCookiesStrings)
    {
      return $@"
                <html>
                    <head>
                      <style>
                        body {{
                          font-family: Arial, sans-serif;
                        }}
                        div.user {{
                          margin: 100px auto;
                          width: 400px;
                          font-size: 2em;
                        }}        
                        div.feedback {{
                          width: 1400px;
                          margin: 20px auto;
                        }}  
                        .ok{{
                          color: green;
                        }}
                        .not-ok{{
                          color: red;
                        }}
                      </style>
                    </head>
                    <body>
                        <div class='user'>
                          Hi {username} <br>
                          We believe you {(isLoggedInOk
                            ? "<strong class='ok' data-testid='login-ok'>ARE</strong>"
                            : "<strong class='not-ok'>ARE NOT</strong>")} logged in to CMS<br/>
                        </div>
                        <div class='feedback'>
                          <code>
                            {JsonSerializer.Serialize(rawCookiesStrings)}
                          </code>
                        </div>
                    </body>
                </html>";
    }

    public static string FullCookieResult(string serializedCmsAuthValues, string modernUrl, string modernAuthToken)
    {
      return $@"
<html>
<body>
<pre>
Cms-Auth-Value Cookie =>
  {serializedCmsAuthValues}

Encoded Cms-Auth-Value Cookie => 
  {Uri.EscapeDataString(serializedCmsAuthValues)}
</pre>  
</body>
</html>";
    }
  }
}