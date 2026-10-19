// Screenshot RecipeVault screens with a faked Supabase session and fixture API responses.
import { createRequire } from 'module';
const require = createRequire(new URL('../../../../ui/package.json', import.meta.url));
const { chromium } = require('@playwright/test');

const OUT = process.argv[2];
const BASE = 'http://localhost:4200';
const now = Math.floor(Date.now() / 1000);
const user = { id: '11111111-1111-1111-1111-111111111111', aud: 'authenticated', role: 'authenticated', email: 'reviewer@example.test', is_anonymous: false, app_metadata: { provider: 'email' }, user_metadata: {}, created_at: '2026-01-01T00:00:00Z' };
const session = { access_token: 'x.eyJzdWIiOiIxMTEifQ.x', token_type: 'bearer', expires_in: 3600 * 24, expires_at: now + 3600 * 24, refresh_token: 'r', user };

const tag = (n, c = 1, cn = 'Cuisine') => ({ tagResourceId: 't-' + n, globalName: n, displayName: n, category: c, categoryName: cn, isAiAssigned: true, isOverridden: false });
const mk = (i, title, desc, extra = {}) => ({
  recipeResourceId: 'r' + i, title, description: desc, yield: 4, prepTimeMinutes: 15, cookTimeMinutes: 30, totalTimeMinutes: 45,
  source: 'https://example.test/recipe', isOwner: true, rating: (i % 5) + 1, isFavorite: i % 3 === 0, forkCount: i,
  tags: [tag('Italian'), tag('Dinner', 2, 'Meal type'), tag('Vegetarian', 3, 'Dietary')].slice(0, (i % 3) + 1),
  ingredients: [
    { sortOrder: 1, quantity: 2, unit: 'cup', item: 'all-purpose flour', preparation: 'sifted' },
    { sortOrder: 2, quantity: 1, unit: 'tsp', item: 'kosher salt' },
    { sortOrder: 3, quantity: 3, unit: 'tbsp', item: 'olive oil' },
    { sortOrder: 4, quantity: 2, item: 'cloves garlic', preparation: 'minced' },
    { sortOrder: 5, quantity: 1.5, unit: 'lb', item: 'cherry tomatoes', preparation: 'halved' },
  ],
  instructions: [
    { stepNumber: 1, instruction: 'Preheat the oven to 425°F and line a baking sheet with parchment.' },
    { stepNumber: 2, instruction: 'Toss the tomatoes with olive oil, garlic and salt. Spread in a single layer and roast for 20 minutes until blistered.' },
    { stepNumber: 3, instruction: 'Meanwhile, bring a large pot of salted water to a boil and cook the pasta until al dente. Reserve one cup of pasta water.' },
    { stepNumber: 4, instruction: 'Combine everything, loosen with pasta water, and finish with basil.' },
  ],
  createdDate: '2026-09-0' + ((i % 9) + 1) + 'T12:00:00Z', ...extra,
});
const recipes = [
  mk(1, 'Roasted Cherry Tomato Pasta', 'Blistered tomatoes, garlic and basil tossed with bucatini.', { sourceImageUrl: '/assets/og-image.jpg' }),
  mk(2, 'Weeknight Chicken Tikka Masala', 'A faster take on the classic with a creamy spiced tomato sauce.'),
  mk(3, 'Lemon Ricotta Pancakes', 'Fluffy pancakes with lemon zest and whole-milk ricotta.'),
  mk(4, 'Miso Glazed Salmon with Charred Scallions and Sesame Rice Bowls', 'Sweet-salty glaze, quick broil, and a very long title to test truncation behaviour in cards.'),
  mk(5, 'Black Bean Tacos', 'Smoky beans, pickled onions and lime crema.'),
  mk(6, 'Classic French Onion Soup', 'Deeply caramelized onions under a gruyère crouton.'),
];
const paged = (items) => ({ items, pageNumber: 1, pageSize: 20, totalCount: items.length, totalPages: 1 });
const mealPlan = { mealPlanResourceId: 'mp1', name: 'Week of Oct 19', startDate: '2026-10-19', endDate: '2026-10-25', entries: [
  { date: '2026-10-19', mealSlot: 3, recipeResourceId: 'r1', recipeTitle: recipes[0].title, recipeYield: 4, servings: 4, isLeftover: false },
  { date: '2026-10-20', mealSlot: 1, recipeResourceId: 'r3', recipeTitle: recipes[2].title, recipeYield: 4, servings: 2, isLeftover: false },
  { date: '2026-10-20', mealSlot: 3, recipeResourceId: 'r2', recipeTitle: recipes[1].title, recipeYield: 4, servings: 4, isLeftover: false },
  { date: '2026-10-21', mealSlot: 2, recipeResourceId: 'r2', recipeTitle: recipes[1].title, recipeYield: 4, servings: 2, isLeftover: true },
  { date: '2026-10-22', mealSlot: 3, recipeResourceId: 'r4', recipeTitle: recipes[3].title, recipeYield: 4, servings: 4, isLeftover: false },
]};

