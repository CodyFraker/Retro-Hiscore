describe("Retro Hiscore dashboard", () => {
  it("lists games and shows missing scores as dashes", () => {
    cy.visit("/");
    cy.contains("Friend boards");
    cy.contains("Game 38130").click();
    cy.contains("Space Cadet");
    cy.contains("352,750");
    cy.contains("td", "—").should("exist");
  });

  it("queues a per-game refresh", () => {
    cy.visit("/");
    cy.contains("Game 38130").click();
    cy.contains("button", "Refresh scores").click();
    cy.contains("Refresh queued");
  });
});
