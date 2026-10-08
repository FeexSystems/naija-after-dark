export interface WalletSnapshot {
  playerId: string;
  currency: "NGN";
  balance: number;
}

export interface SpendResult {
  requestId: string;
  success: boolean;
  errorCode?: string;
  payload?: {
    sku: string;
    amount: number;
    balanceAfter: number;
    category: string;
  };
}

export const PRICES = {
  SUYA_PLATE: 5000,
  CLUB_TICKET: 20000,
  SOFT_DRINK: 1500,
  TRANSPORT_SHORT: 4000,
  BEACH_DRINK: 3000,
} as const;
