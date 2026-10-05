import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { TokenResponse } from './models';
export const API='/api/v1';
@Injectable({providedIn:'root'})
export class AuthService {
  private readonly http=inject(HttpClient);
  private readonly router=inject(Router);
  readonly token=signal(sessionStorage.getItem('accessToken'));
  login(clientId:string,clientSecret:string){
    return this.http.post<TokenResponse>(`${API}/auth/token`,{clientId,clientSecret}).pipe(tap(result=>{
      sessionStorage.setItem('accessToken',result.accessToken);this.token.set(result.accessToken);
    }));
  }
  logout(){sessionStorage.removeItem('accessToken');this.token.set(null);void this.router.navigate(['/login']);}
}
export const authInterceptor:HttpInterceptorFn=(request,next)=>{
  const auth=inject(AuthService);
  const owned=request.url.startsWith(API+'/');
  const token=auth.token();
  if(owned && token && !request.url.endsWith('/auth/token'))request=request.clone({setHeaders:{Authorization:`Bearer ${token}`}});
  return next(request).pipe(catchError((error:HttpErrorResponse)=>{
    if(owned && error.status===401 && !request.url.endsWith('/auth/token'))auth.logout();
    return throwError(()=>error);
  }));
};
export const authGuard:CanActivateFn=()=>inject(AuthService).token() ? true : inject(Router).createUrlTree(['/login']);
export function errorMessage(error:HttpErrorResponse):string {
  return error.status===403 ? 'Você não tem permissão para essa operação.' : error.error?.detail ?? error.error?.title ?? 'Não foi possível concluir a operação. Tente novamente.';
}
