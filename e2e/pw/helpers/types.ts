export interface EgressWorkspace {
  id: string;
  name: string;
}

export interface UploadedFile {
  // Egress file id from the completed-upload response. Used by teardown
  // (deleteFile) to remove the file after the test passes. Optional because
  // older callers may not capture it.
  id?: string;
  fileName: string;
  fileSize: number;
}

export interface TestSetupResult {
  workspace: EgressWorkspace;
  files: UploadedFile[];
  caseUrn: string;
  // Internal case id from the LCC case-api registerCase response. Surfaced
  // so the register-case setup project can persist it for consumers that
  // need to deep-link or populate DEFAULT_CASE_ID. Optional because
  // default-mode setup reads it from env rather than registering.
  caseId?: number;
  // Dated subfolder used in default mode to contain drift in the shared
  // workspace. Applied on both transfer sides: source uploads go into
  // "4. Served Evidence/<uploadSubfolder>/", NetApp->Egress copies land in
  // "2. Counsel only/<uploadSubfolder>/". Undefined for register-case mode.
  uploadSubfolder?: string;
}

export interface AuthTokens {
  accessToken: string;
  cmsAuth: string;
}
