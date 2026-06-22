/** @format */

// E-Store Premium Site JavaScript

// ===== Theme Management =====
(function initTheme() {
	const saved = localStorage.getItem("estore-theme");
	const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
	const theme = saved || (prefersDark ? "dark" : "light");
	document.documentElement.setAttribute("data-theme", theme);
})();

function toggleTheme() {
	const html = document.documentElement;
	const current = html.getAttribute("data-theme") || "light";
	const next = current === "dark" ? "light" : "dark";
	html.setAttribute("data-theme", next);
	localStorage.setItem("estore-theme", next);

	// Update toggle icon with animation
	const icon = document.querySelector(".theme-toggle i");
	if (icon) {
		icon.style.transform = "rotate(360deg) scale(0)";
		setTimeout(() => {
			icon.className = next === "dark" ? "fas fa-sun" : "fas fa-moon";
			icon.style.transform = "rotate(0deg) scale(1)";
		}, 200);
	}
}

// ===== Safely escape HTML to prevent XSS =====
function escapeHtml(text) {
	const div = document.createElement("div");
	div.textContent = text;
	return div.innerHTML;
}

// ===== Update cart count badge via AJAX =====
function updateCartCount() {
	const isAuthenticated = document.body.dataset.authenticated === "true";
	if (!isAuthenticated) return;

	const url = document.body.dataset.cartCountUrl;
	if (!url) return;

	fetch(url)
		.then((response) => response.json())
		.then((data) => {
			const badges = document.querySelectorAll(".cart-count-badge");
			badges.forEach((badge) => {
				if (data.count > 0) {
					badge.textContent = data.count;
					badge.style.display = "flex";
				} else {
					badge.style.display = "none";
				}
			});
		})
		.catch(() => {
			/* silently fail for unauthenticated users */
		});
}

// ===== Show toast notification (stacked with animated layout)
function showNotification(message, type = "success") {
	const safeMessage = escapeHtml(message);

	const colors = {
		success: "linear-gradient(135deg, #10b981, #059669)",
		error: "linear-gradient(135deg, #ef4444, #dc2626)",
		info: "linear-gradient(135deg, #6366f1, #8b5cf6)",
	};

	const icons = {
		success: "check-circle",
		error: "exclamation-circle",
		info: "info-circle",
	};

	// Ensure container
	let container = document.getElementById("estore-notifications");
	if (!container) {
		container = document.createElement("div");
		container.id = "estore-notifications";
		container.style.cssText = `
            position: fixed;
			top: 16px;
				right: 16px;
				display: flex;
				flex-direction: column;
				gap: 0px;
            align-items: flex-end;
            z-index: 10000;
            pointer-events: none;
        `;

		// Inject minimal styles for bounce animation (only once)
		if (!document.getElementById("estore-notification-styles")) {
			const style = document.createElement("style");
			style.id = "estore-notification-styles";
			style.innerHTML = `
			#estore-notifications .estore-notification.slide-in {
				animation: estore-slide-down-in 300ms cubic-bezier(.2, .9, .2, 1);
			}

			@keyframes estore-slide-down-in {
				0% { transform: translateY(-24px); opacity: 0 }
				100% { transform: translateY(0); opacity: 1 }
			}
			`;
			document.head.appendChild(style);
		}

		document.body.appendChild(container);
	}

	const notification = document.createElement("div");
	notification.className = "estore-notification";
	notification.style.cssText = `
        pointer-events: auto;
        padding: 12px 18px;
        background: ${colors[type] || colors.info};
        color: white;
        border-radius: 12px;
        box-shadow: 0 10px 28px rgba(0,0,0,0.18);
        font-weight: 600;
        font-size: 0.92rem;
        font-family: 'Inter', sans-serif;
        display: flex;
        align-items: center;
        gap: 10px;
        max-width: 420px;
		transform: translateY(-24px) scale(1);
		opacity: 0;
		transition: transform 360ms cubic-bezier(.2,1,.22,1), opacity 320ms ease;
    `;
	notification.innerHTML = `<i class="fas fa-${icons[type] || icons.info}" style="font-size: 1.15rem;"></i><span>${safeMessage}</span>`;

	// Mark new notification to slide down from top
	notification.classList.add("slide-in");

	// Insert newest on top
	container.insertBefore(notification, container.firstChild);

	// Layout existing notifications (move down, shrink, fade progressively)
	// Slide existing notifications down to make room (animated via transition)
	Array.from(container.children).forEach((child, index) => {
		const depth = index; // 0 is newest
		const translateY = depth * 36; // px spacing (reduced for overlap)
		const scale = Math.max(0.92, 1 - depth * 0.02);
		const opacity = Math.max(0.45, 1 - depth * 0.12);
		// ensure slide transition for older items
		child.style.transition =
			"transform 320ms cubic-bezier(.2,1,.22,1), opacity 280ms ease";
		child.style.transform = `translateY(${translateY}px) scale(${scale})`;
		child.style.opacity = opacity;
		child.style.zIndex = 10000 - index;
	});

	// Animate entry for the new one
	// Trigger layout then start slide-in animation
	requestAnimationFrame(() => {
		notification.classList.remove("slide-in");
		void notification.offsetWidth; // force reflow
		notification.classList.add("slide-in");
	});

	// Auto-dismiss with staggered fade: older ones will remain dimmed below
	const displayMs = 3500;
	setTimeout(() => {
		// Slide this notification back up out of view
		notification.style.transform = "translateY(-24px) scale(1)";
		notification.style.opacity = "0";
		setTimeout(() => {
			if (notification.parentElement) notification.remove();

			// Re-layout remaining notifications after removal
			Array.from(container.children).forEach((child, index) => {
				const translateY = index * 36;
				const scale = Math.max(0.92, 1 - index * 0.02);
				const opacity = Math.max(0.45, 1 - index * 0.12);
				child.style.transform = `translateY(${translateY}px) scale(${scale})`;
				child.style.opacity = opacity;
				child.style.zIndex = 10000 - index;
			});

			// If container is empty remove it
			if (!container.children.length) container.remove();
		}, 420);
	}, displayMs);
}

