// E-Store Premium Site JavaScript

// ===== Theme Management =====
(function initTheme() {
    const saved = localStorage.getItem('estore-theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const theme = saved || (prefersDark ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', theme);
})();

function toggleTheme() {
    const html = document.documentElement;
    const current = html.getAttribute('data-theme') || 'light';
    const next = current === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-theme', next);
    localStorage.setItem('estore-theme', next);

    // Update toggle icon with animation
    const icon = document.querySelector('.theme-toggle i');
    if (icon) {
        icon.style.transform = 'rotate(360deg) scale(0)';
        setTimeout(() => {
            icon.className = next === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
            icon.style.transform = 'rotate(0deg) scale(1)';
        }, 200);
    }
}

// ===== Safely escape HTML to prevent XSS =====
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ===== Update cart count badge via AJAX =====
function updateCartCount() {
    const isAuthenticated = document.body.dataset.authenticated === 'true';
    if (!isAuthenticated) return;

    const url = document.body.dataset.cartCountUrl;
    if (!url) return;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const badges = document.querySelectorAll('.cart-count-badge');
            badges.forEach(badge => {
                if (data.count > 0) {
                    badge.textContent = data.count;
                    badge.style.display = 'flex';
                } else {
                    badge.style.display = 'none';
                }
            });
        })
        .catch(() => { /* silently fail for unauthenticated users */ });
}

// ===== Show toast notification =====
function showNotification(message, type = 'success') {
    const safeMessage = escapeHtml(message);

    const colors = {
        success: 'linear-gradient(135deg, #10b981, #059669)',
        error: 'linear-gradient(135deg, #ef4444, #dc2626)',
        info: 'linear-gradient(135deg, #6366f1, #8b5cf6)'
    };

    const icons = {
        success: 'check-circle',
        error: 'exclamation-circle',
        info: 'info-circle'
    };

    // Remove existing notifications
    document.querySelectorAll('.estore-notification').forEach(n => n.remove());

    const notification = document.createElement('div');
    notification.className = 'estore-notification';
    notification.style.cssText = `
        position: fixed;
        top: 24px;
        right: 24px;
        padding: 16px 24px;
        background: ${colors[type] || colors.info};
        color: white;
        border-radius: 16px;
        box-shadow: 0 12px 40px rgba(0,0,0,0.2);
        z-index: 10000;
        font-weight: 600;
        font-size: 0.92rem;
        font-family: 'Inter', sans-serif;
        transform: translateX(120%);
        transition: transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
        max-width: 420px;
        backdrop-filter: blur(10px);
        display: flex;
        align-items: center;
        gap: 10px;
    `;
    notification.innerHTML = `<i class="fas fa-${icons[type] || icons.info}" style="font-size: 1.15rem;"></i><span>${safeMessage}</span>`;
    document.body.appendChild(notification);

    requestAnimationFrame(() => {
        notification.style.transform = 'translateX(0)';
    });

    setTimeout(() => {
        notification.style.transform = 'translateX(120%)';
        setTimeout(() => notification.remove(), 500);
    }, 3500);
}

// ===== AJAX Add to Cart =====
function addToCartAjax(productId) {
    const url = document.body.dataset.addToCartUrl;
    if (!url) return;

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, quantity: 1 })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message || 'Added to cart!');
            updateCartCount();
        } else {
            showNotification(data.message || 'Failed to add to cart', 'error');
        }
    })
    .catch(() => showNotification('Failed to add to cart', 'error'));
}

// ===== AJAX Add to Cart with quantity =====
function addToCartAjaxWithQuantity(productId, quantityInputId) {
    const qty = parseInt(document.getElementById(quantityInputId)?.value || '1');
    const url = document.body.dataset.addToCartUrl;
    if (!url) return;

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, quantity: qty })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message || 'Added to cart!');
            updateCartCount();
        } else {
            showNotification(data.message || 'Failed to add to cart', 'error');
        }
    })
    .catch(() => showNotification('Failed to add to cart', 'error'));
}

// ===== Scroll-based Navbar Enhancement =====
function initNavbarScroll() {
    const navbar = document.querySelector('.navbar');
    if (!navbar) return;

    window.addEventListener('scroll', () => {
        if (window.scrollY > 20) {
            navbar.style.boxShadow = '0 4px 20px rgba(0,0,0,0.1)';
        } else {
            navbar.style.boxShadow = 'var(--shadow-sm)';
        }
    }, { passive: true });
}

// ===== Intersection Observer for Animations =====
function initScrollAnimations() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

    document.querySelectorAll('.product-card, .stat-card, .order-card, .card').forEach((el, i) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = `opacity 0.5s ease ${i * 0.05}s, transform 0.5s ease ${i * 0.05}s`;
        observer.observe(el);
    });
}

// ===== Initialize on DOM load =====
document.addEventListener('DOMContentLoaded', function () {
    updateCartCount();
    initNavbarScroll();

    // Delay scroll animations slightly so page paint happens first
    setTimeout(initScrollAnimations, 100);

    // Set correct theme icon
    const theme = document.documentElement.getAttribute('data-theme') || 'light';
    const icon = document.querySelector('.theme-toggle i');
    if (icon) {
        icon.className = theme === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
    }
});
