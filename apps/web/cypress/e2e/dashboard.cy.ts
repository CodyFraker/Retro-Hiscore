describe("Retro Hiscore dashboard", () => {
  it("lists games and shows missing scores as dashes", () => {
    cy.visit("/");
    cy.contains("Friend boards");
    cy.contains("Game 38130").click();
    cy.contains("Space Cadet");
    cy.contains("352,750");
    cy.contains("td", "—").should("exist");
  });

  it("queues a manual refresh", () => {
    cy.visit("/");
    cy.contains("button", "Refresh scores").click();
    cy.contains("Sync queued");
  });
});
