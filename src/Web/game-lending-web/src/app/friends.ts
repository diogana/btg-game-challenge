import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from './api';
import { Friend, Page } from './models';
import { errorMessage } from './auth';
@Component({selector:'app-friends',imports:[ReactiveFormsModule,FormsModule,RouterLink],template:`
<div class="page-title"><div><p class="eyebrow">BOAS PARTIDAS SÃO COMPARTILHADAS</p><h1>Seus amigos</h1></div><button (click)="edit()">+ Cadastrar amigo</button></div>
<form class="toolbar" (ngSubmit)="page=1;load()"><input name="search" aria-label="Pesquisar amigos" [(ngModel)]="search" placeholder="Pesquisar por nome…"><button class="secondary">Pesquisar</button></form>
@if(error()){<p class="error" role="alert">{{error()}}</p>}
@if(showForm()){<form class="panel editor" [formGroup]="form" (ngSubmit)="save()"><h2>{{editingId?'Editar amigo':'Novo amigo'}}</h2><label for="name">Nome</label><input id="name" data-cy="friend-name" formControlName="name"><label for="email">E-mail (opcional)</label><input id="email" type="email" formControlName="email"><div class="actions"><button data-cy="save-friend" [disabled]="form.invalid || busy()">Salvar</button><button type="button" class="quiet" (click)="showForm.set(false)">Cancelar</button></div></form>}
@if(loading()){<p role="status">Carregando amigos…</p>}
@if(data();as d){<section class="panel">@for(friend of d.items;track friend.id){<div class="list-row"><div class="person"><span class="avatar">{{friend.name.slice(0,1)}}</span><div><a [routerLink]="['/friends',friend.id]"><strong>{{friend.name}}</strong></a><small>{{friend.email || 'Sem e-mail cadastrado'}}</small></div></div><div class="actions"><button class="quiet" (click)="edit(friend)">Editar</button><button class="quiet danger" [disabled]="busy()" (click)="remove(friend)">Desativar</button></div></div>}@empty{<p class="empty">Cadastre um amigo para começar a emprestar.</p>}</section><div class="pagination"><button class="quiet" [disabled]="page===1" (click)="page=page-1;load()">← Anterior</button><span>{{d.total}} amigos · Página {{page}}</span><button class="quiet" [disabled]="page*20>=d.total" (click)="page=page+1;load()">Próxima →</button></div>}`})
export class FriendsPage {
  private readonly api=inject(ApiService);private readonly fb=inject(FormBuilder);
  readonly form=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150),Validators.pattern(/\S/)]],email:['',Validators.email]});
  readonly data=signal<Page<Friend>|null>(null);readonly error=signal('');readonly showForm=signal(false);readonly busy=signal(false);readonly loading=signal(false);
  search='';page=1;editingId:string|null=null;constructor(){this.load();}
  load(){this.error.set('');this.loading.set(true);this.api.friends(this.search,this.page).pipe(finalize(()=>this.loading.set(false))).subscribe({next:x=>this.data.set(x),error:e=>this.error.set(errorMessage(e))});}
  edit(friend?:Friend){this.editingId=friend?.id??null;this.form.setValue({name:friend?.name??'',email:friend?.email??''});this.showForm.set(true);}
  save(){if(this.form.invalid)return;const v=this.form.getRawValue();this.busy.set(true);this.api.saveFriend(this.editingId,v.name,v.email).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>{this.showForm.set(false);this.load();},error:e=>this.error.set(errorMessage(e))});}
  remove(friend:Friend){if(!confirm(`Desativar ${friend.name}? O histórico será preservado.`))return;this.busy.set(true);this.api.deleteFriend(friend.id).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>this.load(),error:e=>this.error.set(errorMessage(e))});}
}
