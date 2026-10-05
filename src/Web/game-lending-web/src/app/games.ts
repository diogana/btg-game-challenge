import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from './api';
import { Game, Library } from './models';
import { errorMessage } from './auth';
@Component({selector:'app-games',imports:[ReactiveFormsModule,FormsModule,RouterLink],template:`
<div class="page-title"><div><p class="eyebrow">A PRÓXIMA PARTIDA COMEÇA AQUI</p><h1>Seus jogos</h1></div><button (click)="edit()">+ Cadastrar jogo</button></div>
<form class="toolbar" (ngSubmit)="page=1;load()"><input name="search" aria-label="Pesquisar jogos" [(ngModel)]="search" placeholder="Pesquisar na coleção…"><select name="status" aria-label="Disponibilidade" [(ngModel)]="status"><option value="">Todos os jogos</option><option value="Available">Disponíveis</option><option value="Borrowed">Emprestados</option></select><button class="secondary">Pesquisar</button></form>
@if(error()){<p class="error" role="alert">{{error()}}</p>}
@if(showForm()){<form class="panel editor" [formGroup]="form" (ngSubmit)="save()"><h2>{{editingId?'Editar jogo':'Novo jogo'}}</h2><label for="title">Título</label><input id="title" data-cy="game-title" formControlName="title"><label for="platforms">Plataformas, separadas por vírgula</label><input id="platforms" formControlName="platforms" placeholder="PS5, PS4"><div class="actions"><button data-cy="save-game" [disabled]="form.invalid || busy()">Salvar</button><button type="button" class="quiet" (click)="showForm.set(false)">Cancelar</button></div></form>}
@if(loading()){<p role="status">Carregando jogos…</p>}
@if(data();as d){<div class="game-grid">@for(game of d.games.items;track game.id){<article class="panel game-card"><div class="game-art">{{game.title.slice(0,1)}}</div><span class="badge" [class.borrowed]="game.status==='Borrowed'">{{game.status==='Available'?'Disponível':'Emprestado'}}</span><h2><a [routerLink]="['/games',game.id]">{{game.title}}</a></h2><p class="muted">{{game.platforms.join(' · ') || 'Plataforma não informada'}}</p><p class="borrower">{{game.friendName?'Com '+game.friendName:'Pronto para a próxima partida'}}</p><div class="actions"><button class="quiet" (click)="edit(game)">Editar</button><button class="quiet danger" [disabled]="busy()" (click)="remove(game)">Desativar</button></div></article>}@empty{<p class="empty">Nenhum jogo encontrado.</p>}</div><div class="pagination"><button class="quiet" [disabled]="page===1" (click)="page=page-1;load()">← Anterior</button><span>{{d.games.total}} jogos · Página {{page}}</span><button class="quiet" [disabled]="page*20>=d.games.total" (click)="page=page+1;load()">Próxima →</button></div>}`})
export class GamesPage {
  private readonly api=inject(ApiService);private readonly fb=inject(FormBuilder);
  readonly form=this.fb.nonNullable.group({title:['',[Validators.required,Validators.maxLength(300),Validators.pattern(/\S/)]],platforms:['']});
  readonly data=signal<Library|null>(null);readonly error=signal('');readonly showForm=signal(false);readonly loading=signal(false);readonly busy=signal(false);
  search='';status='';page=1;editingId:string|null=null;
  constructor(){this.load();}
  load(){this.error.set('');this.loading.set(true);this.api.library(this.search,this.status,this.page).pipe(finalize(()=>this.loading.set(false))).subscribe({next:x=>this.data.set(x),error:e=>this.error.set(errorMessage(e))});}
  edit(game?:Game){this.editingId=game?.id??null;this.form.setValue({title:game?.title??'',platforms:game?.platforms.join(', ')??''});this.showForm.set(true);}
  save(){if(this.form.invalid)return;this.busy.set(true);const v=this.form.getRawValue();this.api.saveGame(this.editingId,v.title,v.platforms.split(',').map(x=>x.trim()).filter(Boolean)).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>{this.showForm.set(false);this.load();},error:e=>this.error.set(errorMessage(e))});}
  remove(game:Game){if(!confirm(`Desativar “${game.title}”? O histórico será preservado.`))return;this.busy.set(true);this.api.deleteGame(game.id).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>this.load(),error:e=>this.error.set(errorMessage(e))});}
}
