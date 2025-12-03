import dotenv from 'dotenv';
import * as path from 'path';
import { fileURLToPath } from 'url';

// Load environment variables BEFORE config is evaluated
const __filename_config = fileURLToPath(import.meta.url);
const __dirname_config = path.dirname(__filename_config);

const envLocalPath = path.resolve(__dirname_config, '../../.env.local');
const envPath = path.resolve(__dirname_config, '../../.env');

console.log(':wrench: DEBUG: Loading env from:', envLocalPath);
const result1 = dotenv.config({ path: envLocalPath });
const result2 = dotenv.config({ path: envPath });

if (result1.error) console.log(':warning:  .env.local not found or error:', result1.error.message);
if (result2.error) console.log(':warning:  .env not found or error:', result2.error.message);

export interface TestConfig {
  baseUrl: string;
  headless: boolean;
  useMockAuth: boolean;
  screenSize: {
    width: number;
    height: number;
  };
  auth: {
    azureAd: {
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

// Debug: Show what env vars we got
console.log(':wrench: DEBUG: Environment variables loaded:');
console.log('   BASE_URL:', process.env.BASE_URL || '(not set)');
console.log('   HEADLESS:', process.env.HEADLESS || '(not set)');
console.log('   VITE_MOCK_AUTH:', process.env.VITE_MOCK_AUTH || '(not set)');
console.log('   TEST_CASE_IDS:', process.env.TEST_CASE_IDS || '(not set)');
console.log('   AZURE_AD_TENANT_ID:', process.env.AZURE_AD_TENANT_ID ? '✓ set' : '✗ missing');
console.log('   AZURE_AD_CLIENT_ID:', process.env.AZURE_AD_CLIENT_ID ? '✓ set' : '✗ missing');
console.log('   AZURE_AD_CLIENT_SECRET:', process.env.AZURE_AD_CLIENT_SECRET ? '✓ set' : '✗ missing');
console.log('   AZURE_AD_SCOPE (for token acquisition):', process.env.AZURE_AD_SCOPE || 'api://xxx/.default');
console.log('   CMS_USERNAME:', process.env.CMS_USERNAME ? '✓ set' : '✗ missing');
console.log('   CMS_PASSWORD:', process.env.CMS_PASSWORD ? '✓ set' : '✗ missing');
console.log('   CMS_ACCESS_KEY:', process.env.CMS_ACCESS_KEY ? '✓ set' : '✗ missing');

export const Config: TestConfig = {
  baseUrl: process.env.BASE_URL || 'http://localhost:5173',
  headless: process.env.HEADLESS === 'false' ? false : true, // Explicit check for 'false' string
  useMockAuth: process.env.VITE_MOCK_AUTH === 'true',
  screenSize: {
    width: parseInt(process.env.SCREEN_WIDTH || '1920'),
    height: parseInt(process.env.SCREEN_HEIGHT || '1080'),
  },
  auth: {
    azureAd: {
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