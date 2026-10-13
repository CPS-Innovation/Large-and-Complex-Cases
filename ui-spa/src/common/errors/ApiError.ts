export class ApiError extends Error {
  public readonly name: string = "API_ERROR";
  public readonly path: string;
  public readonly code: number;
  public readonly customProperties?: Record<string, string>;
  public readonly customMessage?: string;
  public readonly correlationId?: string;
  constructor(
    message: string,
    path: string,
    { status, statusText }: { status: number; statusText: string },
    properties?: {
      customProperties?: Record<string, string>;
      customMessage?: string;
      correlationId?: string;
    },
  ) {
    super(`API Error: ${path} returned ${status} ${statusText} - ${message}`);
    this.path = path;
    this.code = status;
    this.customProperties = properties?.customProperties;
    this.customMessage = properties?.customMessage;
    this.correlationId = properties?.correlationId;
  }
}
