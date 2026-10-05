import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ApiService } from './api';
import { LoginPage } from './login';
import { DashboardPage } from './dashboard';
import { GamesPage } from './games';
import { FriendsPage } from './friends';
import { LoansPage } from './loans';
describe('Screens',()=>{
 beforeEach(()=>{sessionStorage.clear();TestBed.configureTestingModule({providers:[provideRouter([]),provideHttpClient()]});});
 it('login rejects whitespace credentials',()=>{const page=TestBed.createComponent(LoginPage).componentInstance;page.form.setValue({clientId:' ',clientSecret:'secret'});expect(page.form.invalid).toBe(true);});
 it('dashboard displays returned counts',async()=>{
   TestBed.overrideProvider(ApiService,{useValue:{dashboard:()=>of({statistics:{totalGames:12,availableGames:10,borrowedGames:2,totalFriends:3,activeLoans:2},activeLoans:[],recentLoans:[]})}});
   const fixture=TestBed.createComponent(DashboardPage);fixture.detectChanges();expect(fixture.nativeElement.textContent).toContain('12');expect(fixture.nativeElement.textContent).toContain('Disponíveis');
 });
 it('games renders current borrower',()=>{
   TestBed.overrideProvider(ApiService,{useValue:{library:()=>of({games:{items:[{id:'g',title:'Game',platforms:['PS5'],status:'Borrowed',friendName:'Alice'}],total:1},activeLoans:[]})}});
   const fixture=TestBed.createComponent(GamesPage);fixture.detectChanges();expect(fixture.nativeElement.textContent).toContain('Com Alice');
 });
 it('friends rejects invalid email',()=>{TestBed.overrideProvider(ApiService,{useValue:{friends:()=>of({items:[],total:0})}});const page=TestBed.createComponent(FriendsPage).componentInstance;page.form.setValue({name:'Alice',email:'bad'});expect(page.form.invalid).toBe(true);});
 it('loan return refreshes available games',()=>{
   const returnLoan=vi.fn(()=>of({}));const library=vi.fn(()=>of({games:{items:[]}}));
   TestBed.overrideProvider(ApiService,{useValue:{loans:()=>of({items:[],total:0}),friends:()=>of({items:[]}),library,returnLoan}});
   const page=TestBed.createComponent(LoansPage).componentInstance;page.returnLoan({id:'loan',gameId:'g',gameTitle:'Game',friendId:'f',friendName:'Alice',loanedAt:'2026-01-01',returnedAt:null});expect(returnLoan).toHaveBeenCalledWith('loan');expect(library).toHaveBeenCalledTimes(2);expect(page.success()).toContain('disponível');
 });
 it('dashboard exposes loading failure',()=>{TestBed.overrideProvider(ApiService,{useValue:{dashboard:()=>throwError(()=>({error:{title:'Service unavailable'}}))}});const fixture=TestBed.createComponent(DashboardPage);fixture.detectChanges();expect(fixture.nativeElement.textContent).toContain('Service unavailable');});
});