function api(path) {
  const p = path.split('?')[0];
  if (p === 'user/onboarding-status') return { hasCompletedOnboarding: true, recipeCount: 6, hasDietaryProfile: true, hasImportedRecipes: true, progress: { dietaryProfileSet: true, samplesAdded: true, tourCompleted: true } };
  if (/^recipes\/r\d$/.test(p)) return recipes.find(r => 'recipes/' + r.recipeResourceId === p);
  if (p === 'recipes' || p === 'recipes/public' || p === 'recipes/discover') return paged(recipes);
  if (p === 'meal-plans') return paged([mealPlan, { ...mealPlan, mealPlanResourceId: 'mp2', name: 'Thanksgiving prep', startDate: '2026-11-23', endDate: '2026-11-27' }]);
  if (p === 'meal-plans/mp1') return mealPlan;
  if (p === 'meal-plans/mp1/grocery-list') return { items: [
    { item: 'cherry tomatoes', quantity: 1.5, unit: 'lb', category: 'Produce', sources: [recipes[0].title] },
    { item: 'garlic', quantity: 4, unit: 'clove', category: 'Produce' },
    { item: 'chicken thighs', quantity: 2, unit: 'lb', category: 'Meat' },
    { item: 'whole-milk ricotta', quantity: 1, unit: 'cup', category: 'Dairy' },
    { item: 'all-purpose flour', quantity: 2, unit: 'cup', category: 'Pantry' },
  ]};
  if (p === 'circles') return paged([{ circleResourceId: 'c1', name: 'Family Recipes', description: 'Grandma’s cards, digitized.', ownerSubjectId: user.id, isOwner: true, memberCount: 5, recipeCount: 12 }, { circleResourceId: 'c2', name: 'Climbing Gym Potluck', ownerSubjectId: 'x', isOwner: false, memberCount: 9, recipeCount: 4 }]);
  if (p === 'cooking-log/stats') return { totalCooks: 37, uniqueRecipes: 14, currentStreak: 3, longestStreak: 9, cooksThisMonth: 8, averageCooksPerWeek: 2.4, mostActiveDayOfWeek: 'Sunday', mostCooked: recipes.slice(0, 3).map((r, i) => ({ recipeResourceId: r.recipeResourceId, recipeTitle: r.title, cookCount: 9 - i * 2 })) };
  if (/^equipment\/recipes\/r\d\/check$/.test(p)) return { recipeResourceId: p.split('/')[2], requiredEquipment: [], hasAllEquipment: true, missingEquipment: [] };
  if (/^recipes\/r\d\/personal-stats$/.test(p)) return { recipeResourceId: p.split('/')[1], cookCount: 0 };
  if (p === 'recipes/generate/quota') return { used: 1, limit: 5, remaining: 4 };
  if (/^(equipment(\/my)?|collections\/my|recipes\/r\d\/(circles|cooking-log|links|used-in)|pantry\/defaults)$/.test(p)) return [];
  if (p === 'pantry') return { items: [] };
  if (p === 'ingredients/suggest') return { suggestions: [] };
  return paged([]);
}

const shots = [
  ['login', '/login', false],
  ['features', '/features', false],
  ['recipes', '/recipes', true],
  ['recipe-detail', '/recipes/r1', true],
  ['recipe-detail-noimg', '/recipes/r4', true],
  ['recipe-new', '/recipes/new', true],
  ['cook', '/recipes/r1/cook', true],
  ['discover', '/discover', true],
  ['ingredient-search', '/ingredient-search', true],
  ['meal-plans', '/meal-plans', true],
  ['meal-plan-detail', '/meal-plans/mp1', true],
  ['grocery', '/meal-plans/mp1/grocery-list', true],
  ['circles', '/circles', true],
  ['stats', '/cooking-stats', true],
  ['settings', '/settings', true],
  ['collections', '/collections', true],
];

