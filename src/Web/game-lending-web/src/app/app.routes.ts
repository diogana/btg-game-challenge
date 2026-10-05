import { Routes } from '@angular/router';
import { authGuard } from './auth';
export const routes:Routes=[
 {path:'login',loadComponent:()=>import('./login').then(m=>m.LoginPage)},
 {path:'dashboard',canActivate:[authGuard],loadComponent:()=>import('./dashboard').then(m=>m.DashboardPage)},
 {path:'games',canActivate:[authGuard],loadComponent:()=>import('./games').then(m=>m.GamesPage)},
 {path:'games/:id',canActivate:[authGuard],data:{kind:'game'},loadComponent:()=>import('./details').then(m=>m.DetailsPage)},
 {path:'friends',canActivate:[authGuard],loadComponent:()=>import('./friends').then(m=>m.FriendsPage)},
 {path:'friends/:id',canActivate:[authGuard],data:{kind:'friend'},loadComponent:()=>import('./details').then(m=>m.DetailsPage)},
 {path:'loans',canActivate:[authGuard],loadComponent:()=>import('./loans').then(m=>m.LoansPage)},
 {path:'',pathMatch:'full',redirectTo:'dashboard'}, {path:'**',redirectTo:'dashboard'}];
