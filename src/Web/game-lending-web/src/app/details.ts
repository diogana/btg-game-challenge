import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ApiService } from './api';
import { GameDetails, FriendSummary, Page, Loan } from './models';
import { errorMessage } from './auth';
@Component({selector:'app-details',imports:[RouterLink,DatePipe],template:`
<a [routerLink]="kind==='game'?'/games':'/friends'">← Voltar</a>
@if(error()){<p class="error" role="alert">{{error()}}</p>}
@if(game();as d){<div class="page-title"><div><p class="eyebrow">DETALHES DO JOGO</p><h1>{{d.game.title}}</h1><p class="lead">{{d.game.friendName?'Com '+d.game.friendName:d.game.status==='Inactive'?'Desativado':'Disponível para emprestar'}}</p></div></div>}
@if(friend();as d){<div class="page-title"><div><p class="eyebrow">SEU AMIGO</p><h1>{{d.friend.name}}</h1><p class="muted">{{d.friend.email}}</p><p>{{d.activeLoans.total}} empréstimos ativos</p></div></div>}
@if(history();as h){<section class="panel"><h2>Histórico de empréstimos</h2>@for(loan of h.items;track loan.id){<div class="list-row"><div><strong>{{loan.gameTitle}}</strong><small>{{loan.friendName}} · {{loan.loanedAt|date:'dd/MM/yyyy HH:mm'}}</small></div><span class="badge">{{loan.returnedAt?'Devolvido':'Em andamento'}}</span></div>}@empty{<p class="empty">Nenhum empréstimo registrado.</p>}<div class="pagination"><button class="quiet" [disabled]="page===1" (click)="page=page-1;load()">← Anterior</button><span>Página {{page}} · {{h.total}} registros</span><button class="quiet" [disabled]="page*20>=h.total" (click)="page=page+1;load()">Próxima →</button></div></section>}`})
export class DetailsPage {
  private readonly api=inject(ApiService);private readonly route=inject(ActivatedRoute);
  readonly kind=this.route.snapshot.data['kind'];readonly id=this.route.snapshot.paramMap.get('id')!;
  readonly game=signal<GameDetails|null>(null);readonly friend=signal<FriendSummary|null>(null);readonly history=signal<Page<Loan>|null>(null);readonly error=signal('');page=1;
  constructor(){this.load();}
  load(){this.error.set('');if(this.kind==='game')this.api.game(this.id,this.page).subscribe({next:x=>{this.game.set(x);this.history.set(x.history);},error:e=>this.error.set(errorMessage(e))});else this.api.friend(this.id,this.page).subscribe({next:x=>{this.friend.set(x);this.history.set(x.history);},error:e=>this.error.set(errorMessage(e))});}
}
