import { logTelemetryEvent } from "./apis/gateway-api";

export const TelemetryType = {
  Event: "Event",
  Exception: "Exception",
  Metric: "Metric",
  PageView: "PageView",
  Trace: "Trace",
} as const;

export type TelemetryPayload = {
  telemetryType: (typeof TelemetryType)[keyof typeof TelemetryType];
  properties: Record<string, unknown>[];
};

export type TelemetryEventPropsMap = {
  JourneyStarted: [{ journeyId: string }];
  JourneyCancelled: [{ journeyId: string }, { cancelledFrom: string }];
};
export type CustomEventName = keyof TelemetryEventPropsMap;

export type TelemetryEventProps<T extends CustomEventName> =
  TelemetryEventPropsMap[T];

export type TelemetryPageViewProps = Record<string, unknown>[];

export class TelemetryService {
  async trackEvent<T extends CustomEventName>(
    eventName: T,
    properties: TelemetryEventProps<T>,
  ): Promise<void> {
    const payload: TelemetryPayload = {
      telemetryType: TelemetryType.Event,
      properties: [{ name: eventName }, ...properties],
    };
    await logTelemetryEvent(payload);
  }

  async trackPageView(
    pageName: string,
    properties: TelemetryPageViewProps,
  ): Promise<void> {
    const payload: TelemetryPayload = {
      telemetryType: TelemetryType.PageView,
      properties: [{ pageName: pageName }, ...properties],
    };
    await logTelemetryEvent(payload);
  }

  async trackException(
    error: Error,
    properties: Record<string, unknown>[] = [],
  ): Promise<void> {
    const payload: TelemetryPayload = {
      telemetryType: TelemetryType.Exception,
      properties: [
        {
          exceptionMessage: error.message,
        },
        { errorName: error.name },
        {
          errorStack: error.stack ?? "",
        },
        ...properties,
      ],
    };
    await logTelemetryEvent(payload);
  }

  async trackTrace(properties: Record<string, unknown>[] = []): Promise<void> {
    const payload: TelemetryPayload = {
      telemetryType: TelemetryType.Trace,
      properties: [...properties],
    };
    await logTelemetryEvent(payload);
  }
}
export const telemetryService = new TelemetryService();
