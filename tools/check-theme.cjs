// Exercise user-visible theme behavior without installing browser dependencies.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.join(__dirname, '../docs/assets/js/theme.js'), 'utf8');

function page({ dark = false, saved = null, blocked = false } = {}) {
  const callbacks = {};
  const root = { dataset: {}, removeAttribute: () => delete root.dataset.theme };
  const label = { hidden: true };
  const select = { value: '', closest: () => label,
    addEventListener: (_, fn) => { callbacks.select = fn; } };
  const media = { matches: dark, addEventListener: (_, fn) => { callbacks.system = fn; } };
  const metas = [{}, {}];
  let stored = saved;
  vm.runInNewContext(source, {
    window: { matchMedia: () => media },
    document: { documentElement: root, querySelectorAll: () => metas,
      getElementById: () => select, addEventListener: (_, fn) => { callbacks.ready = fn; } },
    localStorage: {
      getItem: () => { if (blocked) throw Error('blocked'); return stored; },
      setItem: (_, value) => { if (blocked) throw Error('blocked'); stored = value; },
      removeItem: () => { if (blocked) throw Error('blocked'); stored = null; },
    },
  });
  callbacks.ready();
  assert.equal(label.hidden, false);
  return { root, select, metas, stored: () => stored,
    choose(value) { select.value = value; callbacks.select(); },
    system(value) { media.matches = value; callbacks.system(); } };
}

const system = page();
assert.equal(system.select.value, 'system');
assert.equal(system.root.dataset.theme, undefined);
system.system(true);
assert.equal(system.metas[0].content, '#17191c');
system.choose('light');
assert.equal(system.root.dataset.theme, 'light');
system.system(false); system.system(true);
assert.equal(system.metas[0].content, '#f5f5f2');
assert.equal(page({ dark: true, saved: system.stored() }).select.value, 'light');
system.choose('dark');
assert.equal(system.root.dataset.theme, 'dark');
assert.equal(page({ saved: system.stored() }).metas[1].content, '#17191c');
system.choose('system');
assert.equal(system.stored(), null);
assert.equal(system.root.dataset.theme, undefined);
system.system(false);
assert.equal(system.metas[0].content, '#f5f5f2');
const blocked = page({ dark: true, blocked: true });
blocked.choose('light');
assert.equal(blocked.root.dataset.theme, 'light');
assert.equal(page({ saved: 'invalid' }).select.value, 'system');
console.log('Passed: system changes, overrides, reload persistence, reset and unavailable storage.');
