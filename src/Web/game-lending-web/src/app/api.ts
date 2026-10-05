import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API } from './auth';
import { Dashboard, Friend, FriendSummary, Game, GameDetails, Library, Loan, Page } from './models';
@Injectable({providedIn:'root'})
export class ApiService {
  private readonly http=inject(HttpClient);
  dashboard(){return this.http.get<Dashboard>(`${API}/dashboard`);}
  library(search='',status='',page=1,pageSize=20){return this.http.get<Library>(`${API}/library`,{params:{search,status,page,pageSize}});}
  friends(search='',page=1,pageSize=20){return this.http.get<Page<Friend>>(`${API}/friends`,{params:{search,page,pageSize}});}
  friend(id:string,page=1){return this.http.get<FriendSummary>(`${API}/friends/${id}/summary`,{params:{page}});}
  game(id:string,page=1){return this.http.get<GameDetails>(`${API}/library/${id}`,{params:{page}});}
  saveFriend(id:string|null,name:string,email:string){return id ? this.http.put<Friend>(`${API}/friends/${id}`,{name,email}) : this.http.post<Friend>(`${API}/friends`,{name,email});}
  saveGame(id:string|null,title:string,platforms:string[]){return id ? this.http.put<Game>(`${API}/games/${id}`,{title,platforms}) : this.http.post<Game>(`${API}/games`,{title,platforms});}
  deleteFriend(id:string){return this.http.delete<void>(`${API}/friends/${id}`);}
  deleteGame(id:string){return this.http.delete<void>(`${API}/games/${id}`);}
  loans(active=true,page=1){return this.http.get<Page<Loan>>(`${API}/loans${active?'/active':''}`,{params:{page}});}
  borrow(gameId:string,friendId:string){return this.http.post<Loan>(`${API}/loans`,{gameId,friendId});}
  returnLoan(id:string){return this.http.post<Loan>(`${API}/loans/${id}/return`,{});}
}