// ===== AJAX Add to Cart =====
function addToCartAjax(productId) {
	const url = document.body.dataset.addToCartUrl;
	if (!url) return;

	fetch(url, {
		method: "POST",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify({ productId: productId, quantity: 1 }),
	})
		.then((response) => response.json())
		.then((data) => {
			if (data.success) {
				showNotification(data.message || "Added to cart!");
				updateCartCount();
			} else {
				showNotification(data.message || "Failed to add to cart", "error");
			}
		})
		.catch(() => showNotification("Failed to add to cart", "error"));
}

// ===== AJAX Add to Cart with quantity =====
function addToCartAjaxWithQuantity(productId, quantityInputId) {
	const qty = parseInt(document.getElementById(quantityInputId)?.value || "1");
	const url = document.body.dataset.addToCartUrl;
	if (!url) return;

	fetch(url, {
		method: "POST",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify({ productId: productId, quantity: qty }),
	})
		.then((response) => response.json())
		.then((data) => {
			if (data.success) {
				showNotification(data.message || "Added to cart!");
				updateCartCount();
			} else {
				showNotification(data.message || "Failed to add to cart", "error");
			}
		})
		.catch(() => showNotification("Failed to add to cart", "error"));
}

// ===== Scroll-based Navbar Enhancement =====
function initNavbarScroll() {
	const navbar = document.querySelector(".navbar");
	if (!navbar) return;

	window.addEventListener(
		"scroll",
		() => {
			if (window.scrollY > 20) {
				navbar.style.boxShadow = "0 4px 20px rgba(0,0,0,0.1)";
			} else {
				navbar.style.boxShadow = "var(--shadow-sm)";
			}
		},
		{ passive: true },
	);
}

// ===== Intersection Observer for Animations =====
function initScrollAnimations() {
	const observer = new IntersectionObserver(
		(entries) => {
			entries.forEach((entry) => {
				if (entry.isIntersecting) {
					entry.target.style.opacity = "1";
					entry.target.style.transform = "translateY(0)";
					observer.unobserve(entry.target);
				}
			});
		},
		{ threshold: 0.1, rootMargin: "0px 0px -40px 0px" },
	);

	document
		.querySelectorAll(".product-card, .stat-card, .order-card, .card")
		.forEach((el, i) => {
			el.style.opacity = "0";
			el.style.transform = "translateY(20px)";
			el.style.transition = `opacity 0.5s ease ${i * 0.05}s, transform 0.5s ease ${i * 0.05}s`;
			observer.observe(el);
		});
}

// ===== Initialize on DOM load =====
document.addEventListener("DOMContentLoaded", function () {
	updateCartCount();
	initNavbarScroll();

	// Delay scroll animations slightly so page paint happens first
	setTimeout(initScrollAnimations, 100);

	// Set correct theme icon
	const theme = document.documentElement.getAttribute("data-theme") || "light";
	const icon = document.querySelector(".theme-toggle i");
	if (icon) {
		icon.className = theme === "dark" ? "fas fa-sun" : "fas fa-moon";
	}
});
