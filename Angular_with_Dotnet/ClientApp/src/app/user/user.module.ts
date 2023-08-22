import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserRoutingModule } from './user-routing.module';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { UserActivityComponent } from './user-activity/user-activity.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthGuardService } from '../services/auth-guard.service';


@NgModule({
  declarations: [
    UserProfileComponent,
    UserSettingsComponent,
    UserActivityComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    UserRoutingModule
  ],
  providers: [AuthGuardService]
})
export class UserModule { }
