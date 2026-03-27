export interface EgressWorkspace {
  id: string;
  name: string;
}

export interface UploadedFile {
  fileId: string;
  fileName: string;
  fileSize: number;
}

export interface TestSetupResult {
  workspace: EgressWorkspace;
  files: UploadedFile[];
  caseId: number;
  caseUrn: string;
}

export interface AuthTokens {
  accessToken: string;
  cmsAuth: string;
}
