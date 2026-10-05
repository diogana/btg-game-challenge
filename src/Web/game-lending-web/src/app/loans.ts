import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { ApiService } from './api';
import { Friend, Game, Loan, Page } from './models';
import { errorMessage } from './auth';
@Component({selector:'app-loans',imports:[FormsModule,DatePipe],template:`
<div class="page-title"><div><p class="eyebrow">QUEM ESTÁ COM O QUÊ</p><h1>Empréstimos</h1></div></div>
@if(error()){<p class="error" role="alert">{{error()}}</p>}@if(success()){<p class="success" role="status">{{success()}}</p>}
<form class="panel" (ngSubmit)="borrow()"><h2>Novo empréstimo</h2><div class="two-columns"><div><label for="gameSearch">Busque um jogo disponível</label><div class="search-line"><input id="gameSearch" name="gameSearch" [(ngModel)]="gameSearch" placeholder="Nome do jogo"><button type="button" class="secondary" (click)="searchGames()">Buscar jogos</button></div><select aria-label="Selecionar jogo" data-cy="loan-game" name="gameId" [(ngModel)]="gameId" required><option value="">Selecione um jogo</option>@for(game of games();track game.id){<option [value]="game.id">{{game.title}}</option>}</select><small>Até 100 resultados. Refine a busca para localizar outros jogos.</small></div>
<div><label for="friendSearch">Busque um amigo</label><div class="search-line"><input id="friendSearch" name="friendSearch" [(ngModel)]="friendSearch" placeholder="Nome do amigo"><button type="button" class="secondary" (click)="searchFriends()">Buscar amigos</button></div><select aria-label="Selecionar amigo" data-cy="loan-friend" name="friendId" [(ngModel)]="friendId" required><option value="">Selecione um amigo</option>@for(friend of friends();track friend.id){<option [value]="friend.id">{{friend.name}}</option>}</select><small>Até 100 resultados. Refine a busca para localizar outros amigos.</small></div></div><button data-cy="borrow" [disabled]="!gameId || !friendId || busy()">Confirmar empréstimo →</button></form>
<section class="panel"><div class="section-title"><h2>{{active?'Empréstimos ativos':'Histórico completo'}}</h2><button class="quiet" (click)="active=!active;page=1;load()">{{active?'Ver histórico':'Ver ativos'}}</button></div>
@if(loading()){<p role="status">Carregando empréstimos…</p>}
@if(data();as d){@for(loan of d.items;track loan.id){<div class="list-row" data-cy="loan-row"><div><strong>{{loan.gameTitle}}</strong><small>Com {{loan.friendName}} · {{loan.loanedAt|date:'dd/MM/yyyy HH:mm'}}</small></div>@if(!loan.returnedAt){<button class="secondary" [disabled]="busy()" (click)="returnLoan(loan)">Devolver</button>}@else{<span class="badge">Devolvido em {{loan.returnedAt|date:'dd/MM/yyyy'}}</span>}</div>}@empty{<p class="empty">Nenhum empréstimo nesta lista.</p>}<div class="pagination"><button class="quiet" [disabled]="page===1" (click)="page=page-1;load()">← Anterior</button><span>{{d.total}} registros · Página {{page}}</span><button class="quiet" [disabled]="page*20>=d.total" (click)="page=page+1;load()">Próxima →</button></div>}</section>`})
export class LoansPage {
  private readonly api=inject(ApiService);readonly data=signal<Page<Loan>|null>(null);readonly games=signal<Game[]>([]);readonly friends=signal<Friend[]>([]);
  readonly error=signal('');readonly success=signal('');readonly busy=signal(false);readonly loading=signal(false);
  gameSearch='';friendSearch='';gameId='';friendId='';active=true;page=1;
  constructor(){this.load();this.searchGames();this.searchFriends();}
  load(){this.loading.set(true);this.api.loans(this.active,this.page).pipe(finalize(()=>this.loading.set(false))).subscribe({next:x=>this.data.set(x),error:e=>this.error.set(errorMessage(e))});}
  searchGames(){this.api.library(this.gameSearch,'Available',1,100).subscribe({next:x=>{this.games.set(x.games.items);this.gameId='';},error:e=>this.error.set(errorMessage(e))});}
  searchFriends(){this.api.friends(this.friendSearch,1,100).subscribe({next:x=>{this.friends.set(x.items);this.friendId='';},error:e=>this.error.set(errorMessage(e))});}
  borrow(){if(!this.gameId || !this.friendId)return;this.error.set('');this.success.set('');this.busy.set(true);this.api.borrow(this.gameId,this.friendId).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>{this.success.set('Empréstimo registrado.');this.page=1;this.load();this.searchGames();},error:e=>this.error.set(errorMessage(e))});}
  returnLoan(loan:Loan){this.error.set('');this.success.set('');this.busy.set(true);this.api.returnLoan(loan.id).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>{this.success.set('Jogo devolvido e disponível novamente.');this.load();this.searchGames();},error:e=>this.error.set(errorMessage(e))});}
}
