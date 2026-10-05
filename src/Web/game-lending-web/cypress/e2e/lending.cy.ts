describe('Game lending lifecycle',()=>{
 it('logs in, creates a friend and a game, lends and returns it',()=>{
   const suffix=Date.now()+'-'+Math.random().toString(16).slice(2,8);
   const friend='Friend '+suffix;const game='Game '+suffix;
   cy.visit('/login');cy.get('[data-cy=client-id]').type('e2e-client');cy.get('[data-cy=client-secret]').type('e2e-secret');cy.get('[data-cy=login]').click();
   cy.url().should('include','/dashboard');cy.contains('Total de jogos').should('be.visible');
   cy.contains('nav a','Amigos').click();cy.contains('button','Cadastrar amigo').click();cy.get('[data-cy=friend-name]').type(friend);cy.get('[data-cy=save-friend]').click();
   cy.get('input[aria-label="Pesquisar amigos"]').type(friend);cy.contains('button','Pesquisar').click();cy.contains(friend).should('be.visible');
   cy.contains('nav a','Jogos').click();cy.contains('button','Cadastrar jogo').click();cy.get('[data-cy=game-title]').type(game);cy.get('[data-cy=save-game]').click();
   cy.contains('nav a','Empréstimos').click();cy.get('#gameSearch').type(game);cy.contains('button','Buscar jogos').click();cy.get('[data-cy=loan-game]').select(game);
   cy.get('#friendSearch').type(friend);cy.contains('button','Buscar amigos').click();cy.get('[data-cy=loan-friend]').select(friend);cy.get('[data-cy=borrow]').click();
   cy.contains('[data-cy=loan-row]',game).should('contain',friend);
   cy.contains('nav a','Jogos').click();cy.get('input[aria-label="Pesquisar jogos"]').type(game);cy.contains('button','Pesquisar').click();
   cy.contains('article',game).should('contain','Emprestado').and('contain',friend);
   cy.contains('nav a','Empréstimos').click();cy.contains('[data-cy=loan-row]',game).within(()=>cy.contains('button','Devolver').click());cy.contains('Jogo devolvido e disponível novamente.').should('be.visible');
   cy.contains('nav a','Jogos').click();cy.get('input[aria-label="Pesquisar jogos"]').type(game);cy.contains('button','Pesquisar').click();cy.contains('article',game).should('contain','Disponível').and('not.contain',friend);
 });
});
