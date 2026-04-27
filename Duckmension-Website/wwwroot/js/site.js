// Theme toggle: clicking the floating "Toggle theme" button will switch between dark and light.
// Dark theme will set data-bs-theme="dark" on the <html> element so CSS rules (including
// the background override in site.css) take effect.

window.addEventListener('DOMContentLoaded', function () {
  var toggleBtn = document.getElementById('bd-theme');
  if (!toggleBtn) return;

  // Apply saved theme on load if present
  try {
    var saved = localStorage.getItem('theme');
    if (saved) document.documentElement.setAttribute('data-bs-theme', saved);
  } catch (e) {
    // ignore
  }

  toggleBtn.addEventListener('click', function () {
    var html = document.documentElement;
    var current = html.getAttribute('data-bs-theme');
    var next = current === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-bs-theme', next);
    try { localStorage.setItem('theme', next); } catch (e) { }

    // Update dropdown buttons state to reflect current selection if present
    var themeButtons = document.querySelectorAll('[data-bs-theme-value]');
    themeButtons.forEach(function (btn) {
      var v = btn.getAttribute('data-bs-theme-value');
      if (v === next) {
        btn.classList.add('active');
        btn.setAttribute('aria-pressed', 'true');
      } else {
        btn.classList.remove('active');
        btn.setAttribute('aria-pressed', 'false');
      }
    });
  });
});