const only = process.argv[3] ? process.argv[3].split(',') : null;
const browser = await chromium.launch();
for (const [vpName, vp] of [['desktop', { width: 1366, height: 860 }], ['mobile', { width: 390, height: 844 }]]) {
  for (const theme of ['light', 'dark']) {
    for (const [name, url, auth] of shots) {
      if (only && !only.includes(name)) continue;
      const ctx = await browser.newContext({ viewport: vp, deviceScaleFactor: 1, isMobile: vpName === 'mobile', hasTouch: vpName === 'mobile' });
      await ctx.addInitScript(([s, t, a]) => {
        localStorage.setItem('rv-theme', t);
        localStorage.setItem('tour_skipped', 'true');
        localStorage.setItem('pwa-install-dismissed', 'true');
        if (a) { localStorage.setItem('sb-localhost-auth-token', JSON.stringify(s)); localStorage.setItem('rv-has-account', 'true'); }
      }, [session, theme, auth]);
      await ctx.route('http://localhost:54321/**', r => r.fulfill({ json: r.request().url().includes('/user') ? user : {} }));
      await ctx.route('http://localhost:5000/**', r => {
        const path = r.request().url().replace('http://localhost:5000/api/v1/', '');
        if (path.endsWith('/cooking-data')) return r.fulfill({ json: { recipeResourceId: 'r1', steps: recipes[0].instructions.map(s => ({ ...s, timerIndexes: s.stepNumber === 2 ? [0] : [] })), timers: [{ index: 0, label: '20 min roast', seconds: 1200, stepNumber: 2 }], temperatures: [{ stepNumber: 1, value: 425, unit: 'F', context: 'oven' }] } });
        if (r.request().method() !== 'GET') return r.fulfill({ json: {} });
        // No nutrition has been calculated for the fixture recipes.
        if (/^recipes\/r\d\/nutrition/.test(path)) return r.fulfill({ status: 404, json: { message: 'Not found' } });
        return r.fulfill({ json: api(path) });
      });
      const page = await ctx.newPage();
      const errors = [];
      page.on('pageerror', e => errors.push(e.message));
      await page.goto(BASE + url, { waitUntil: 'networkidle' }).catch(() => {});
      await page.waitForTimeout(800);
      // A compile error in `ng serve` shows an overlay instead of the page; never treat that as a capture.
      if (await page.locator('vite-error-overlay').count()) errors.push('dev-server error overlay is showing');
      // The app scrolls an inner container, so grow the viewport to the tallest scrollable element.
      const h = await page.evaluate(() => Math.max(...[...document.querySelectorAll('*')].map(e => e.scrollHeight)));
      await page.setViewportSize({ width: vp.width, height: Math.min(Math.max(h, vp.height), 3200) });
      await page.waitForTimeout(300);
      await page.screenshot({ path: `${OUT}/${vpName}-${theme}-${name}.png`, fullPage: true });
      const lowContrast = await page.evaluate(() => {
        const parse = c => { const m = c.match(/[\d.]+/g); return m ? m.map(Number) : [0, 0, 0, 0]; };
        const lum = ([r, g, b]) => { const f = v => { v /= 255; return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4; }; return 0.2126 * f(r) + 0.7152 * f(g) + 0.0722 * f(b); };
        const blend = (top, bot) => { const a = top[3] ?? 1; return [0, 1, 2].map(i => top[i] * a + bot[i] * (1 - a)); };
        const bgOf = el => { const stack = []; for (let e = el; e; e = e.parentElement) { const cs = getComputedStyle(e); if (cs.backgroundImage !== 'none') return null; const c = parse(cs.backgroundColor); if (c.length === 3) c.push(1); if (c[3] > 0) { stack.push(c); if (c[3] >= 1) break; } } let out = [255, 255, 255]; for (let i = stack.length - 1; i >= 0; i--) out = blend(stack[i], out); return out; };
        const res = []; const seen = new Set();
        const w = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
        while (w.nextNode()) {
          const t = w.currentNode.textContent.trim(); const el = w.currentNode.parentElement;
          if (!t || !el || !el.offsetParent || el.closest('mat-icon')) continue;
          const cs = getComputedStyle(el); if (cs.visibility === 'hidden' || +cs.opacity === 0) continue;
          if (el.closest('[disabled], .mat-mdc-button-disabled')) continue;
          const bg = bgOf(el); if (!bg) continue;
          const fg = parse(cs.color); const fgc = blend(fg.length === 4 ? fg : [...fg, 1], bg);
          const L1 = lum(fgc), L2 = lum(bg); const ratio = (Math.max(L1, L2) + 0.05) / (Math.min(L1, L2) + 0.05);
          const size = parseFloat(cs.fontSize), bold = +cs.fontWeight >= 700; const large = size >= 24 || (bold && size >= 18.66);
          if (ratio < (large ? 3 : 4.5)) { const k = t.slice(0, 40) + ratio.toFixed(1); if (!seen.has(k)) { seen.add(k); res.push(`${ratio.toFixed(2)} "${t.slice(0, 40)}" ${cs.color}`); } }
        }
        return res;
      });
      if (lowContrast.length) { const fs = await import('fs'); fs.appendFileSync(`${OUT}/contrast.txt`, `\n## ${vpName} ${theme} ${name}\n` + lowContrast.join('\n')); }
      const brokenIcons =await page.evaluate(() => [...document.querySelectorAll('mat-icon')].filter(i => i.offsetParent && !getComputedStyle(i).fontFamily.includes('Material')).map(i => i.textContent.trim()));
      console.log(vpName, theme, name, page.url().replace(BASE, ''), errors.length ? 'ERR ' + errors[0].slice(0, 120) : '', brokenIcons.length ? 'BROKEN-ICONS ' + brokenIcons.join(',') : '');
      await ctx.close();
    }
  }
}
await browser.close();
