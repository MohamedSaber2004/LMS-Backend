/* Custom Swagger UI enhancement: single "Response code" navigator per endpoint.
 * Adds one dropdown listing the endpoint's status codes; picking a code
 * scrolls to and highlights that code's full native response block
 * (description, every switchable example, Schema tab, media type).
 * Nothing native is hidden or removed. Live "Try it out" results
 * (.live-responses-table) are never touched.
 *
 * Implementation notes:
 * - Everything is idempotent and re-applied on an interval, because React
 *   wipes foreign DOM nodes whenever an operation re-renders.
 */
(function () {
    'use strict';

    var POLL_MS = 800;
    var FLASH_MS = 1800;

    function rowCode(tr) {
        if (tr.getAttribute('data-code')) return tr.getAttribute('data-code');
        var statusCell = tr.querySelector('.response-col_status');
        return statusCell ? statusCell.textContent.trim().split(/\s/)[0] : '';
    }

    function findRow(table, code) {
        var rows = table.querySelectorAll('tbody tr.response');
        for (var i = 0; i < rows.length; i++) {
            if (rowCode(rows[i]) === code) return rows[i];
        }
        return null;
    }

    function buildNavigator(opblock) {
        var wrapper = opblock.querySelector('.opblock-body .responses-wrapper');
        if (!wrapper || wrapper.querySelector('.rc-nav')) return;

        var table = wrapper.querySelector('table.responses-table');
        if (!table) return;

        var codes = Array.prototype.slice
            .call(table.querySelectorAll('tbody tr.response'))
            .map(rowCode)
            .filter(function (c) { return c; });

        if (!codes.length) return;

        var nav = document.createElement('div');
        nav.className = 'rc-nav';

        var label = document.createElement('label');
        label.className = 'rc-label';
        label.textContent = 'Response code';

        var select = document.createElement('select');
        select.className = 'rc-select';
        codes.forEach(function (c) {
            var opt = document.createElement('option');
            opt.value = c;
            opt.textContent = c;
            select.appendChild(opt);
        });

        var flashTimer = null;
        select.addEventListener('change', function () {
            var row = findRow(table, select.value);
            if (!row) return;
            if (flashTimer) clearTimeout(flashTimer);
            table.querySelectorAll('tr.rc-flash').forEach(function (r) {
                r.classList.remove('rc-flash');
            });
            row.classList.add('rc-flash');
            try {
                row.scrollIntoView({ behavior: 'smooth', block: 'center' });
            } catch (e) {
                row.scrollIntoView();
            }
            flashTimer = setTimeout(function () {
                row.classList.remove('rc-flash');
            }, FLASH_MS);
        });

        nav.appendChild(label);
        nav.appendChild(select);

        var heading = wrapper.querySelector('h4');
        if (heading && heading.nextSibling) {
            heading.parentNode.insertBefore(nav, heading.nextSibling);
        } else {
            wrapper.insertBefore(nav, wrapper.firstChild);
        }
    }

    function run() {
        var root = document.getElementById('swagger-ui');
        if (!root) return;
        root.querySelectorAll('.opblock').forEach(buildNavigator);
    }

    var root = document.getElementById('swagger-ui');
    if (root && window.MutationObserver) {
        var debounced = null;
        new MutationObserver(function () {
            if (debounced) clearTimeout(debounced);
            debounced = setTimeout(run, 250);
        }).observe(root, { childList: true, subtree: true });
    }

    run();
    setInterval(run, POLL_MS);
})();
