import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from './api';
import { Dashboard } from './models';
import { errorMessage } from './auth';
@Component({selector:'app-dashboard',imports:[DatePipe,RouterLink],template:`
<div class="page-title"><div><p class="eyebrow">SUA BIBLIOTECA EM DIA</p><h1>Visão geral</h1><p class="muted">Cada jogo no lugar certo. Mesmo quando está com um amigo.</p></div><a class="button" routerLink="/loans">+ Novo empréstimo</a></div>
@if(error()){<p class="error" role="alert">{{error()}}</p><button (click)="load()">Tentar novamente</button>}
@else if(!data()){<p role="status">Carregando sua biblioteca…</p>}
@if(data();as d){<div class="stats"><article class="panel"><span>Total de jogos</span><strong>{{d.statistics.totalGames}}</strong></article><article class="panel"><span>Disponíveis</span><strong class="accent">{{d.statistics.availableGames}}</strong></article><article class="panel"><span>Emprestados</span><strong>{{d.statistics.borrowedGames}}</strong></article><article class="panel"><span>Amigos</span><strong>{{d.statistics.totalFriends}}</strong></article></div>
<section class="panel"><div class="section-title"><h2>Com seus amigos <span class="badge">{{d.statistics.activeLoans}}</span></h2><a routerLink="/loans">Ver todos →</a></div>
@if(!d.activeLoans.length){<p class="empty">Nenhum empréstimo ativo. Seus jogos estão em casa.</p>}
@for(loan of d.activeLoans;track loan.id){<div class="list-row"><div><strong>{{loan.gameTitle}}</strong><small>Com {{loan.friendName}}</small></div><span class="muted">{{loan.loanedAt|date:'dd/MM/yyyy'}}</span></div>}</section>
<section class="panel"><h2>Atividade recente</h2>@for(loan of d.recentLoans;track loan.id){<div class="list-row"><span>{{loan.gameTitle}} <small>{{loan.friendName}}</small></span><span class="badge">{{loan.returnedAt?'Devolvido':'Emprestado'}}</span></div>}@empty{<p class="empty">Seu histórico começa no primeiro empréstimo.</p>}</section>}`})
export class DashboardPage { private readonly api=inject(ApiService);readonly data=signal<Dashboard|null>(null);readonly error=signal('');constructor(){this.load();}load(){this.error.set('');this.api.dashboard().subscribe({next:x=>this.data.set(x),error:e=>this.error.set(errorMessage(e))});} }
