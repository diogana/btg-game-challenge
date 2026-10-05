import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService, errorMessage } from './auth';
@Component({selector:'app-login',imports:[ReactiveFormsModule],template:`
<section class="login-layout"><div><p class="eyebrow">SEUS JOGOS. SEUS AMIGOS.</p><h1>Sempre saiba<br>com quem está<br><span class="accent">seu próximo jogo.</span></h1><p class="lead">Uma biblioteca organizada e um histórico para cada empréstimo.</p></div>
<form class="panel login-card" [formGroup]="form" (ngSubmit)="submit()"><p class="eyebrow">BEM-VINDO</p><h2>Acesse sua biblioteca</h2><p class="muted">Ambiente de demonstração: use qualquer ID e segredo não vazios.</p>
<label for="clientId">Client ID</label><input id="clientId" data-cy="client-id" formControlName="clientId" autocomplete="username" placeholder="demo-client">
<label for="clientSecret">Client Secret</label><input id="clientSecret" data-cy="client-secret" type="password" formControlName="clientSecret" autocomplete="current-password" placeholder="Seu segredo de acesso">
@if(error()){<p class="error" role="alert">{{error()}}</p>}
<button data-cy="login" type="submit" [disabled]="form.invalid || busy()">{{busy()?'Entrando…':'Entrar na biblioteca →'}}</button></form></section>`})
export class LoginPage {
  private readonly auth=inject(AuthService);private readonly router=inject(Router);private readonly fb=inject(FormBuilder);
  readonly form=this.fb.nonNullable.group({clientId:['',[Validators.required,Validators.pattern(/\S/)]],clientSecret:['',[Validators.required,Validators.pattern(/\S/)]]});
  readonly error=signal('');readonly busy=signal(false);
  submit(){if(this.form.invalid)return;this.error.set('');this.busy.set(true);const v=this.form.getRawValue();
    this.auth.login(v.clientId,v.clientSecret).pipe(finalize(()=>this.busy.set(false))).subscribe({next:()=>{this.form.reset();void this.router.navigate(['/dashboard']);},error:e=>this.error.set(errorMessage(e))});}
}
