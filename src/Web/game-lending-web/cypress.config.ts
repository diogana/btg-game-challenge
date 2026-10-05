import { defineConfig } from 'cypress';
export default defineConfig({e2e:{baseUrl:'http://localhost:4200',supportFile:false,specPattern:'cypress/e2e/**/*.cy.ts'},video:false,viewportWidth:1366,viewportHeight:900});
