import { Injectable } from '@angular/core';
import { AuthService } from "angularx-social-login";
import { FacebookLoginProvider } from "angularx-social-login";

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  constructor(private socialAuthService: AuthService) {
    this.socialAuthService.authState.subscribe((user) => {
      if(user){
      let userInfo = user;//save for reference 
      this.isUserLoggedIn = true;
      alert('Login Success');
      }
    });
   }

  public isUserLoggedIn:boolean = false

  public loginUser(){
    this.socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID);
  }

  public logout(){
    this.socialAuthService.signOut();
    this.isUserLoggedIn = false;
  }
}
