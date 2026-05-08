import { randomInt } from "node:crypto";

export async function registerCase(
  caseApiBaseUrl: string,
  accessToken: string,
  cmsAuth: string,
  defendantSurname: string
): Promise<{ caseId: number; caseUrn: string }> {
  const urnUniqueRef = String(randomInt(10000, 100000)).padStart(5, "0");

  const correlationId = crypto.randomUUID();

  const body = {
    urn: {
      policeForce: "45",
      policeUnit: "EL",
      uniqueRef: urnUniqueRef,
      year: new Date().getFullYear() % 100,
    },
    registeringAreaId: 1001,
    registeringUnitId: 2002,
    allocatedWcuId: 2012391,
    crest: "",
    operationName: "",
    courtLocationId: 0,
    courtLocationName: "",
    hearingDate: null,
    defendants: [
      {
        isDefendant: true,
        surname: defendantSurname,
        firstname: "",
        companyName: "",
        dateOfBirth: "1984-05-23",
        gender: "M",
        disability: "U",
        ethnicity: "W1",
        religion: "NP",
        type: "UN",
        arrestDate: null,
        seriousDangerousOffender: false,
        arrestSummonsNumber: "",
        isNotYetCharged: false,
        aliases: [
          {
            listOrder: 0,
            surname: "AliasTest",
            firstNames: "Automation",
          },
        ],
        charges: [
          {
            offenceCode: "FI68036",
            offenceDescription:
              "Possess a prohibited weapon ( automatic )",
            offenceId: "22336",
            dateFrom: "2025-10-01",
            dateTo: null,
            comment: "Automation test charge",
            offenceLocation: {
              addressLine1: "123 Test Street",
              addressLine2: "",
              townCity: "London",
              postcode: "SW1A 1AA",
            },
            victimIndexId: 0,
            chargeDetailsSummary: "",
            modeOfTrial: "EW",
          },
        ],
      },
    ],
    victims: [
      {
        surname: "VictimTest",
        forename: "Automation",
        isVulnerable: false,
        isIntimidated: false,
        isWitness: true,
      },
    ],
    complexity: "",
    caseWeight: "",
    monitoringCodes: [{ code: "ATRY", selected: true }],
    prosecutorId: 0,
    caseWorker: "",
    oicRank: "",
    oicSurname: "",
    oicFirstnames: "",
    oicShoulderNumber: "",
    oicPoliceUnit: "",
  };

  const response = await fetch(`${caseApiBaseUrl}/api/v1/cases`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Cookie: `Cms-Auth-Values=${cmsAuth}`,
      "Correlation-Id": correlationId,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `Case registration failed (${response.status}): ${text}`
    );
  }

  const data = await response.json();

  if (!data.caseId || data.caseId <= 0) {
    throw new Error(`Invalid caseId returned: ${JSON.stringify(data)}`);
  }

  return { caseId: data.caseId, caseUrn: data.urn || data.caseUrn };
}
