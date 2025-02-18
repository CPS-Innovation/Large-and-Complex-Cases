describe("Find a case page", () => {
  it("Should show the page correctly", () => {
    cy.visit("http://localhost:3000/");
    cy.get("h1").contains("Find a case");
    cy.contains('button', 'Search').should('be.visible'); 
    cy.contains('label', 'Search by Operation name').find('input').should('be.visible');
  });
});
