## Facebook Authentication in ASP.Net Core without Identity

### Motivation:
>Implement social authentication without the [ASP.NET Core Identity](1) API.

While trying to implement social login for a sample *ASP.NET Core MVC application*, I went through the official Microsoft [documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-3.1). And, I must admit despite going through it mutiple times, I was not able to wrap my head around it. Either, perhaps the documentation is arranged in a manner which is more focussed towards [ASP.NET Core Identity](1) API or I was terribly confused. I think safe to assume later.

This blog is a result of one of my self learning exercise, I followed this help [document](2), but still many things were missing and hence this post. Therefore documenting it for later reference.

### Goals to  achieve:
We will be trying to achieve following, and solve issues as they arise:

#### 1. Enable basic Facebook Authentication
#### 2. Custom Page to list login options

**Source Code**: Check source code of this experiment at [GitHub repo.](4)   

Before we begin, **Lets setup the project**.

1. From VS 2019, create a sample *ASP.NET Core Web Application*.
	1. For this post, I Created a *Web Application (Model-View-Controller)*
	2. Do not enable *Authentication*, and it's value should be *No Authentication*. **Why**? Because enabling it will bring in [ASP.NET Core Identity](1) API which we dont want. 
2. We need to install following *Nuget* packages
	1. Microsoft.AspNetCore.Authentication.Cookies
	2. Microsoft.AspNetCore.Authentication.Facebook
3. [Add a new Facebook app](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-3.1). 
4. For the sake of this project, we will **secure** the *Privacy* endpoint of our *HomeController* with *AuthorizeAttribute* as follows.
	```
	[Authorize]
	public IActionResult Privacy()
	{
		return View();
	}
	```

### Enable basic Facebook Authentication


Following the [documentation](2), the basic configuraton is following in the *startup.cs* file's *ConfigureServices* method:
	
```
public void ConfigureServices(IServiceCollection services)
{
    ...
	
    services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddFacebook(FacebookDefaults.AuthenticationScheme,options =>
        {
            options.AppId = <you app id>;
            options.AppSecret = <your App Secret> ;
        });

    ...
}
```
And, enable *Authentication*, in the *Configure* method.
```
app.UseAuthentication();
```
**Important**: It is important to specify both the default authentication scheme and default Challenge scheme. Else, we will get an error which is something like this: Also, dont forgot to add *Cookie* auth scheme (```.AddCookie()```).
> *InvalidOperationException*: No authenticationScheme was specified, and there was no DefaultChallengeScheme found. The default schemes can be set using either AddAuthentication(string defaultScheme) or AddAuthentication(Action<AuthenticationOptions> configureOptions).

> **Why**: Because, these values will be used by Authentication middleware to check and follow rules defined for Authentication. For example, when an unauthorized user tries to access a secure endpoint then the challenge prompts user to sign in, here we are using **FacebookDefaults.AuthenticationScheme** for *Challenge*. More [here](3).

**Test**: Debug or Hit F5, and try to navigate to "https://localhost:44300/Home/Privacy", we will be immediately redirected to Facebook login screen for *Authentication*.

### Custom Page to list login options 

Up untill here we saw that, as soon as user tries to access resource "https://localhost:44300/Home/Privacy", the user is immediately taken to the FB login page. In this section we want to change this behaviour. Now, we want that if a user is not authenticated, then he should see our custom login page. This login page should display all the options which are available for login. For now, we have FB, but it will serve the purpose.

1. Lets create a new Controller (AccountController) and its associated views.
	1. Create an action, named *Login*
	2. Create View for the *Login* Action
		```
		//controller code
		public IActionResult Login(string ReturnUrl)
		{
				ViewData["ReturnUrl"] = ReturnUrl;
				return View();
		}
		//view template
		<div class="text-center">
			<h3>Pleasse Login</h3>
			<a asp-action="FbLogin"  asp-route-ReturnUrl="@ViewData["ReturnUrl"]">Facebook Login</a>
		</div>
		```
2. Hook this *Login* action method in our *ConfigureServices* method for *Startup.cs* file. Replace ```.AddCookie()``` with following code, here we are configuring that user should be redirected to *"/Account/Login/"* when it is not authenticated.
	```
	.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
		{
			options.LoginPath = new PathString("/Account/Login/");
			
		})
	```
3. Remove *DefaultChallengeScheme* from our *Authentication Service* configuration. This is a **very important** step. **Why**? Becasue, we dont want the user to be redirected to FB login page yet. Instead, We want user to first go to our page which lists all the authentication provider configured for the application;Step 1, above. See snippet for the Login view.
4. Next, last but again an important step. You must be wondering, that we removed the *FB challenge* from our service configuration then how user will be able to login. Here we should create an action *FbLogin*, which when called will raise a *Challenge*.
	```
	public async Task FbLogin(string ReturnUrl)
		{
			await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme);
		}
	```
	
Now, If you try to run application, and try to access the secure route, user would be redirected to *"https://localhost:44314/Account/Login/?ReturnUrl=%2FHome%2FPrivacy"*. On that view, user will see link for Fb Login, clicking on which, we expect that user should be redirected to Facebook login page, Right?. But instead we will see following error @ *"https://localhost:44314/signin-facebook?code=someEncodedCodeHere"*:

> Exception: **Correlation failed**.

> Unknown location

> Exception: An error was encountered while handling the remote login.
Microsoft.AspNetCore.Authentication.RemoteAuthenticationHandler<TOptions>.HandleRequestAsync()

**How to solve this?** The reason for this error is, we did not specify the return url where user should be redirected to, once authentication is complete. Remember, we were trying to access *"https://localhost:44300/Home/Privacy"*. So, thats is where user shoulg go back.  Making following changes would fix above error.

```
public async Task FbLogin(string ReturnUrl)
{
	await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, new AuthenticationProperties()
	{
		RedirectUri = new PathString(ReturnUrl)
	});
}
```
**Test**: Debug or Hit F5, and try to navigate to *"https://localhost:44300/Home/Privacy"*, from Login page, after clicking on Fb Login link, we will be redirected to Facebook login screen for *Authentication*. And, once that is done, we should be able to access *"https://localhost:44300/Home/Privacy"*

Thanks.

---

[1]:https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-3.1&tabs=visual-studio
[2]:https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/social-without-identity?view=aspnetcore-3.1
[3]:https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-3.1#authentication-concepts
[4]:https://github.com/pkaushik23/mycodeshares/tree/master/FbLoginMvcWebApp
