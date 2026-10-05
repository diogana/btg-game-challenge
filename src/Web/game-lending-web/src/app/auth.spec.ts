import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService, authGuard, authInterceptor } from './auth';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
describe('Authentication',()=>{
 let http:HttpTestingController;let auth:AuthService;
 beforeEach(()=>{sessionStorage.clear();TestBed.configureTestingModule({providers:[provideRouter([]),provideHttpClient(withInterceptors([authInterceptor])),provideHttpClientTesting()]});http=TestBed.inject(HttpTestingController);auth=TestBed.inject(AuthService);});
 afterEach(()=>{http.verify();sessionStorage.clear();});
 it('stores successful token without saving credentials',()=>{auth.login('demo','secret').subscribe();const request=http.expectOne('/api/v1/auth/token');expect(request.request.body).toEqual({clientId:'demo',clientSecret:'secret'});request.flush({accessToken:'token',tokenType:'Bearer',expiresIn:3600});expect(auth.token()).toBe('token');expect(sessionStorage.getItem('accessToken')).toBe('token');expect(sessionStorage.getItem('clientSecret')).toBeNull();});
 it('does not store token on failed login',()=>{auth.login('demo','secret').subscribe({error:()=>{}});http.expectOne('/api/v1/auth/token').flush({}, {status:401,statusText:'Unauthorized'});expect(auth.token()).toBeNull();});
 it('adds bearer only to owned API',()=>{auth.token.set('token');const client=TestBed.inject(HttpClient);client.get('/api/v1/games').subscribe();client.get('https://external.example/data').subscribe();const owned=http.expectOne('/api/v1/games');const external=http.expectOne('https://external.example/data');expect(owned.request.headers.get('Authorization')).toBe('Bearer token');expect(external.request.headers.has('Authorization')).toBe(false);owned.flush({});external.flush({});});
 it('clears session on API 401',()=>{auth.token.set('old');const navigate=vi.spyOn(TestBed.inject(Router),'navigate').mockResolvedValue(true);TestBed.inject(HttpClient).get('/api/v1/games').subscribe({error:()=>{}});http.expectOne('/api/v1/games').flush({}, {status:401,statusText:'Unauthorized'});expect(auth.token()).toBeNull();expect(navigate).toHaveBeenCalledWith(['/login']);});
 it('guard redirects unauthenticated navigation',()=>{const result=TestBed.runInInjectionContext(()=>authGuard({} as ActivatedRouteSnapshot,{} as RouterStateSnapshot));expect(String(result)).toBe('/login');});
});
