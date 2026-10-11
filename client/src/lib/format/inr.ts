export function formatInr(amount: number): string {
  const fixed = amount.toFixed(2);
  const [whole, frac] = fixed.split(".");
  const lastThree = whole.slice(-3);
  const rest = whole.slice(0, -3);
  const groupedRest = rest.replace(/\B(?=(\d{2})+(?!\d))/g, ",");
  const wholeFormatted = rest.length > 0 ? `${groupedRest},${lastThree}` : lastThree;
  return `₹${wholeFormatted}.${frac}`;
}
