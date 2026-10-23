(() => {
  const root = document.documentElement;
  const system = window.matchMedia('(prefers-color-scheme: dark)');
  const key = 'kami-theme';
  let preference = 'system';
  try {
    const saved = localStorage.getItem(key);
    if (saved === 'light' || saved === 'dark') preference = saved;
  } catch { /* System theme still works when storage is unavailable. */ }

  function apply() {
    if (preference === 'system') root.removeAttribute('data-theme');
    else root.dataset.theme = preference;
    const dark = preference === 'dark' || (preference === 'system' && system.matches);
    document.querySelectorAll('meta[name="theme-color"]').forEach(meta => {
      meta.content = dark ? '#17191c' : '#f5f5f2';
    });
  }
  apply();
  system.addEventListener('change', apply);

  document.addEventListener('DOMContentLoaded', () => {
    const select = document.getElementById('theme');
    select.value = preference;
    select.closest('label').hidden = false;
    select.addEventListener('change', () => {
      preference = select.value;
      try {
        if (preference === 'system') localStorage.removeItem(key);
        else localStorage.setItem(key, preference);
      } catch { /* Keep the selection working for this page. */ }
      apply();
    });
  });
})();
