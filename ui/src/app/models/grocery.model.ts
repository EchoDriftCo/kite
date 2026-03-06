export interface GroceryCheckoutUrl {
  service: string;
  url: string;
  itemCount: number;
  normalizedItems: string[];
}

export interface GroceryCheckoutOptions {
  itemCount: number;
  normalizedItems: string[];
  options: GroceryCheckoutUrl[];
}
