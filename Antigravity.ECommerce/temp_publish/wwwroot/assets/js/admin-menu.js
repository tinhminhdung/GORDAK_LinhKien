/**
 * Admin Sidebar Menu Active & Auto-Expand Logic
 * Autho: Antigravity
 */
$(document).ready(function () {
    var fullPath = (window.location.pathname + window.location.search).toLowerCase();
    var pathOnly = window.location.pathname.toLowerCase();

    // Normalize path: ignore trailing slash unless it's just "/"
    if (pathOnly.length > 1 && pathOnly.endsWith('/')) {
        pathOnly = pathOnly.substring(0, pathOnly.length - 1);
    }

    var bestMatch = null;
    var maxLength = -1;

    $("#navbar-nav a.nav-link").each(function () {
        var ahref = $(this).attr("href");
        if (!ahref || ahref === "#" || ahref.startsWith("javascript:")) return;
        var href = ahref.toLowerCase();

        var hrefOnly = href.split('?')[0];
        var hasQuery = href.indexOf('?') !== -1;

        // Normalize href to remove trailing /index for matching subpages (e.g. /AdminCategory/Create matches /AdminCategory/Index)
        var normalizedHref = hrefOnly.endsWith('/index') ? hrefOnly.substring(0, hrefOnly.length - 6) : hrefOnly;

        // 1. Exact match (including query params for things like Category?type=2)
        if (fullPath === href || (fullPath === "/admin" && href === "/admin")) {
            if (href.length + 100 > maxLength) {
                bestMatch = this;
                maxLength = href.length + 100; // Ưu tiên tuyệt đối
            }
        } 
        // 2. Sub-page match (Không có query string và khớp thư mục)
        else if (!hasQuery && href !== "/admin" && pathOnly.startsWith(normalizedHref)) {
            var nextChar = pathOnly.charAt(normalizedHref.length);
            if (nextChar === "/" || nextChar === "") {
                if (normalizedHref.length > maxLength) {
                    bestMatch = this;
                    maxLength = normalizedHref.length;
                }
            }
        }
        // 3. Sub-page match for links WITH query string (e.g. /AdminCategory?type=1 -> /AdminCategory/Create)
        else if (hasQuery && hrefOnly !== "/admin" && pathOnly.startsWith(normalizedHref)) {
            var nextChar2 = pathOnly.charAt(normalizedHref.length);
            if (nextChar2 === "/" || nextChar2 === "") {
                // Lower priority than exact match but still valid
                if (normalizedHref.length > maxLength) {
                    bestMatch = this;
                    maxLength = normalizedHref.length;
                }
            }
        }
    });

    if (bestMatch) {
        $(bestMatch).addClass("active");
        
        // Expand all parent levels
        $(bestMatch).parents(".collapse").each(function () {
            $(this).addClass("show");
            
            // Find the toggle button that opens this collapse
            var collapseId = $(this).attr("id");
            var parentToggle = $(`a[data-bs-target="#${collapseId}"], a[aria-controls="${collapseId}"]`);
            
            parentToggle.addClass("active");
            parentToggle.attr("aria-expanded", "true");
        });
        
        // If the element itself is a toggle
        if ($(bestMatch).attr("data-bs-toggle") === "collapse") {
             $(bestMatch).attr("aria-expanded", "true");
        }
    }
});
