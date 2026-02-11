import { Pipe, PipeTransform } from '@angular/core';

const FRACTION_MAP: [number, string][] = [
  [0.125, '⅛'],
  [0.25, '¼'],
  [1 / 3, '⅓'],
  [0.375, '⅜'],
  [0.5, '½'],
  [0.625, '⅝'],
  [2 / 3, '⅔'],
  [0.75, '¾'],
  [0.875, '⅞'],
];

const TOLERANCE = 0.02;

@Pipe({
  name: 'fraction',
  standalone: true,
})
export class FractionPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value == null) {
      return '';
    }

    const whole = Math.floor(value);
    const decimal = value - whole;

    if (decimal < TOLERANCE) {
      return whole.toString();
    }

    for (const [target, symbol] of FRACTION_MAP) {
      if (Math.abs(decimal - target) < TOLERANCE) {
        return whole > 0 ? `${whole}${symbol}` : symbol;
      }
    }

    // No matching fraction — show clean decimal
    return value % 1 === 0 ? value.toString() : parseFloat(value.toFixed(2)).toString();
  }
}
