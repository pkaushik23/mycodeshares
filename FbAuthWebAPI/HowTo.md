## Securing ASP.NET Core 3.X Api with JWT Authentication

In my last post - [Facebook Authentication in an Angular application][1],   I tried securing my Angular SPA with social login. The intent was only users authenticated by Facebook will be allowed to access the application.

A SPA would generally involve calling an API endpoint to fetch data or update some values. Therefore, naturally the next task for me was to have an API, and access the endpoint securely.

> I do not want to secure my API with cookies, instead I wanted to use JWT to secure the endpoint.

**Source Code**: Check source code of this learning post at [GitHub repo][2].

### Here is what we are going to do in this post:

1. Create an ASP.NET Core Api
2. Implement JWT Authentication

### Create an ASP.NET Core API

Before we begin, **Lets setup the project**.

1. From VS 2019, create a sample *ASP.NET Core Web Application*.
	1. For this post, I Created a *API* project.
	2. Do not enable *Authentication*, and it's value should be *No Authentication*. **Why**? Because enabling it will bring in [ASP.NET Core Identity][3] API which we don't want. 

At this point, we have a basic API Project setup, with a basic controller called 'WeatherForecastController'. If we run this application and try to access URL *https://localhost:44332/weatherforecast* , then we should see results in JSON.

### Implement JWT Authentication

As part of this post, we want to implement a use case that only authorized users will be able to access our endpoint. To prevent anonymous access to our endpoint, we must use *AuthorizeAttribute*.

There are two ways to decorate with *AuthorizeAttribute*, either decorate the controller class or decorate specific method. When we decorate at the class level, all the endpoint methods will have its effect. However, we can exclude few actions from it by having a *AllowAnonymousAttribute*. Other approach is to have the *AuthorizeAttribute* at individual method level. We are going to take the class approach. 

Decorate *WeatherForecastController* with *AuthorizeAttribute* as follows. It would require reference to *Microsoft.AspNetCore.Authorization* namespace.
```
[ApiController]
[Route("[controller]")]
[Authorize]
public class WeatherForecastController : ControllerBase
{
...
}
```

Now, if we try to access URL *https://localhost:44332/weatherforecast*, we were expecting to see some UnAuthorized access error.Instead, we see following
>InvalidOperationException: No authenticationScheme was specified, and there was no DefaultChallengeScheme found. The default schemes can be set using either AddAuthentication(string defaultScheme) or AddAuthentication(Action<AuthenticationOptions> configureOptions).

Here, the exceptions is clearly telling us that we don't have a *authenticationScheme* configured.  Why do we need this? Because there should be a way for the application to first identify the user who is trying to access the application(**Authentication**) and then determine whether that authenticated user can access a resource (**Authorization**). Because, we decorated our class/endpoint with *AuthorizeAttribute* but there is no way to authenticate the user, hence the exception. So, the next step will be to configure for *Authentication*.

#### Implement Authentication

For this, we are going to expose a POST endpoint which will accept a dummy username/password and return a JWT token. The new endpoint which we are going to expose will be *anonymously* accessible. Why? because the first time when a user tries to login, he/she is not authenticated yet and we need a way for them to be authenticated and acquire a token. Think of it as a user trying to login and server issuing a cookie, which can be used for subsequent requests. The cookie is validated for each request and if valid, then the user is considered a valid user. Here, instead of cookie we will issue a JWT token.

Create a new API Controller as *UserController*, and don't decorate it with *AuthorizeAttribute*.

```
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
}
```
Expose an anonymously accessible endpoint as following
```
[HttpPost("login")]
[AllowAnonymous]
public IActionResult Login([FromBody] string username)
{
	if(username == "Prerak")
	{
		return Ok($"{username} logged in");
	}
	else
	{
		return BadRequest("Invalid User");
		
	}
}
```
If we try to access ```POST https://localhost:44332/api/user/login ``` in [Postman][4] as follows, we would see a successful response. For the sake of simplicity, we are saying - If a user is known, we say it is authenticated.

![Alt][6]

