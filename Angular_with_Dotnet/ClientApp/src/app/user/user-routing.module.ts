import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserActivityComponent } from './user-activity/user-activity.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { AuthGuardService } from '../services/auth-guard.service';


const routes: Routes = [
  { path: '', component: UserProfileComponent },
  { path: 'myaccount', component: UserProfileComponent, canActivate: [AuthGuardService] },
  { path: 'settings', component: UserSettingsComponent , canActivate: [AuthGuardService] },
  { path: 'activity', component: UserActivityComponent , canActivate: [AuthGuardService] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
