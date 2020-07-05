import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AppComponent } from './app.component';
import { AuthorizeGuardService } from './authorize-guard.service';


const routes: Routes = [
  { path: 'dashboard', component: DashboardComponent, canActivate:[AuthorizeGuardService]},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