Next, we will be issuing a token on successful login.

First, install a package : ```install-package System.IdentityModel.Tokens.Jwt```.
We will have a helper method, which will generate a token. Following is the code.
```
[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] string username)
{
	if(username == "Prerak")
	{
		var token = await GenerateJwtToken(username);
		return Ok(new { user = username, token = token});
	}
	else
	{
		return BadRequest("Invalid User");
	}
}

private async Task<string> GenerateJwtToken(string username)
{
	var someSecret = "a random string which should come from appsettings";
	List<Claim> claims = new List<Claim>() {
		new Claim(ClaimTypes.Name,username),
		new Claim(ClaimTypes.Role,"User"),
	   
	};
	var jwtTokenHandler = new JwtSecurityTokenHandler();
	var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(someSecret));
	var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
	JwtSecurityToken SecurityToken = new JwtSecurityToken(
		issuer:"myapi.com",
		audience: "myapi.com",
		claims: claims,
		expires: DateTime.Now.AddDays(7),
		signingCredentials: credentials
		);
	  
	return await Task.Run<string>(() => {
		return jwtTokenHandler.WriteToken(SecurityToken);
	});
}
```
We will now try to understand what is going on in our *GenerateJwtToken* helper method. The main things to focus are:

1.  **SymmetricSecurityKey** and **SigningCredentials** are used to get a signing credential, which will be used to sign the **JwtSecurityToken**.
2. **JwtSecurityTokenHandler** gets string representation of a **JwtSecurityToken**
3. **JwtSecurityToken** class is the main actor here, which contains configuration for a JWT security token like - Issuer of the token, token  is issued for which resource (the audience), claims, expiry of the token and signing information.

Let's try to access this endpoint ```POST https://localhost:44332/api/user/login ``` which would get us following result.
```
{"user":"Prerak",
"token":"...base64 encoded jwt string..."}
```

At this point, we are ready to issue JWT tokens. Next, task is to validate this token when user tries to access our URL *https://localhost:44332/weatherforecast*.

#### Configure API to validate JWT tokens

In this section we will configure our API with a [Authentication Scheme][7] to **Authenticate** the incoming request which contains the JWT token. The *Authentication Scheme* that we will be using is '*JwtBearerDefaults.AuthenticationScheme*'.

For this part, first we need to install a package - *Microsoft.AspNetCore.Authentication.JwtBearer*.

We will make changes to our *startup.cs* file's ```ConfigureServices``` as follows, note *AddAuthentication* and *AddJwtBearer*: 

```
public void ConfigureServices(IServiceCollection services)
{
	services.AddControllers();
	services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.ClaimsIssuer = "myapi.com";
		options.Audience = "myapi.com";
		options.SaveToken = true;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			//Required else token will fail to be validated and auth will fail
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a very random string which should come from appsettings")),
			ValidateLifetime = true,
			ValidIssuer = "myapi.com",
			ValidateIssuer = true,
		};
		options.RequireHttpsMetadata = false;
	});
}
```

Plus, in *Configure* method **set middleware** of *Authentication* as ```app.UseAuthentication()```

Now, when we try to access the URL *https://localhost:44332/weatherforecast*, we should not see the error that Auth Scheme is not available. Instead, we will get a *401 Unauthorized* and a response header *WWW-Authenticate: Bearer*, indicating that a *Bearer* token is required to access the resource.

If we now get a fresh token by hitting the login endpoint and use the token received, we will be able to access our weather result.

```
//send token as authorization header in request.
Authorization: Bearer authtokenwereceievdfromtheloginendpoint
```

That is it for this post. thanks.


[1]: https://prerakkaushik.wordpress.com/2020/07/05/facebook-authentication-in-an-angular-application/
[2]: https://github.com/pkaushik23/mycodeshares/tree/master/FbAuthWebAPI
[3]: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-3.1&tabs=visual-studio
[4]: https://www.postman.com/
[5]: /seq.dig.png
[6]: https://prerakkaushik.files.wordpress.com/2020/06/postlogin1.png
[7]: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-3.1https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-3.1