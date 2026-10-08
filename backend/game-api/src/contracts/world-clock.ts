/**
 * NAAD World Clock contracts (Gate 6)
 * Server owns game time. Client only presents / interpolates.
 */

export type WorldPeriod =
  | "MORNING" // 06:00–08:59
  | "DAY" // 09:00–15:59
  | "TRANSITION" // 16:00–18:59 sunset
  | "NIGHT" // 19:00–23:59
  | "LATE_NIGHT" // 00:00–02:59
  | "AFTER_HOURS"; // 03:00–05:59

export interface WorldClock {
  gameDate: string; // YYYY-MM-DD (UTC game calendar)
  gameTime: string; // ISO timestamptz
  minutesSinceMidnight: number;
  period: WorldPeriod;
  timeScale: number; // game minutes per real minute
  weather: string;
  trafficLevel: number;
  nightlifeLevel: number;
}

export interface WorldClockResponse {
  success: boolean;
  errorCode?: string;
  errorMessage?: string;
  gameDate?: string;
  gameTime?: string;
  minutesSinceMidnight?: number;
  period?: WorldPeriod;
  timeScale?: number;
  weather?: string;
  trafficLevel?: number;
  nightlifeLevel?: number;
}

/** Period boundaries in minutes since midnight */
export const PERIOD_BOUNDS: Record<WorldPeriod, { start: number; end: number }> = {
  MORNING: { start: 360, end: 539 },
  DAY: { start: 540, end: 959 },
  TRANSITION: { start: 960, end: 1139 },
  NIGHT: { start: 1140, end: 1439 },
  LATE_NIGHT: { start: 0, end: 179 },
  AFTER_HOURS: { start: 180, end: 359 },
};
