import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ApiService } from './api';
describe('API service',()=>{
 let http:HttpTestingController;let api:ApiService;
 beforeEach(()=>{TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});http=TestBed.inject(HttpTestingController);api=TestBed.inject(ApiService);});
 afterEach(()=>http.verify());
 it('uses library filters and pagination',()=>{api.library('Zelda','Available',2).subscribe();const req=http.expectOne(r=>r.url==='/api/v1/library');expect(req.request.params.get('search')).toBe('Zelda');expect(req.request.params.get('page')).toBe('2');req.flush({games:{items:[],total:0,pageNumber:2,pageSize:20},activeLoans:[]});});
 it('sends game and friend when borrowing',()=>{api.borrow('game','friend').subscribe();const req=http.expectOne('/api/v1/loans');expect(req.request.method).toBe('POST');expect(req.request.body).toEqual({gameId:'game',friendId:'friend'});req.flush({});});
 it('returns a loan through BFF',()=>{api.returnLoan('loan').subscribe();const req=http.expectOne('/api/v1/loans/loan/return');expect(req.request.method).toBe('POST');req.flush({});});
});
