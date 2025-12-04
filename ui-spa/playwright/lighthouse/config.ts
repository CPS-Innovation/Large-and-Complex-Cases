export interface TestConfig {
  baseUrl: string;
  aadUsername: string;
  aadPassword: string;
  headless: boolean;
  screenSize: {
    width: number;
    height: number;
  };
  auth: {
    azureAdApi: {
      tenantId: string;
      clientId: string;
      clientSecret: string;
      scope: string;
    };
    cms: {
      baseUrl: string;
      accessKey: string;
    };
  };
  cases: string[]; // Configurable case IDs
  performanceBudget: {
    lcp: number;
    fid: number;
    cls: number;
    tbt: number;
    si: number;
  };
  timeouts: {
    pageLoad: number;
    apiCall: number;
    auth: number;
  };
}

export const Config: TestConfig = {
  baseUrl: process.env.BASE_URL || 'http://localhost:5173',
  aadUsername: process.env.AZURE_AD_USERNAME || '',
  aadPassword: process.env.AZURE_AD_PASSWORD || '',
  headless: process.env.HEADLESS === 'true' || true, // Default to headless for CI
  screenSize: {
    width: parseInt(process.env.SCREEN_WIDTH || '1920'),
    height: parseInt(process.env.SCREEN_HEIGHT || '1080'),
  },
  auth: {
    azureAdApi: {
      tenantId: process.env.AZURE_AD_TENANT_ID || '',
      clientId: process.env.AZURE_AD_CLIENT_ID || '',
      clientSecret: process.env.AZURE_AD_CLIENT_SECRET || '',
      scope: process.env.AZURE_AD_SCOPE || 'api://user_impersonation'
    },
    cms: {
      baseUrl: process.env.CMS_BASE_URL || '',
      accessKey: process.env.CMS_ACCESS_KEY || ''
    }
  },
  cases: (process.env.TEST_CASE_IDS || '123,456,789').split(',').map(id => id.trim()), // Multiple case IDs
  performanceBudget: {
    lcp: parseInt(process.env.LH_LCP_BUDGET || '2500'),
    fid: parseInt(process.env.LH_FID_BUDGET || '100'),
    cls: parseFloat(process.env.LH_CLS_BUDGET || '0.1'),
    tbt: parseInt(process.env.LH_TBT_BUDGET || '300'),
    si: parseInt(process.env.LH_SI_BUDGET || '3000')
  },
  timeouts: {
    pageLoad: parseInt(process.env.PAGE_LOAD_TIMEOUT || '30000'),
    apiCall: parseInt(process.env.API_TIMEOUT || '10000'),
    auth: parseInt(process.env.AUTH_TIMEOUT || '15000')
  }
};

export function validateConfig(): void {
  const requiredVars = [
    'BASE_URL',
    'AZURE_AD_TENANT_ID',
    'AZURE_AD_CLIENT_ID', 
    'AZURE_AD_CLIENT_SECRET',
    'CMS_USERNAME',
    'CMS_PASSWORD',
    'CMS_ACCESS_KEY'
  ];

  const missing = requiredVars.filter(varName => !process.env[varName]);
  
  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missing.join(', ')}\n` +
      'Please check your .env configuration.'
    );
  }

  if (Config.cases.length === 0) {
    throw new Error('No case IDs configured. Please set TEST_CASE_IDS environment variable.');
  }
}
