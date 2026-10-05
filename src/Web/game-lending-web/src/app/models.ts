export interface Page<T> { items:T[]; total:number; pageNumber:number; pageSize:number; }
export interface Friend { id:string; name:string; email:string|null; isActive:boolean; }
export interface Game { id:string; title:string; platforms:string[]; isActive:boolean; status:string; loanId:string|null; friendId:string|null; friendName:string|null; }
export interface Loan { id:string; gameId:string; gameTitle:string; friendId:string; friendName:string; loanedAt:string; returnedAt:string|null; }
export interface Statistics { totalGames:number; availableGames:number; borrowedGames:number; totalFriends:number; activeLoans:number; }
export interface Dashboard { statistics:Statistics; activeLoans:Loan[]; recentLoans:Loan[]; }
export interface Library { games:Page<Game>; activeLoans:Loan[]; }
export interface FriendSummary { friend:Friend; activeLoans:Page<Loan>; history:Page<Loan>; }
export interface GameDetails { game:Game; history:Page<Loan>; }
export interface TokenResponse { accessToken:string; tokenType:string; expiresIn:number; }
