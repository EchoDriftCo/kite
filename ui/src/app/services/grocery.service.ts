import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { GroceryCheckoutOptions } from '../models/grocery.model';

@Injectable({
  providedIn: 'root'
})
export class GroceryService {
  private readonly endpoint = 'grocery';

  constructor(private api: ApiService) {}

  getCheckoutOptions(items: string[]): Observable<GroceryCheckoutOptions> {
    const validItems = items
      .map(item => item?.trim())
      .filter((item): item is string => !!item);

    const params = new URLSearchParams();
    validItems.forEach(item => params.append('items', item));

    return this.api.get<GroceryCheckoutOptions>(`${this.endpoint}/checkout-options?${params.toString()}`);
  }
}
