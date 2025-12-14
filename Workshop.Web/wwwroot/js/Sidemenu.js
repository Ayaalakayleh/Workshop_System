const sideNavMenu = document.getElementById("js-nav-menu");
const currentUrl = window.location.href.split(/[?#]/)[0]; // Match current URL
const allLinks = sideNavMenu.querySelectorAll("a");
const allCollapses = sideNavMenu.querySelectorAll("li [aria-expanded='true']"); // Collapses marked as expanded

// Prevent default toggle behavior for toggle links
sideNavMenu.querySelectorAll("li .has-ul").forEach((toggle) => {
    toggle.addEventListener("click", (e) => e.preventDefault());
});

// Ensure accordion behavior: only one collapse open at a time
allCollapses.forEach((collapse) => {
    collapse.addEventListener("show.bs.collapse", (event) => {
        const currentCollapse = event.target;

        // Get all ancestor collapses of the current item
        const ancestors = [];
        let el = currentCollapse.parentElement;
        while (el && el !== sideNavMenu) {
            if (el.classList.contains("collapse")) {
                ancestors.push(el);
            }
            el = el.parentElement;
        }

        allCollapses.forEach((other) => {
            if (other !== currentCollapse && !ancestors.includes(other)) {
                new bootstrap.Collapse(other, { toggle: false }).hide();
            }
        });
    });
});

allLinks.forEach((link) => {
    if (link.href === currentUrl) {
        // Mark link itself as active
        link.classList.add("active");

        // Traverse up to activate all parents and show collapses
        let currentElement = link.closest("li");
        while (currentElement && currentElement !== sideNavMenu) {
            // Mark <li> as active
            currentElement.classList.add("active");

            // Show parent collapses (div.collapse[role="menu"])
            const parentCollapse = currentElement.closest("[role='menu']");
            if (parentCollapse) {
                if (parentCollapse.classList.contains("collapse")) {
                    // Use Bootstrap Collapse if present
                    const bsInstance = bootstrap.Collapse.getOrCreateInstance(parentCollapse, { toggle: false });
                    bsInstance.show();

                    // Ensure it's flex, not hidden
                    parentCollapse.style.display = "flex";
                    parentCollapse.style.height = "auto";
                } else {
                    // Fallback generic container
                    parentCollapse.style.display = "flex";
                    parentCollapse.style.height = "auto";
                }

                // Also mark the <li> that contains the collapse as active
                const collapseParentLi = parentCollapse.closest("li");
                if (collapseParentLi) {
                    collapseParentLi.classList.add("active");
                }

                currentElement = collapseParentLi;
            } else {
                currentElement = currentElement.parentElement;
            }
        }
    }
});
