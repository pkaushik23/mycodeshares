## Implement Facebook login in an Angular application

This post is an outcome of the next learning task which I took upon myself for my self learning series. In my last post -  [Facebook Authentication in ASP.Net Core without Identity](1), I tried securing .Net Core Web application with social login. The intent was only users authenticated by Facebook will be allowed to access the application.

In this post, we will do the same with an Angular SPA.

**Source Code**: Check source code of this learnig post at [GitHub repo][4]. 

## Here is what we are going to do in this post:

1. Use Angular CLI to create an application.
2. Implement Social login. 


### Use Angular CLI to create a application
We are not going to discuss much here as this is outside the scope of the this learning post.

Run following commands and follow the wizard that follows. When asked,  please enable routing.
```
ng new NgSocialLoginSpa
```
*NgSocialLoginSpa* is a fictious app name I have used. For detailed help, please follow official [Angular Documentation](2).

Run application by executing following command:
```
ng serve
```
When you try to browse - http://localhost:4200/ ,you would see something like following. At this point, we have a basic Angular SPA ready with routing  enabled.

![Alt][3]

### Implement Social login

For the purpose of this post, we will try to do few things:

1. Add a *DashboardComponent*, which will be accessible only for Authorized users (Authenticated by Facebook).
2. Configure route links for user to access our new *Dashboard*
3. Add a *LoginService*, which would be our central service to maintain login related stuff.
4. Add a Route Guard, so that we can write some logic for Authorization.
5. Enable  Facebook Authentication

Feel free to skip to step: Enable Facebook Authentication, if you are already done with basic infrastructure.

#### 1. Add a *DashboardComponent*, which would be accessible only for Authorized users (Authenticated by Facebook)
```
ng g c dashboard //or ng generate component dashboard
```
#### 2. Configure route links for user to access our new *Dashboard*

1. Open file *src\app\app-routing.module.ts*, and make changes to the routes array as follows, and resolve reference errors:
```
	const routes: Routes = [
		{ path: 'dashboard', component: DashboardComponent 
	];
```

2. Open file *src\app\app.component.html*, and above ```<h2>Resources</h2>```, paste following lines:
```
	<div class="card-container">
		<nav>
		  <a routerLink="/dashboard">Dashboard</a> 
		</nav>
	</div>
```
Also move ```<router-outlet></router-outlet>``` just after the above template.

At this point, on the home page you would see a *Dashboard* link and clicking on which will show its content.
![Alt][6]

![Alt][7]

For more information check [Add Routing](5)
#### 3. Add a *LoginService*, which would be our central service to maintain login related state and behaviour.
Now, we want to create a service which will have state of the user(whether logged in or not) and also method for login and logout.
Execute ng-cli command ``` ng generate service login``` to generate *LoginService*. Also, add following code. We will refine the below code when we implement FB Login.
```
	public isUserLoggedIn:boolean = false

	  public loginUser(){
		this.isUserLoggedIn = true;
		alert('Login Success');
	  }

	  public logout(){
		this.isUserLoggedIn = false;
	  }
```
Also, in the *src\app\app.component.html*, add following code:

```
	<div *ngIf="!loginSvc.isUserLoggedIn">
		<button (click)="loginSvc.loginUser()">SignIn</button>
	</div>
	<div *ngIf="loginSvc.isUserLoggedIn">
		<button (click)="loginSvc.logout()">Sign Out</button>
	</div>
```
At this step, we have laid down basic infrastructure to login a user and log out a user.

#### 4. Add a Route Guard so that we can write some logic for Authorization.
> A class that acts a guard deciding if a route can be activated or not.

A) Create a new service which will act as our route guard.

```
	ng generate service authorizeGuard
```

- The class needs to implement [CanActivate](8) interface.
- This service would be required to check if the user is logged in or not, therefore will also need reference to our Login Service

B) Replace the content of the class with following and resolve reference errors.
```
	@Injectable({
	  providedIn: 'root'
	})
	export class AuthorizeGuardService implements CanActivate {

	  constructor(private loginSvc: LoginService) { }
	  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
		if(this.loginSvc.isUserLoggedIn){
		  return true;
		} else {
		  alert('Please sign-in to access this resource');
		  return false;
		}

	  }
	}
```
C) Attach the route guard with our route configuration. Open *src\app\app-routing.module.ts* and change route for 'dashboard' to following, notice '*canActivate*':
```
	const routes: Routes = [
	  { path: 'dashboard', component: DashboardComponent, canActivate:[AuthorizeGuardService]},
	];
```

At this point, If we we try to access the url (http://localhost:4200/dashboard) without signing in then we will get an error *"Please sign-in to access this resource"*. But if signed in, we will be able to access the resource.

#### 4. Enable Facebook Authentication.

This is the step, for which the post is written and is coming last but at the same time, is the **simplest one**.

For this step, we are going to use an excellent npm package : ["angularx-social-login"](9). The package has a very good documentation and if you wish, you can see and learn from the source code and implement something on your own. Check its [github repo](10) as well. There is nothing much to focus as the [documentation](10) is very accurate.

What we are going to do next is , configure our *LoginService*, to make use of this package.

```
	npm install  angularx-social-login
```

A) Open **src\app\app.module.ts** and make following changes:

```
	import { SocialLoginModule,FacebookLoginProvider, AuthServiceConfig } from "angularx-social-login";
	...
	let config = new AuthServiceConfig([
	  {
		id: FacebookLoginProvider.PROVIDER_ID,
		provider: new FacebookLoginProvider("you fb app id")
	  }
	]);

	export function provideConfig() {
	  return config;
	}

	//Also change the provider array as following
	...

	 providers: [{
		provide: AuthServiceConfig,
		useFactory: provideConfig
	  }]
	...
	
	//Also include 'SocialLoginModule' in the imports
	imports: [
		BrowserModule,
		AppRoutingModule,
		SocialLoginModule
	],
	...
```
B) Open **src\app\login.service.ts** and make following changes:

* Inject dependency  
```	private socialAuthService: AuthService```
* Update method *loginUser()*
```
	public loginUser(){
		this.socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID);
	}
```
* In constructor, Subscribe to user-info when login completes
```
	this.socialAuthService.authState.subscribe((user) => {
	  if(user){
	  let userInfo = user;//save for reference 
	  this.isUserLoggedIn = true;
	  }
	});
```
* Tweak method *logout()*
```
	public logout(){
		this.socialAuthService.signOut();
		this.isUserLoggedIn = false;
	}
```

That's it folks, our SPA is ready with Facebook Authentication.
It is a very raw implementation, but enough to give us a start for something bigger.  Thanks.

[1]: https://prerakkaushik.wordpress.com/2020/06/21/facebook-authentication-in-asp-net-core-without-identity/
[2]: https://angular.io/guide/setup-local
[3]: https://prerakkaushik.files.wordpress.com/2020/07/appstart.png?w=700  "Angular SPA"
[4]:https://github.com/pkaushik23/mycodeshares/tree/master/NgSocialLoginSpa
[5]:https://prerakkaushik.wordpress.com/2020/06/13/add-routing-to-existing-anguar-project/
[6]: https://prerakkaushik.files.wordpress.com/2020/07/dashboardlink.png "dashboard link" 
[7]: https://prerakkaushik.files.wordpress.com/2020/07/dashboardcontent.png  "dashboard content" 
[8]: https://angular.io/api/router/CanActivate
[9]: https://www.npmjs.com/package/angularx-social-login/v/2.2.1
[10]: https://github.com/abacritt/angularx-social-login
